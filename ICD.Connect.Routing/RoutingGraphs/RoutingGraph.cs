using System;
using System.Collections.Generic;
using System.Linq;
using ICD.Common.Properties;
using ICD.Common.Utils;
using ICD.Common.Utils.Collections;
using ICD.Common.Utils.Extensions;
using ICD.Common.Utils.Services.Logging;
using ICD.Connect.Devices;
using ICD.Connect.Devices.Extensions;
using ICD.Connect.Routing.Connections;
using ICD.Connect.Routing.Controls;
using ICD.Connect.Routing.Endpoints;
using ICD.Connect.Routing.Endpoints.Destinations;
using ICD.Connect.Routing.Endpoints.Sources;
using ICD.Connect.Routing.EventArguments;
using ICD.Connect.Routing.Groups.Endpoints.Destinations;
using ICD.Connect.Routing.Groups.Endpoints.Sources;
using ICD.Connect.Routing.RoutingCaches;
using ICD.Connect.Routing.StaticRoutes;
using ICD.Connect.Settings;
using ICD.Connect.Settings.Originators;

namespace ICD.Connect.Routing.RoutingGraphs
{
	/// <summary>
	/// Maps devices to each other via connections.
	/// </summary>
	[PublicAPI]
	public sealed class RoutingGraph : AbstractRoutingGraph<RoutingGraphSettings>
	{
		#region Events

		/// <summary>
		/// Raised when a route operation fails or succeeds.
		/// </summary>
		public override event EventHandler<RouteFinishedEventArgs> OnRouteFinished;

		/// <summary>
		/// Raised when a switcher changes routing.
		/// </summary>
		public override event EventHandler<SwitcherRouteChangeEventArgs> OnRouteChanged;

		/// <summary>
		/// Raised when a source device starts/stops sending video.
		/// </summary>
		public override event EventHandler<EndpointStateEventArgs> OnSourceTransmissionStateChanged;

		/// <summary>
		/// Raised when a source device is connected or disconnected.
		/// </summary>
		public override event EventHandler<EndpointStateEventArgs> OnSourceDetectionStateChanged;

		/// <summary>
		/// Raised when a destination device changes active input state.
		/// </summary>
		public override event EventHandler<EndpointStateEventArgs> OnDestinationInputActiveStateChanged;

		#endregion

		private readonly IcdHashSet<IRouteMidpointControl> m_SubscribedMidpoints;
		private readonly IcdHashSet<IRouteDestinationControl> m_SubscribedDestinationControls;
		private readonly IcdHashSet<IRouteSourceControl> m_SubscribedSourceControls;

		private readonly ConnectionsCollection m_Connections;
		private readonly StaticRoutesCollection m_StaticRoutes;
		private readonly CoreSourceCollection m_Sources;
		private readonly CoreDestinationCollection m_Destinations;
		private readonly CoreSourceGroupCollection m_SourceGroups;
		private readonly CoreDestinationGroupCollection m_DestinationGroups;

		private readonly SafeCriticalSection m_PendingRoutesSection;
		private readonly Dictionary<Guid, int> m_PendingRoutes;

		private readonly RoutingCache m_Cache;

		#region Properties

		/// <summary>
		/// Gets the connections collection.
		/// </summary>
		public override IConnectionsCollection Connections { get { return m_Connections; } }

		/// <summary>
		/// Gets the static routes collection.
		/// </summary>
		public override IOriginatorCollection<StaticRoute> StaticRoutes { get { return m_StaticRoutes; } }

		/// <summary>
		/// Gets the sources collection.
		/// </summary>
		public override ISourceCollection Sources { get { return m_Sources; } }

		/// <summary>
		/// Gets the destinations collection.
		/// </summary>
		public override IDestinationCollection Destinations { get { return m_Destinations; } }

		/// <summary>
		/// Gets the source groups collection.
		/// </summary>
		public override ISourceGroupCollection SourceGroups { get { return m_SourceGroups; } }

		/// <summary>
		/// Gets the destination groups collection.
		/// </summary>
		public override IDestinationGroupCollection DestinationGroups { get { return m_DestinationGroups; } }

		/// <summary>
		/// Gets the Routing Cache.
		/// </summary>
		public override RoutingCache RoutingCache { get { return m_Cache; } }

		#endregion

		#region Constructors

		/// <summary>
		/// Constructor.
		/// </summary>
		public RoutingGraph()
		{
			m_SubscribedMidpoints = new IcdHashSet<IRouteMidpointControl>();
			m_SubscribedDestinationControls = new IcdHashSet<IRouteDestinationControl>();
			m_SubscribedSourceControls = new IcdHashSet<IRouteSourceControl>();

			m_StaticRoutes = new StaticRoutesCollection(this);
			m_Connections = new ConnectionsCollection(this);
			m_Sources = new CoreSourceCollection();
			m_Destinations = new CoreDestinationCollection();
			m_SourceGroups = new CoreSourceGroupCollection();
			m_DestinationGroups = new CoreDestinationGroupCollection();

			m_PendingRoutes = new Dictionary<Guid, int>();
			m_PendingRoutesSection = new SafeCriticalSection();

			m_Cache = new RoutingCache(this);

			m_Connections.OnCollectionChanged += ConnectionsOnConnectionsChanged;
		}

		/// <summary>
		/// Override to release resources.
		/// </summary>
		/// <param name="disposing"></param>
		protected override void DisposeFinal(bool disposing)
		{
			OnRouteFinished = null;
			OnRouteChanged = null;
			OnSourceTransmissionStateChanged = null;
			OnSourceDetectionStateChanged = null;
			OnDestinationInputActiveStateChanged = null;

			base.DisposeFinal(disposing);

			m_Cache.Dispose();
		}

		/// <summary>
		/// Called when connections are added or removed.
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="eventArgs"></param>
		private void ConnectionsOnConnectionsChanged(object sender, EventArgs eventArgs)
		{
			SubscribeMidpoints();
			SubscribeDestinationControls();
			SubscribeSourceControls();

			m_StaticRoutes.UpdateStaticRoutes();

			m_Cache.RebuildCache();
		}

		#endregion

		#region Recursion

		/// <summary>
		/// Finds the actively routed sources for the destination.
		/// Will return multiple items when connection types are combined, e.g. seperate audio and video sources.
		/// </summary>
		/// <param name="destination"></param>
		/// <param name="type"></param>
		/// <param name="signalDetected">When true skips inputs where no video is detected.</param>
		/// <param name="inputActive"></param>
		/// <exception cref="ArgumentNullException"></exception>
		/// <returns>The sources</returns>
		public override IEnumerable<EndpointInfo> GetActiveSourceEndpoints(IDestination destination, eConnectionType type,
		                                                                   bool signalDetected, bool inputActive)
		{
			if (destination == null)
				throw new ArgumentNullException("destination");

			return m_Connections.FilterEndpointsAny(destination, type)
			                    .SelectMany(e => GetActiveSourceEndpoints(e, type, signalDetected, inputActive))
			                    .Distinct();
		}

		/// <summary>
		/// Finds the actively routed sources for the destination at the given input address.
		/// Will return multiple items when connection types are combined, e.g. seperate audio and video sources.
		/// </summary>
		/// <param name="destinationInput"></param>
		/// <param name="type"></param>
		/// <param name="signalDetected">When true skips inputs where no video is detected.</param>
		/// <param name="inputActive"></param>
		/// <exception cref="ArgumentNullException"></exception>
		/// <returns>The sources</returns>
		private IEnumerable<EndpointInfo> GetActiveSourceEndpoints(EndpointInfo destinationInput, eConnectionType type,
		                                                           bool signalDetected, bool inputActive)
		{
			IRouteDestinationControl destination = GetDestinationControl(destinationInput.Device, destinationInput.Control);
			if (destination == null)
				yield break;

			foreach (eConnectionType flag in EnumUtils.GetFlagsExceptNone(type))
			{
				EndpointInfo? endpoint = GetActiveSourceEndpoint(destination, destinationInput.Address, flag, signalDetected,
				                                                 inputActive);
				if (endpoint.HasValue)
					yield return endpoint.Value;
			}
		}

		/// <summary>
		/// Finds the actively routed source for the destination at the given input address.
		/// </summary>
		/// <param name="destination"></param>
		/// <param name="input"></param>
		/// <param name="flag"></param>
		/// <param name="signalDetected">When true skips inputs where no video is detected.</param>
		/// <param name="inputActive"></param>
		/// <exception cref="ArgumentNullException"></exception>
		/// <returns>The source</returns>
		public override EndpointInfo? GetActiveSourceEndpoint(IRouteDestinationControl destination, int input, eConnectionType flag, bool signalDetected, bool inputActive)
		{
			if (destination == null)
				throw new ArgumentNullException("destination");

			if (EnumUtils.HasMultipleFlags(flag))
				throw new ArgumentException("Connection type must be a single flag", "flag");

			// Prevent stack overflow
			IcdHashSet<KeyValuePair<IRouteSourceControl, int>> visited =
				new IcdHashSet<KeyValuePair<IRouteSourceControl, int>>();

			while (true)
			{
				if (signalDetected && !destination.GetSignalDetectedState(input, flag))
					return null;

				if (inputActive && !destination.GetInputActiveState(input, flag))
					return null;

				Connection inputConnection = m_Connections.GetInputConnection(destination, input);
				if (inputConnection == null)
					return null;

				// Narrow the type by what the connection supports
				if (!inputConnection.ConnectionType.HasFlag(flag))
					return null;

				IRouteSourceControl sourceControl = this.GetSourceControl(inputConnection);
				int sourceAddress = inputConnection.Source.Address;

				// Prevent stack overflow
				if (!visited.Add(new KeyValuePair<IRouteSourceControl, int>(sourceControl, sourceAddress)))
					return sourceControl.GetOutputEndpointInfo(sourceAddress);

				// The source has no inputs
				IRouteMidpointControl sourceAsMidpoint = sourceControl as IRouteMidpointControl;
				if (sourceAsMidpoint == null)
					return sourceControl.GetOutputEndpointInfo(sourceAddress);

				// No active path through the midpoint
				ConnectorInfo? sourceConnector = sourceAsMidpoint.GetInput(sourceAddress, flag);
				if (!sourceConnector.HasValue)
					return sourceControl.GetOutputEndpointInfo(sourceAddress);

				destination = sourceAsMidpoint;
				input = sourceConnector.Value.Address;
			}
		}

		/// <summary>
		/// Recurses over all of the source devices that can be routed to the destination.
		/// </summary>
		/// <param name="destinationControl"></param>
		/// <param name="type"></param>
		/// <returns></returns>
		public override IEnumerable<IRouteSourceControl> GetSourceControlsRecursive(
			IRouteDestinationControl destinationControl,
			eConnectionType type)
		{
			if (destinationControl == null)
				throw new ArgumentNullException("destinationControl");

			Queue<IRouteSourceControl> sources = new Queue<IRouteSourceControl>();
			sources.EnqueueRange(GetSourceControls(destinationControl, type));

			while (sources.Count > 0)
			{
				IRouteSourceControl source = sources.Dequeue();
				if (source == null)
					continue;

				yield return source;

				IRouteDestinationControl sourceAsDestination = source as IRouteDestinationControl;
				if (sourceAsDestination != null)
					sources.EnqueueRange(GetSourceControls(sourceAsDestination, type));
			}
		}

		/// <summary>
		/// Simple check to see if the source is detected by the next node in the graph.
		/// </summary>
		/// <param name="sourceControl"></param>
		/// <param name="type"></param>
		/// <returns></returns>
		[PublicAPI]
		public override bool SourceDetected(IRouteSourceControl sourceControl, eConnectionType type)
		{
			if (sourceControl == null)
				throw new ArgumentNullException("sourceControl");

			return Connections.GetOutputs(sourceControl, type).Any(o => SourceDetected(sourceControl, o, type));
		}

		/// <summary>
		/// Returns true if the source control is detected by the next node in the graph at the given output.
		/// </summary>
		/// <param name="sourceControl"></param>
		/// <param name="output"></param>
		/// <param name="type"></param>
		/// <returns></returns>
		[PublicAPI]
		public override bool SourceDetected(IRouteSourceControl sourceControl, int output, eConnectionType type)
		{
			if (sourceControl == null)
				throw new ArgumentNullException("sourceControl");

			int input;
			IRouteDestinationControl destination = GetDestinationControl(sourceControl, output, type, out input);
			return destination != null && destination.GetSignalDetectedState(input, type);
		}

		/// <summary>
		/// Returns true if the source is detected by the next node in the graph at the given output.
		/// </summary>
		/// <param name="sourceEndpoint"></param>
		/// <param name="type"></param>
		/// <returns></returns>
		public override bool SourceDetected(EndpointInfo sourceEndpoint, eConnectionType type)
		{
			IRouteSourceControl sourceControl = GetSourceControl(sourceEndpoint.Device, sourceEndpoint.Control);
			return sourceControl != null && SourceDetected(sourceControl, sourceEndpoint.Address, type);
		}

		/// <summary>
		/// Returns true if the given destination endpoint is active for all of the given connection types.
		/// </summary>
		/// <param name="endpoint"></param>
		/// <param name="type"></param>
		/// <returns></returns>
		public override bool InputActive(EndpointInfo endpoint, eConnectionType type)
		{
			IRouteDestinationControl destinationControl = GetDestinationControl(endpoint.Device, endpoint.Control);
			return destinationControl.GetInputActiveState(endpoint.Address, type);
		}

		/// <summary>
		/// Finds the current paths from the given source to the destination.
		/// Return multiple paths if multiple connection types are provided.
		/// </summary>
		/// <param name="source"></param>
		/// <param name="destination"></param>
		/// <param name="type"></param>
		/// <param name="signalDetected"></param>
		/// <param name="inputActive"></param>
		/// <returns></returns>
		public override IEnumerable<Connection[]> FindActivePaths(ISource source, IDestination destination,
		                                                          eConnectionType type, bool signalDetected,
		                                                          bool inputActive)
		{
			if (source == null)
				throw new ArgumentNullException("source");

			if (destination == null)
				throw new ArgumentNullException("destination");

			// Optimization - Only one of the source endpoints may be routed to the destination endpoint for the given type
			return Connections.FilterEndpointsAny(destination, type)
			                  .SelectMany(d => FindActivePaths(source, d, type, signalDetected, inputActive));
		}

		/// <summary>
		/// Finds the current paths from the given source to the destination.
		/// Return multiple paths if multiple connection types are provided.
		/// </summary>
		/// <param name="source"></param>
		/// <param name="destination"></param>
		/// <param name="type"></param>
		/// <param name="signalDetected"></param>
		/// <param name="inputActive"></param>
		/// <returns></returns>
		public override IEnumerable<Connection[]> FindActivePaths(EndpointInfo source, EndpointInfo destination,
		                                                          eConnectionType type, bool signalDetected,
		                                                          bool inputActive)
		{
			return EnumUtils.GetFlagsExceptNone(type)
			                .Select(f => FindActivePath(source, destination, f, signalDetected, inputActive))
							.Where(p => p != null);
		}

		/// <summary>
		/// Finds the current paths from the given source to the destination.
		/// Return multiple paths if multiple connection types are provided.
		/// </summary>
		/// <param name="source"></param>
		/// <param name="destination"></param>
		/// <param name="type"></param>
		/// <param name="signalDetected"></param>
		/// <param name="inputActive"></param>
		/// <returns></returns>
		public override IEnumerable<Connection[]> FindActivePaths(EndpointInfo source, IDestination destination,
		                                                          eConnectionType type, bool signalDetected,
		                                                          bool inputActive)
		{
			if (destination == null)
				throw new ArgumentNullException("destination");

			return Connections.FilterEndpointsAny(destination, type)
			                  .SelectMany(e => FindActivePaths(source, e, type, signalDetected, inputActive));
		}

		/// <summary>
		/// Finds the path currently routed to the destination.
		/// </summary>
		/// <param name="destination"></param>
		/// <param name="flag"></param>
		/// <param name="signalDetected"></param>
		/// <param name="inputActive"></param>
		/// <returns></returns>
		private Connection[] FindActivePath(EndpointInfo destination, eConnectionType flag, bool signalDetected, bool inputActive)
		{
			if (!EnumUtils.HasSingleFlag(flag))
				throw new ArgumentException("Type enum requires exactly 1 flag.", "flag");

			IRouteDestinationControl destinationControl = GetControl<IRouteDestinationControl>(destination.Device, destination.Control);
			if (destinationControl == null)
				return new Connection[0];

			List<Connection> result = new List<Connection>();

			while (true)
			{
				// If there is no input connection to this destination then we are done.
				Connection inputConnection = m_Connections.GetInputConnection(destination, flag);
				if (inputConnection == null)
					break;

				IRouteSourceControl sourceControl = this.GetSourceControl(inputConnection);

				// Ensure the source output even supports the given type.
				ConnectorInfo output = sourceControl.GetOutput(inputConnection.Source.Address);
				if (!output.ConnectionType.HasFlag(flag))
					break;

				// If we care about signal detection state, don't follow this path if the source isn't detected by the destination.
				if (signalDetected && !destinationControl.GetSignalDetectedState(inputConnection.Destination.Address, flag))
					break;

				// If we care about input active state, don't follow this path if the input isn't active on the destination.
				if (inputActive && !destinationControl.GetInputActiveState(inputConnection.Destination.Address, flag))
					break;

				result.Add(inputConnection);

				// Get the input address from the source if it is a midpoint device.
				IRouteMidpointControl midpointControl = sourceControl as IRouteMidpointControl;
				if (midpointControl == null)
					break;

				ConnectorInfo? input = midpointControl.GetInput(inputConnection.Source.Address, flag);
				if (input == null)
					break;

				// Loop
				destination = midpointControl.GetInputEndpointInfo(input.Value.Address);
				destinationControl = midpointControl;
			}

			result.Reverse();
			return result.ToArray(result.Count);
		}

		/// <summary>
		/// Finds the path currently routed from the source to the destination.
		/// </summary>
		/// <param name="source"></param>
		/// <param name="destination"></param>
		/// <param name="flag"></param>
		/// <param name="signalDetected"></param>
		/// <param name="inputActive"></param>
		/// <returns></returns>
		[CanBeNull]
		private Connection[] FindActivePath(EndpointInfo source, EndpointInfo destination, eConnectionType flag, bool signalDetected, bool inputActive)
		{
			if (!EnumUtils.HasSingleFlag(flag))
				throw new ArgumentException("Type enum requires exactly 1 flag.", "flag");

			Connection[] path = FindActivePath(destination, flag, signalDetected, inputActive);
			if (path == null)
				return null;

			int index = path.FindIndex(c => c.Source == source);

			return index < 0 ? null : path.Skip(index).ToArray();
		}

		/// <summary>
		/// Finds the current paths from the given source to the destination.
		/// Return multiple paths if multiple connection types are provided.
		/// </summary>
		/// <param name="source"></param>
		/// <param name="destination"></param>
		/// <param name="type"></param>
		/// <param name="signalDetected"></param>
		/// <param name="inputActive"></param>
		/// <returns></returns>
		public override IEnumerable<Connection[]> FindActivePaths(ISource source, EndpointInfo destination,
		                                                  eConnectionType type,
		                                                  bool signalDetected, bool inputActive)
		{
			if (source == null)
				throw new ArgumentNullException("source");

			// Optimization - Only one of the source endpoints may be routed to the destination endpoint for the given type
			return EnumUtils.GetFlagsExceptNone(type)
			                .Select(f => Connections.FilterEndpoints(source, f)
			                                        .Select(s => FindActivePath(s, destination, f, signalDetected, inputActive))
			                                        .FirstOrDefault(p => p != null))
			                .Where(p => p != null);
		}

		/// <summary>
		/// Finds all of the active paths from the given source.
		/// </summary>
		/// <param name="source"></param>
		/// <param name="type"></param>
		/// <param name="signalDetected"></param>
		/// <param name="inputActive"></param>
		/// <returns></returns>
		public override IEnumerable<Connection[]> FindActivePaths(EndpointInfo source, eConnectionType type,
		                                                          bool signalDetected, bool inputActive)
		{
			return EnumUtils.GetFlagsExceptNone(type)
			                .SelectMany(f => FindActivePathsSingleFlag(source, f, signalDetected, inputActive));
		}

		/// <summary>
		/// Finds all of the active paths from the given source.
		/// </summary>
		/// <param name="source"></param>
		/// <param name="type"></param>
		/// <param name="signalDetected"></param>
		/// <param name="inputActive"></param>
		/// <returns></returns>
		private IEnumerable<Connection[]> FindActivePathsSingleFlag(EndpointInfo source, eConnectionType type,
		                                                            bool signalDetected, bool inputActive)
		{
			return FindActivePathsSingleFlag(source, type, signalDetected, inputActive, new List<Connection>());
		}

		/// <summary>
		/// Finds all of the active paths from the given source.
		/// </summary>
		/// <param name="source"></param>
		/// <param name="type"></param>
		/// <param name="signalDetected"></param>
		/// <param name="inputActive"></param>
		/// <param name="visited"></param>
		/// <returns></returns>
		private IEnumerable<Connection[]> FindActivePathsSingleFlag(EndpointInfo source, eConnectionType type,
		                                                            bool signalDetected, bool inputActive,
		                                                            ICollection<Connection> visited)
		{
			// TODO - Optimize into a breadth first loop?

			if (visited == null)
				throw new ArgumentNullException("visited");

			if (!EnumUtils.HasSingleFlag(type))
				throw new ArgumentException("Type enum requires exactly 1 flag.", "type");

			// If there is no output connection from this source then we are done.
			Connection outputConnection = m_Connections.GetOutputConnection(source);
			if (outputConnection == null || !outputConnection.ConnectionType.HasFlag(type))
			{
				if (visited.Count > 0)
					yield return visited.ToArray(visited.Count);
				yield break;
			}

			IRouteDestinationControl destination = this.GetDestinationControl(outputConnection);

			// Ensure the destination input even supports the given type.
			ConnectorInfo input = destination.GetInput(outputConnection.Destination.Address);
			if (!input.ConnectionType.HasFlag(type))
			{
				if (visited.Count > 0)
					yield return visited.ToArray(visited.Count);
				yield break;
			}

			// If we care about signal detection state, don't follow this path if the source isn't detected by the destination.
			if (signalDetected && !destination.GetSignalDetectedState(outputConnection.Destination.Address, type))
			{
				if (visited.Count > 0)
					yield return visited.ToArray(visited.Count);
				yield break;
			}

			// If we care about input active state, don't follow this path if the input isn't active on the destination.
			if (inputActive && !destination.GetInputActiveState(outputConnection.Destination.Address, type))
			{
				if (visited.Count > 0)
					yield return visited.ToArray(visited.Count);
				yield break;
			}

			visited.Add(outputConnection);

			// Get the output addresses from the destination if it is a midpoint device.
			IRouteMidpointControl midpoint = destination as IRouteMidpointControl;
			if (midpoint == null)
			{
				if (visited.Count > 0)
					yield return visited.ToArray(visited.Count);
				yield break;
			}

			int[] outputs = midpoint.GetOutputs(outputConnection.Destination.Address, type)
			                        .Select(c => c.Address)
			                        .ToArray();

			if (outputs.Length == 0)
			{
				if (visited.Count > 0)
					yield return visited.ToArray(visited.Count);
				yield break;
			}

			// Recurse for each output.
			foreach (int outputAddress in outputs)
			{
				EndpointInfo newSource = midpoint.GetOutputEndpointInfo(outputAddress);

				IEnumerable<Connection[]> paths = FindActivePathsSingleFlag(newSource, type, signalDetected, inputActive,
				                                                            new List<Connection>(visited));
				foreach (Connection[] path in paths)
					yield return path;
			}
		}

		#endregion

		#region Routing

		/// <summary>
		/// Applies the given path to the switchers.
		/// </summary>
		/// <param name="path"></param>
		/// <param name="roomId"></param>
		public override void RoutePath(ConnectionPath path, int roomId)
		{
			if (path == null)
				throw new ArgumentNullException("path");

			RoutePaths(new[] {path}, roomId);
		}

		/// <summary>
		/// Applies the given paths to the switchers.
		/// </summary>
		/// <param name="paths"></param>
		/// <param name="roomId"></param>
		public override void RoutePaths(IEnumerable<ConnectionPath> paths, int roomId)
		{
			if (paths == null)
				throw new ArgumentNullException("paths");

			// Build a reduced set of switcher operations
			RouteOperationAggregator aggregator = new RouteOperationAggregator();

			foreach (ConnectionPath path in paths)
			{
				foreach (Connection[] pair in path.GetAdjacentPairs())
				{
					Connection connection = pair[0];

					IRouteSwitcherControl switcher = this.GetDestinationControl(connection) as IRouteSwitcherControl;
					if (switcher == null)
						continue;

					Connection nextConnection = pair[1];

					RouteOperation switchOperation = new RouteOperation
					{
						LocalInput = connection.Destination.Address,
						LocalOutput = nextConnection.Source.Address,
						LocalDevice = connection.Destination.Device,
						LocalControl = connection.Destination.Control,
						Source = path.SourceEndpoint,
						Destination = path.DestinationEndpoint,
						RoomId = roomId,
						ConnectionType = path.ConnectionType
					};

					aggregator.Add(switchOperation);
				}
			}

			// Apply the routes
			foreach (RouteOperation op in aggregator)
			{
				IRouteSwitcherControl switcher = GetControl<IRouteSwitcherControl>(op.LocalDevice, op.LocalControl);
				switcher.Route(op);

				RouteOperation opCopy = op;
				int pendingRoutes = m_PendingRoutesSection.Execute(() => m_PendingRoutes.GetDefault(opCopy.Id, 0));
				if (pendingRoutes > 0)
					continue;

				OnRouteFinished.Raise(this, new RouteFinishedEventArgs(op, true));
			}
		}

		/// <summary>
		/// Increments the number of pending routes for the given route operation
		/// </summary>
		/// <param name="op"></param>
		/// <returns></returns>
		public void PendingRouteStarted(RouteOperation op)
		{
			if (op == null)
				throw new ArgumentNullException("op");

			m_PendingRoutesSection.Enter();

			try
			{
				if (!m_PendingRoutes.ContainsKey(op.Id))
					m_PendingRoutes[op.Id] = 0;

				m_PendingRoutes[op.Id]++;
			}
			finally
			{
				m_PendingRoutesSection.Leave();
			}
		}

		/// <summary>
		/// Decrements the number of pending routes for the given route operation.
		/// If unsuccessful or all pending routes completed, raises the OnRouteFinished event.
		/// </summary>
		/// <param name="op"></param>
		/// <param name="success"></param>
		/// <returns></returns>
		public void PendingRouteFinished(RouteOperation op, bool success)
		{
			if (op == null)
				throw new ArgumentNullException("op");

			m_PendingRoutesSection.Enter();

			try
			{
				if (!m_PendingRoutes.ContainsKey(op.Id) || m_PendingRoutes[op.Id] <= 0)
					return;

				if (!success || m_PendingRoutes[op.Id] == 1)
				{
					m_PendingRoutes.Remove(op.Id);
					OnRouteFinished.Raise(this, new RouteFinishedEventArgs(op, success));
				}
				else
					m_PendingRoutes[op.Id]--;
			}
			finally
			{
				m_PendingRoutesSection.Leave();
			}
		}

		/// <summary>
		/// Searches for switchers currently routing the source to the destination and unroutes them.
		/// </summary>
		/// <param name="source"></param>
		/// <param name="destination"></param>
		/// <param name="type"></param>
		/// <param name="roomId"></param>
		/// <returns></returns>
		public override void Unroute(ISource source, IDestination destination, eConnectionType type, int roomId)
		{
			if (source == null)
				throw new ArgumentNullException("source");

			if (destination == null)
				throw new ArgumentNullException("destination");

			foreach (Connection[] path in FindActivePaths(source, destination, type, false, false))
				Unroute(path, type, roomId);
		}

		/// <summary>
		/// Searches for switchers currently routing the source to the destination and unroutes them.
		/// </summary>
		/// <param name="source"></param>
		/// <param name="destination"></param>
		/// <param name="type"></param>
		/// <param name="roomId"></param>
		/// <returns></returns>
		public override void Unroute(ISource source, EndpointInfo destination, eConnectionType type, int roomId)
		{
			if (source == null)
				throw new ArgumentNullException("source");

			foreach (Connection[] path in FindActivePaths(source, destination, type, false, false))
				Unroute(path, type, roomId);
		}

		/// <summary>
		/// Searches for switchers currently routing the source and unroutes them.
		/// </summary>
		/// <param name="sourceControl"></param>
		/// <param name="type"></param>
		/// <param name="roomId"></param>
		/// <returns></returns>
		public override void Unroute(IRouteSourceControl sourceControl, eConnectionType type, int roomId)
		{
			if (sourceControl == null)
				throw new ArgumentNullException("sourceControl");

			m_Connections.GetOutputsAny(sourceControl, type)
			             .ForEach(output => Unroute(sourceControl, output, type, roomId));
		}

		/// <summary>
		/// Searches for switchers currently routing the source and unroutes them.
		/// </summary>
		/// <param name="sourceControl"></param>
		/// <param name="sourceAddress"></param>
		/// <param name="type"></param>
		/// <param name="roomId"></param>
		private void Unroute(IRouteSourceControl sourceControl, int sourceAddress, eConnectionType type, int roomId)
		{
			if (sourceControl == null)
				throw new ArgumentNullException("sourceControl");

			EndpointInfo sourceEndpoint = sourceControl.GetOutputEndpointInfo(sourceAddress);

			Unroute(sourceEndpoint, type, roomId);
		}

		/// <summary>
		/// Searches for switchers currently routing the source to the destination and unroutes them.
		/// </summary>
		/// <param name="sourceControl"></param>
		/// <param name="sourceAddress"></param>
		/// <param name="destinationControl"></param>
		/// <param name="destinationAddress"></param>
		/// <param name="type"></param>
		/// <param name="roomId"></param>
		/// <returns>False if the devices could not be unrouted.</returns>
		private void Unroute([NotNull] IRouteSourceControl sourceControl, int sourceAddress,
		                     [NotNull] IRouteDestinationControl destinationControl,
		                     int destinationAddress, eConnectionType type, int roomId)
		{
			if (sourceControl == null)
				throw new ArgumentNullException("sourceControl");

			if (destinationControl == null)
				throw new ArgumentNullException("destinationControl");

			EndpointInfo sourceEndpoint = sourceControl.GetOutputEndpointInfo(sourceAddress);
			EndpointInfo destinationEndpoint = destinationControl.GetInputEndpointInfo(destinationAddress);

			Unroute(sourceEndpoint, destinationEndpoint, type, roomId);
		}

		/// <summary>
		/// Unroutes every path from the given source to the destination.
		/// </summary>
		/// <param name="sourceControl"></param>
		/// <param name="sourceAddress"></param>
		/// <param name="destinationControl"></param>
		/// <param name="type"></param>
		/// <param name="roomId"></param>
		/// <returns>False if the devices could not be unrouted.</returns>
		private void Unroute([NotNull] IRouteSourceControl sourceControl, int sourceAddress,
		                     [NotNull] IRouteDestinationControl destinationControl,
		                     eConnectionType type, int roomId)
		{
			if (sourceControl == null)
				throw new ArgumentNullException("sourceControl");

			if (destinationControl == null)
				throw new ArgumentNullException("destinationControl");

			m_Connections.GetInputsAny(destinationControl, type)
			             .ForEach(input => Unroute(sourceControl, sourceAddress, destinationControl, input, type, roomId));
		}

		/// <summary>
		/// Unroutes every path from the given source to the destination.
		/// </summary>
		/// <param name="sourceControl"></param>
		/// <param name="destinationControl"></param>
		/// <param name="type"></param>
		/// <param name="roomId"></param>
		/// <returns>False if the devices could not be unrouted.</returns>
		public override void Unroute([NotNull] IRouteSourceControl sourceControl, [NotNull] IRouteDestinationControl destinationControl,
		                             eConnectionType type, int roomId)
		{
			if (sourceControl == null)
				throw new ArgumentNullException("sourceControl");

			if (destinationControl == null)
				throw new ArgumentNullException("destinationControl");

			m_Connections.GetOutputsAny(sourceControl, type)
			             .ForEach(output => Unroute(sourceControl, output, destinationControl, type, roomId));
		}

		/// <summary>
		/// Searches for switchers currently routing the source and unroutes them.
		/// </summary>
		/// <param name="source"></param>
		/// <param name="type"></param>
		/// <param name="roomId"></param>
		private void Unroute(EndpointInfo source, eConnectionType type, int roomId)
		{
			foreach (Connection[] path in FindActivePaths(source, type, false, false))
			{
				// Loop backwards looking for switchers closest to the destination
				foreach (Connection[] pair in path.GetAdjacentPairs().Reverse())
				{
					Connection previous = pair[0];
					Connection current = pair[1];

					if (!Unroute(previous, current, type, roomId))
						break;
				}
			}
		}

		public override void Unroute(EndpointInfo source, EndpointInfo destination, eConnectionType type, int roomId)
		{
			foreach (Connection[] path in FindActivePaths(source, destination, type, false, false))
				Unroute(path, type, roomId);
		}

		/// <summary>
		/// Unroutes all switchers routing the active source to the given destination.
		/// </summary>
		/// <param name="destination"></param>
		/// <param name="type"></param>
		/// <param name="roomId"></param>
		public override void Unroute([NotNull] IDestination destination, eConnectionType type, int roomId)
		{
			if (destination == null)
				throw new ArgumentNullException("destination");

			foreach (EndpointInfo endpoint in Connections.FilterEndpointsAny(destination, type))
				UnrouteDestination(endpoint, type, roomId);
		}

		/// <summary>
		/// Unroutes all switchers routing the active source to the given destination.
		/// </summary>
		/// <param name="destination"></param>
		/// <param name="type"></param>
		/// <param name="roomId"></param>
		public override void UnrouteDestination(EndpointInfo destination, eConnectionType type, int roomId)
		{
			foreach (eConnectionType flag in EnumUtils.GetFlagsExceptNone(type))
			{
				Connection[] path = FindActivePath(destination, flag, false, false);
				Unroute(path, flag, roomId);
			}
		}

		/// <summary>
		/// Unroutes the given connection path.
		/// </summary>
		/// <param name="path"></param>
		/// <param name="type"></param>
		/// <param name="roomId"></param>
		public override void Unroute([NotNull] Connection[] path, eConnectionType type, int roomId)
		{
			if (path == null)
				throw new ArgumentNullException("path");

			// Loop backwards looking for switchers closest to the destination
			for (int index = path.Length - 1; index > 0; index--)
			{
				Connection previous = path[index - 1];
				Connection current = path[index];

				IRouteSwitcherControl switcher = this.GetSourceControl(current) as IRouteSwitcherControl;
				if (switcher == null)
					continue;

				if (!Unroute(switcher, previous.Destination.Address, current.Source.Address, type, roomId))
					break;

				// Stop unrouting if the input is routed to other outputs - we reached a fork
				int input = previous.Destination.Address;
				if (switcher.GetOutputs(input, type).Any())
					break;
			}
		}

		/// <summary>
		/// Unroutes the consecutive connections a -> b.
		/// </summary>
		/// <param name="incomingConnection"></param>
		/// <param name="outgoingConnection"></param>
		/// <param name="type"></param>
		/// <param name="roomId"></param>
		/// <returns>False if unauthorized to unroute the connections</returns>
		private bool Unroute([NotNull] Connection incomingConnection, [NotNull] Connection outgoingConnection, eConnectionType type, int roomId)
		{
			if (incomingConnection == null)
				throw new ArgumentNullException("incomingConnection");

			if (outgoingConnection == null)
				throw new ArgumentNullException("outgoingConnection");

			if (incomingConnection.Destination.Device != outgoingConnection.Source.Device || incomingConnection.Destination.Control != outgoingConnection.Source.Control)
				throw new InvalidOperationException("Connections are not consecutive");

			type = EnumUtils.GetFlagsIntersection(incomingConnection.ConnectionType, outgoingConnection.ConnectionType, type);

			IRouteSwitcherControl switcher = this.GetSourceControl(outgoingConnection) as IRouteSwitcherControl;
			if (switcher == null)
				return true;

			int input = incomingConnection.Destination.Address;
			int output = outgoingConnection.Source.Address;

			return Unroute(switcher, input, output, type, roomId);
		}

		/// <summary>
		/// Unroutes the consecutive connections a -> b.
		/// </summary>
		/// <param name="switcher"></param>
		/// <param name="input"></param>
		/// <param name="output"></param>
		/// <param name="type"></param>
		/// <param name="roomId"></param>
		/// <returns>False if unauthorized to unroute the connections</returns>
		private bool Unroute([NotNull] IRouteSwitcherControl switcher, int input, int output, eConnectionType type, int roomId)
		{
			if (switcher == null)
				throw new ArgumentNullException("switcher");

			foreach (eConnectionType flag in EnumUtils.GetFlagsExceptNone(type))
			{
				// Don't unroute if there is no path here
				ConnectorInfo? connector = switcher.GetInput(output, flag);
				if (!connector.HasValue || connector.Value.Address != input)
					continue;

				switcher.ClearOutput(output, flag);
			}

			return true;
		}

		#endregion

		#region Controls

		/// <summary>
		/// Gets the devices for the given connection.
		/// </summary>
		/// <param name="connection"></param>
		/// <returns></returns>
		public override IEnumerable<IRouteControl> GetControls(Connection connection)
		{
			if (connection == null)
				throw new ArgumentNullException("connection");

			yield return this.GetSourceControl(connection);
			yield return this.GetDestinationControl(connection);
		}

		/// <summary>
		/// Gets the control for the given device and control ids.
		/// </summary>
		/// <param name="device"></param>
		/// <param name="control"></param>
		/// <returns></returns>
		public override T GetControl<T>(int device, int control)
		{
			return Core.GetControl<T>(device, control);
		}

		public override IRouteDestinationControl GetDestinationControl(int device, int control)
		{
			return GetControl<IRouteDestinationControl>(device, control);
		}

		/// <summary>
		/// Gets the immediate destination device at the given address.
		/// </summary>
		/// <param name="sourceControl"></param>
		/// <param name="address"></param>
		/// <param name="type"></param>
		/// <param name="destinationInput"></param>
		/// <returns></returns>
		public override IRouteDestinationControl GetDestinationControl(IRouteSourceControl sourceControl, int address,
		                                                               eConnectionType type, out int destinationInput)
		{
			destinationInput = 0;

			if (sourceControl == null)
				throw new ArgumentNullException("sourceControl");

			Connection connection = m_Connections.GetOutputConnection(sourceControl, address);
			if (connection == null)
				return null;

			destinationInput = connection.Destination.Address;
			return this.GetDestinationControl(connection);
		}

		/// <summary>
		/// Gets the source control with the given device and control ids.
		/// </summary>
		/// <param name="device"></param>
		/// <param name="control"></param>
		/// <returns></returns>
		public override IRouteSourceControl GetSourceControl(int device, int control)
		{
			return GetControl<IRouteSourceControl>(device, control);
		}

		/// <summary>
		/// Returns the immediate source devices from [1 -> input count] inclusive, including nulls.
		/// </summary>
		/// <param name="destination"></param>
		/// <param name="type"></param>
		/// <returns></returns>
		private IEnumerable<IRouteSourceControl> GetSourceControls(IRouteDestinationControl destination,
		                                                                   eConnectionType type)
		{
			if (destination == null)
				throw new ArgumentNullException("destination");

			int unused;
			return Connections.GetInputs(destination, type)
			                  .Select(i => GetSourceControl(destination, i, type, out unused));
		}

		/// <summary>
		/// Gets the immediate source device at the given address.
		/// </summary>
		/// <param name="destination"></param>
		/// <param name="address"></param>
		/// <param name="type"></param>
		/// <param name="sourceOutput"></param>
		/// <returns></returns>
		public override IRouteSourceControl GetSourceControl(IRouteDestinationControl destination, int address,
		                                                     eConnectionType type, out int sourceOutput)
		{
			if (destination == null)
				throw new ArgumentNullException("destination");

			sourceOutput = 0;

			Connection connection = m_Connections.GetInputConnection(destination.GetInputEndpointInfo(address), type);
			if (connection == null)
				return null;

			sourceOutput = connection.Source.Address;
			return this.GetSourceControl(connection);
		}

		#endregion

		#region Destination Control Callbacks

		/// <summary>
		/// Unsubscribe from the previous destination controls and subscribe to the new destination control events.
		/// </summary>
		private void SubscribeDestinationControls()
		{
			UnsubscribeDestinationControls();

			m_SubscribedDestinationControls.AddRange(Connections.SelectMany(c => GetControls(c))
			                                             .OfType<IRouteDestinationControl>()
			                                             .Distinct());

			foreach (IRouteDestinationControl destination in m_SubscribedDestinationControls)
				Subscribe(destination);
		}

		/// <summary>
		/// Unsubscribe from the previous destination control events.
		/// </summary>
		private void UnsubscribeDestinationControls()
		{
			foreach (IRouteDestinationControl destinationControl in m_SubscribedDestinationControls)
				Unsubscribe(destinationControl);
			m_SubscribedDestinationControls.Clear();
		}

		/// <summary>
		/// Subscribe to the destination control events.
		/// </summary>
		/// <param name="destinationControl"></param>
		private void Subscribe(IRouteDestinationControl destinationControl)
		{
			if (destinationControl == null)
				return;

			destinationControl.OnSourceDetectionStateChange += DestinationControlOnSourceDetectionStateChange;
			destinationControl.OnActiveInputsChanged += DestinationControlOnActiveInputsChanged;
		}

		/// <summary>
		/// Unsubscribe from the destination control events.
		/// </summary>
		/// <param name="destinationControl"></param>
		private void Unsubscribe(IRouteDestinationControl destinationControl)
		{
			if (destinationControl == null)
				return;

			destinationControl.OnSourceDetectionStateChange -= DestinationControlOnSourceDetectionStateChange;
			destinationControl.OnActiveInputsChanged -= DestinationControlOnActiveInputsChanged;
		}

		/// <summary>
		/// Called when a destination input becomes active or inactive.
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="args"></param>
		private void DestinationControlOnActiveInputsChanged(object sender, ActiveInputStateChangeEventArgs args)
		{
			if (args == null)
				throw new ArgumentNullException("args");

			IRouteDestinationControl destination = sender as IRouteDestinationControl;
			if (destination == null)
				return;

			EndpointInfo endpoint = destination.GetInputEndpointInfo(args.Input);

			// If there's no connection to the input we don't care about it
			if (Connections.GetInputConnection(endpoint, args.Type) == null)
				return;

			OnDestinationInputActiveStateChanged.Raise(this, new EndpointStateEventArgs(endpoint, args.Type, args.Active));
		}

		/// <summary>
		/// Called when a destination control source detection state changes.
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="args"></param>
		private void DestinationControlOnSourceDetectionStateChange(object sender, SourceDetectionStateChangeEventArgs args)
		{
			if (args == null)
				throw new ArgumentNullException("args");

			IRouteDestinationControl destination = sender as IRouteDestinationControl;
			if (destination == null)
				return;

			int output;
			IRouteSourceControl source = GetSourceControl(destination, args.Input, args.Type, out output);

			// No source to detect
			if (source == null)
				return;

			EndpointInfo info = source.GetOutputEndpointInfo(output);
			OnSourceDetectionStateChanged.Raise(this, new EndpointStateEventArgs(info, args.Type, args.State));
		}

		#endregion

		#region Source Control Callbacks

		/// <summary>
		/// Unsubscribe from the previous source controls and subscribe to the current source events.
		/// </summary>
		private void SubscribeSourceControls()
		{
			UnsubscribeSourceControls();

			m_SubscribedSourceControls.AddRange(Connections.SelectMany(c => GetControls(c))
			                                               .OfType<IRouteSourceControl>()
			                                               .Distinct());

			foreach (IRouteSourceControl source in m_SubscribedSourceControls)
				Subscribe(source);
		}

		/// <summary>
		/// Unsubscribe from the previous source control events.
		/// </summary>
		private void UnsubscribeSourceControls()
		{
			foreach (IRouteSourceControl control in m_SubscribedSourceControls)
				Unsubscribe(control);
			m_SubscribedSourceControls.Clear();
		}

		/// <summary>
		/// Subscribe to the source control events.
		/// </summary>
		/// <param name="sourceControl"></param>
		private void Subscribe(IRouteSourceControl sourceControl)
		{
			if (sourceControl == null)
				return;

			sourceControl.OnActiveTransmissionStateChanged += SourceControlOnActiveTransmissionStateChanged;
		}

		/// <summary>
		/// Unsubscribe from the source control events.
		/// </summary>
		/// <param name="sourceControl"></param>
		private void Unsubscribe(IRouteSourceControl sourceControl)
		{
			if (sourceControl == null)
				return;

			sourceControl.OnActiveTransmissionStateChanged -= SourceControlOnActiveTransmissionStateChanged;
		}

		/// <summary>
		/// Called when a source control starts/stops sending video.
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="args"></param>
		private void SourceControlOnActiveTransmissionStateChanged(object sender, TransmissionStateEventArgs args)
		{
			if (args == null)
				throw new ArgumentNullException("args");

			IRouteSourceControl source = sender as IRouteSourceControl;
			if (source == null)
				return;

			EndpointInfo endpoint = source.GetOutputEndpointInfo(args.Output);

			// If there's no connection from the output we don't care about it
			if (Connections.GetOutputConnection(endpoint, args.Type) == null)
				return;

			OnSourceTransmissionStateChanged.Raise(this, new EndpointStateEventArgs(endpoint, args.Type, args.State));
		}

		#endregion

		#region Midpoint Callbacks

		/// <summary>
		/// Subscribes to the midpoints found in the connections.
		/// </summary>
		private void SubscribeMidpoints()
		{
			UnsubscribeMidpoints();

			m_SubscribedMidpoints.AddRange(Connections.SelectMany(c => GetControls(c))
			                                          .OfType<IRouteMidpointControl>()
			                                          .Distinct());

			foreach (IRouteMidpointControl midpoint in m_SubscribedMidpoints)
				Subscribe(midpoint);
		}

		/// <summary>
		/// Unsubscribes from the previously subscribed midpoints.
		/// </summary>
		private void UnsubscribeMidpoints()
		{
			foreach (IRouteMidpointControl midpoint in m_SubscribedMidpoints)
				Unsubscribe(midpoint);
			m_SubscribedMidpoints.Clear();
		}

		/// <summary>
		/// Subscribe to the midpoint events.
		/// </summary>
		/// <param name="midpoint"></param>
		private void Subscribe(IRouteMidpointControl midpoint)
		{
			if (midpoint == null)
				return;

			midpoint.OnRouteChange += MidpointOnRouteChange;
		}

		/// <summary>
		/// Unsubscribe from the midpoint events.
		/// </summary>
		/// <param name="midpoint"></param>
		private void Unsubscribe(IRouteMidpointControl midpoint)
		{
			if (midpoint == null)
				return;

			midpoint.OnRouteChange -= MidpointOnRouteChange;
		}

		/// <summary>
		/// Called when a midpoints routing changes.
		/// We want to ensure that static routes remain in place after routing changes.
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="args"></param>
		private void MidpointOnRouteChange(object sender, RouteChangeEventArgs args)
		{
			IRouteMidpointControl midpoint = sender as IRouteMidpointControl;
			if (midpoint == null)
				return;

			OnRouteChanged.Raise(this, new SwitcherRouteChangeEventArgs(midpoint, args));

			// Re-enforce static routes
			m_StaticRoutes.ReApplyStaticRoutesForMidpoint(midpoint);
		}

		#endregion

		#region Settings

		protected override void ClearSettingsFinal()
		{
			m_Connections.OnCollectionChanged -= ConnectionsOnConnectionsChanged;

			base.ClearSettingsFinal();

			m_Connections.SetChildren(Enumerable.Empty<Connection>());
			m_StaticRoutes.SetChildren(Enumerable.Empty<StaticRoute>());
			m_Sources.SetChildren(Enumerable.Empty<ISource>());
			m_Destinations.SetChildren(Enumerable.Empty<IDestination>());
			m_SourceGroups.SetChildren(Enumerable.Empty<ISourceGroup>());
			m_DestinationGroups.SetChildren(Enumerable.Empty<IDestinationGroup>());

			SubscribeMidpoints();
			SubscribeDestinationControls();
			SubscribeSourceControls();

			m_Connections.OnCollectionChanged += ConnectionsOnConnectionsChanged;
		}

		protected override void CopySettingsFinal(RoutingGraphSettings settings)
		{
			base.CopySettingsFinal(settings);

			settings.ConnectionSettings.SetRange(CopySerializableSettings(Connections));
			settings.StaticRouteSettings.SetRange(CopySerializableSettings(StaticRoutes));
			settings.SourceSettings.SetRange(CopySerializableSettings(Sources));
			settings.DestinationSettings.SetRange(CopySerializableSettings(Destinations));
			settings.SourceGroupSettings.SetRange(CopySerializableSettings(SourceGroups));
			settings.DestinationGroupSettings.SetRange(CopySerializableSettings(DestinationGroups));
		}

		private static IEnumerable<ISettings> CopySerializableSettings<TOriginator>(IEnumerable<TOriginator> collection)
			where TOriginator : class, IOriginator
		{
			if (collection == null)
				throw new ArgumentNullException("collection");

			return collection.Where(c => c.Serialize).Select(r => r.CopySettings());
		}

		protected override void ApplySettingsFinal(RoutingGraphSettings settings, IDeviceFactory factory)
		{
			m_Connections.OnCollectionChanged -= ConnectionsOnConnectionsChanged;

			// First load in all of the devices
			factory.LoadOriginators<IDeviceBase>();

			base.ApplySettingsFinal(settings, factory);

			// Populate the collections before moving on to loading the next types of originators
			IEnumerable<Connection> connections = GetOriginatorsSkipExceptions<Connection>(settings.ConnectionSettings, factory);
			m_Connections.SetChildren(connections);

			IEnumerable<StaticRoute> staticRoutes = GetOriginatorsSkipExceptions<StaticRoute>(settings.StaticRouteSettings, factory);
			m_StaticRoutes.SetChildren(staticRoutes);

			IEnumerable<ISource> sources = GetOriginatorsSkipExceptions<ISource>(settings.SourceSettings, factory);
			m_Sources.SetChildren(sources);

			IEnumerable<IDestination> destinations = GetOriginatorsSkipExceptions<IDestination>(settings.DestinationSettings, factory);
			m_Destinations.SetChildren(destinations);

			IEnumerable<ISourceGroup> sourceGroups = GetOriginatorsSkipExceptions<ISourceGroup>(settings.SourceGroupSettings, factory);
			m_SourceGroups.SetChildren(sourceGroups);

			IEnumerable<IDestinationGroup> destinationGroups = GetOriginatorsSkipExceptions<IDestinationGroup>(settings.DestinationGroupSettings, factory);
			m_DestinationGroups.SetChildren(destinationGroups);

			SubscribeMidpoints();
			SubscribeDestinationControls();
			SubscribeSourceControls();

			m_Cache.RebuildCache();

			m_Connections.OnCollectionChanged += ConnectionsOnConnectionsChanged;
		}

		private IEnumerable<T> GetOriginatorsSkipExceptions<T>(IEnumerable<ISettings> originatorSettings,
		                                                       IDeviceFactory factory)
			where T : class, IOriginator
		{
			foreach (ISettings settings in originatorSettings)
			{
				T output;

				try
				{
					output = factory.GetOriginatorById<T>(settings.Id);
				}
				catch (Exception e)
				{
					Logger.Log(eSeverity.Error, "Failed to instantiate {0} with id {1} - {2}", typeof(T).Name, settings.Id, e.Message);
					continue;
				}

				yield return output;
			}
		}

		#endregion
	}
}
