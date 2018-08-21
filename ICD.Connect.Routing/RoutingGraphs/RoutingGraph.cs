using System;
using System.Collections.Generic;
using System.Linq;
using ICD.Common.Properties;
using ICD.Common.Utils;
using ICD.Common.Utils.Collections;
using ICD.Common.Utils.Extensions;
using ICD.Common.Utils.Services;
using ICD.Common.Utils.Services.Logging;
using ICD.Connect.API.Nodes;
using ICD.Connect.Devices.Extensions;
using ICD.Connect.Routing.Connections;
using ICD.Connect.Routing.ConnectionUsage;
using ICD.Connect.Routing.Controls;
using ICD.Connect.Routing.Endpoints;
using ICD.Connect.Routing.Endpoints.Destinations;
using ICD.Connect.Routing.Endpoints.Groups;
using ICD.Connect.Routing.Endpoints.Sources;
using ICD.Connect.Routing.EventArguments;
using ICD.Connect.Routing.StaticRoutes;
using ICD.Connect.Settings;
using ICD.Connect.Settings.Core;

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

		private readonly IcdHashSet<IRouteSwitcherControl> m_SubscribedSwitchers;
		private readonly IcdHashSet<IRouteDestinationControl> m_SubscribedDestinations;
		private readonly IcdHashSet<IRouteSourceControl> m_SubscribedSources;

		private readonly ConnectionsCollection m_Connections;
		private readonly StaticRoutesCollection m_StaticRoutes;
		private readonly ConnectionUsageCollection m_ConnectionUsages;
		private readonly CoreSourceCollection m_Sources;
		private readonly CoreDestinationCollection m_Destinations;
		private readonly CoreDestinationGroupCollection m_DestinationGroups;

		private readonly SafeCriticalSection m_PendingRoutesSection;
		private readonly Dictionary<Guid, int> m_PendingRoutes;

		private readonly RoutingCache m_Cache;

		private ICore m_CachedCore;

		#region Properties

		public ICore Core { get { return m_CachedCore = m_CachedCore ?? ServiceProvider.GetService<ICore>(); } }

		/// <summary>
		/// Gets the connections collection.
		/// </summary>
		public override IConnectionsCollection Connections { get { return m_Connections; } }

		/// <summary>
		/// Gets the static routes collection.
		/// </summary>
		public override IOriginatorCollection<StaticRoute> StaticRoutes { get { return m_StaticRoutes; } }

		/// <summary>
		/// Gets the connection usages collection.
		/// </summary>
		public override IConnectionUsageCollection ConnectionUsages { get { return m_ConnectionUsages; } }

		/// <summary>
		/// Gets the sources collection.
		/// </summary>
		public override ISourceCollection Sources { get { return m_Sources; } }

		/// <summary>
		/// Gets the destinations collection.
		/// </summary>
		public override IDestinationCollection Destinations { get { return m_Destinations; } }

		/// <summary>
		/// Gets the destination groups collection.
		/// </summary>
		public override IOriginatorCollection<IDestinationGroup> DestinationGroups { get { return m_DestinationGroups; } }

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
			m_SubscribedSwitchers = new IcdHashSet<IRouteSwitcherControl>();
			m_SubscribedDestinations = new IcdHashSet<IRouteDestinationControl>();
			m_SubscribedSources = new IcdHashSet<IRouteSourceControl>();

			m_StaticRoutes = new StaticRoutesCollection(this);
			m_ConnectionUsages = new ConnectionUsageCollection(this);
			m_Connections = new ConnectionsCollection(this);
			m_Sources = new CoreSourceCollection();
			m_Destinations = new CoreDestinationCollection();
			m_DestinationGroups = new CoreDestinationGroupCollection();

			m_PendingRoutes = new Dictionary<Guid, int>();
			m_PendingRoutesSection = new SafeCriticalSection();

			m_Cache = new RoutingCache(this);

			m_Connections.OnChildrenChanged += ConnectionsOnConnectionsChanged;
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
			//ConnectionUsages.RemoveInvalid();

			SubscribeSwitchers();
			SubscribeDestinations();
			SubscribeSources();

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
		                                                                   bool signalDetected,
		                                                                   bool inputActive)
		{
			if (destination == null)
				throw new ArgumentNullException("destination");

			return destination.GetEndpoints()
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
		public override EndpointInfo? GetActiveSourceEndpoint(IRouteDestinationControl destination, int input,
		                                                      eConnectionType flag, bool signalDetected, bool inputActive)
		{
			if (destination == null)
				throw new ArgumentNullException("destination");

			if (EnumUtils.HasMultipleFlags(flag))
				throw new ArgumentException("Connection type must be a single flag", "flag");

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
			if (sourceControl == null)
				return null;

			IRouteMidpointControl sourceAsMidpoint = sourceControl as IRouteMidpointControl;
			if (sourceAsMidpoint == null)
				return sourceControl.GetOutputEndpointInfo(inputConnection.Source.Address);

			ConnectorInfo? sourceConnector = sourceAsMidpoint.GetInput(inputConnection.Source.Address, flag);
			return sourceConnector.HasValue
				       ? GetActiveSourceEndpoint(sourceAsMidpoint, sourceConnector.Value.Address, flag, signalDetected, inputActive)
				       : sourceControl.GetOutputEndpointInfo(inputConnection.Source.Address);
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

			List<Connection> result = new List<Connection>();

			IRouteDestinationControl destinationControl = GetControl<IRouteDestinationControl>(destination.Device, destination.Control);
			if (destinationControl == null)
				return result.ToArray(result.Count);

			while (true)
			{
				// If there is no input connection to this destination then we are done.
				Connection inputConnection = m_Connections.GetInputConnection(destination, flag);
				if (inputConnection == null)
					return result.ToArray(result.Count);

				IRouteSourceControl sourceControl = this.GetSourceControl(inputConnection);
				if (sourceControl == null)
					return result.ToArray(result.Count);

				// Ensure the source output even supports the given type.
				ConnectorInfo output = sourceControl.GetOutput(inputConnection.Source.Address);
				if (!output.ConnectionType.HasFlag(flag))
					return result.ToArray(result.Count);

				// If we care about signal detection state, don't follow this path if the source isn't detected by the destination.
				if (signalDetected && !destinationControl.GetSignalDetectedState(inputConnection.Destination.Address, flag))
					return result.ToArray(result.Count);

				// If we care about input active state, don't follow this path if the input isn't active on the destination.
				if (inputActive && !destinationControl.GetInputActiveState(inputConnection.Destination.Address, flag))
					return result.ToArray(result.Count);

				result.Insert(0, inputConnection);

				// Get the input address from the source if it is a midpoint device.
				IRouteMidpointControl midpointControl = sourceControl as IRouteMidpointControl;
				if (midpointControl == null)
					return result.ToArray(result.Count);

				ConnectorInfo? input = midpointControl.GetInput(inputConnection.Source.Address, flag);
				if (input == null)
					return result.ToArray(result.Count);

				// Loop
				destination = midpointControl.GetInputEndpointInfo(input.Value.Address);
				destinationControl = midpointControl;
			}
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
			if (index < 0)
				return null;

			return path.Skip(index).ToArray();
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
		private IEnumerable<Connection[]> FindActivePaths(ISource source, EndpointInfo destination,
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
		private IEnumerable<Connection[]> FindActivePaths(EndpointInfo source, eConnectionType type,
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
			if (destination == null)
			{
				if (visited.Count > 0)
					yield return visited.ToArray(visited.Count);
				yield break;
			}

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
					return;

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

			Unroute(sourceControl.GetOutputEndpointInfo(sourceAddress), type, roomId);
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
		private void Unroute(IRouteSourceControl sourceControl, int sourceAddress,
		                             IRouteDestinationControl destinationControl,
		                             int destinationAddress, eConnectionType type, int roomId)
		{
			if (sourceControl == null)
				throw new ArgumentNullException("sourceControl");

			if (destinationControl == null)
				throw new ArgumentNullException("destinationControl");

			Unroute(sourceControl.GetOutputEndpointInfo(sourceAddress),
			        destinationControl.GetInputEndpointInfo(destinationAddress),
			        type, roomId);
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
		private void Unroute(IRouteSourceControl sourceControl, int sourceAddress,
		                             IRouteDestinationControl destinationControl,
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
		public override void Unroute(IRouteSourceControl sourceControl, IRouteDestinationControl destinationControl,
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

		/// <summary>
		/// Searches for switchers currently routing the source to the destination and unroutes them.
		/// </summary>
		/// <param name="source"></param>
		/// <param name="destination"></param>
		/// <param name="type"></param>
		/// <param name="roomId"></param>
		/// <returns>False if the devices could not be unrouted.</returns>
		private void Unroute(EndpointInfo source, EndpointInfo destination, eConnectionType type, int roomId)
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
		public override void Unroute(IDestination destination, eConnectionType type, int roomId)
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
		private void UnrouteDestination(EndpointInfo destination, eConnectionType type, int roomId)
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
		public override void Unroute(Connection[] path, eConnectionType type, int roomId)
		{
			if (path == null)
				throw new ArgumentNullException("path");

			// Loop backwards looking for switchers closest to the destination
			for (int index = path.Length - 1; index > 0; index--)
			{
				Connection previous = path[index - 1];
				Connection current = path[index];

				IRouteMidpointControl midpoint = this.GetSourceControl(current) as IRouteMidpointControl;
				if (midpoint == null)
					continue;

				IRouteSwitcherControl switcher = midpoint as IRouteSwitcherControl;
				if (switcher == null)
					continue;

				if (!Unroute(previous, current, type, roomId))
					break;

				// Stop unrouting if the input is routed to other outputs - we reached a fork
				int input = previous.Destination.Address;
				if (midpoint.GetOutputs(input, type).Any())
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
		private bool Unroute(Connection incomingConnection, Connection outgoingConnection, eConnectionType type, int roomId)
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
		                                                     eConnectionType type,
		                                                     out int sourceOutput)
		{
			sourceOutput = 0;

			if (destination == null)
				throw new ArgumentNullException("destination");

			Connection connection = m_Connections.GetInputConnections(destination.Parent.Id, destination.Id, type)
			                                     .FirstOrDefault(c => c.Destination.Address == address);
			if (connection == null)
				return null;

			sourceOutput = connection.Source.Address;
			return this.GetSourceControl(connection);
		}

		#endregion

		#region Destination Callbacks

		/// <summary>
		/// Unsubscribe from the previous destination controls and subscribe to the new destination control events.
		/// </summary>
		private void SubscribeDestinations()
		{
			UnsubscribeDestinations();

			m_SubscribedDestinations.AddRange(Connections.SelectMany(c => GetControls(c))
			                                             .OfType<IRouteDestinationControl>()
			                                             .Distinct());

			foreach (IRouteDestinationControl destination in m_SubscribedDestinations)
				Subscribe(destination);
		}

		/// <summary>
		/// Unsubscribe from the previous destination control events.
		/// </summary>
		private void UnsubscribeDestinations()
		{
			foreach (IRouteDestinationControl destinationControl in m_SubscribedDestinations)
				Unsubscribe(destinationControl);
			m_SubscribedDestinations.Clear();
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
			if (source == null)
				return;

			EndpointInfo info = source.GetOutputEndpointInfo(output);
			OnSourceDetectionStateChanged.Raise(this, new EndpointStateEventArgs(info, args.Type, args.State));
		}

		#endregion

		#region Source Callbacks

		/// <summary>
		/// Unsubscribe from the previous source controls and subscribe to the current source events.
		/// </summary>
		private void SubscribeSources()
		{
			UnsubscribeSources();

			m_SubscribedSources.AddRange(Connections.SelectMany(c => GetControls(c))
			                                        .OfType<IRouteSourceControl>()
			                                        .Distinct());

			foreach (IRouteSourceControl source in m_SubscribedSources)
				Subscribe(source);
		}

		/// <summary>
		/// Unsubscribe from the previous source control events.
		/// </summary>
		private void UnsubscribeSources()
		{
			foreach (IRouteSourceControl control in m_SubscribedSources)
				Unsubscribe(control);
			m_SubscribedSources.Clear();
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
			OnSourceTransmissionStateChanged.Raise(this, new EndpointStateEventArgs(endpoint, args.Type, args.State));
		}

		#endregion

		#region Switcher Callbacks

		/// <summary>
		/// Subscribes to the switchers found in the connections.
		/// </summary>
		private void SubscribeSwitchers()
		{
			UnsubscribeSwitchers();

			m_SubscribedSwitchers.AddRange(Connections.SelectMany(c => GetControls(c))
			                                          .OfType<IRouteSwitcherControl>()
			                                          .Distinct());

			foreach (IRouteSwitcherControl switcher in m_SubscribedSwitchers)
				Subscribe(switcher);
		}

		/// <summary>
		/// Unsubscribes from the previously subscribed switchers.
		/// </summary>
		private void UnsubscribeSwitchers()
		{
			foreach (IRouteSwitcherControl switcher in m_SubscribedSwitchers)
				Unsubscribe(switcher);
			m_SubscribedSwitchers.Clear();
		}

		/// <summary>
		/// Subscribe to the switcher events.
		/// </summary>
		/// <param name="switcher"></param>
		private void Subscribe(IRouteSwitcherControl switcher)
		{
			if (switcher == null)
				return;

			switcher.OnRouteChange += SwitcherOnRouteChange;
		}

		/// <summary>
		/// Unsubscribe from the switcher events.
		/// </summary>
		/// <param name="switcher"></param>
		private void Unsubscribe(IRouteSwitcherControl switcher)
		{
			if (switcher == null)
				return;

			switcher.OnRouteChange -= SwitcherOnRouteChange;
		}

		/// <summary>
		/// Called when a switchers routing changes.
		/// We want to ensure that static routes remain in place after routing changes.
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="args"></param>
		private void SwitcherOnRouteChange(object sender, RouteChangeEventArgs args)
		{
			IRouteSwitcherControl switcher = sender as IRouteSwitcherControl;
			if (switcher == null)
				return;

			IcdHashSet<EndpointInfo> oldSources = new IcdHashSet<EndpointInfo>();
			IcdHashSet<EndpointInfo> newSources = new IcdHashSet<EndpointInfo>();

			if (args.OldInput.HasValue)
			{
				IEnumerable<EndpointInfo> sources =
					GetSourceEndpointsRecursive(switcher.GetInputEndpointInfo(args.OldInput.Value), args.Type)
					.Where(e => switcher.Parent.Id != e.Device);
				oldSources.AddRange(sources);
			}

			if (args.NewInput.HasValue)
			{
				IEnumerable<EndpointInfo> sources =
					GetSourceEndpointsRecursive(switcher.GetInputEndpointInfo(args.NewInput.Value), args.Type)
					.Where(e => switcher.Parent.Id != e.Device);
				newSources.AddRange(sources);
			}

			IcdHashSet<EndpointInfo> destinations =
				GetDestinationEndpoints(switcher.GetOutputEndpointInfo(args.Output), args.Type)
				.Where(e => switcher.Parent.Id != e.Device )
					.ToIcdHashSet();

			OnRouteChanged.Raise(this, new SwitcherRouteChangeEventArgs(switcher, args, oldSources, newSources, destinations));

			// Re-enforce static routes
			m_StaticRoutes.ReApplyStaticRoutesForSwitcher(switcher);
		}

		private IEnumerable<EndpointInfo> GetDestinationEndpoints(EndpointInfo outputEndpointInfo, eConnectionType type)
		{
			if (EnumUtils.HasMultipleFlags(type))
				throw new ArgumentException("Connection type must be a single flag", "type");

			IcdHashSet<EndpointInfo> destinations = new IcdHashSet<EndpointInfo>();

			Queue<EndpointInfo> process = new Queue<EndpointInfo>();
			process.Enqueue(outputEndpointInfo);

			while (process.Count > 0)
			{
				EndpointInfo current = process.Dequeue();

				// Grab the immediate destination for this source and add it to the hashset
				Connection connection = m_Connections.GetOutputConnection(current);
				if(connection == null || !connection.ConnectionType.HasFlag(type))
					continue;

				destinations.Add(connection.Destination);

				// If destination represents a midpoint device, find the outputs on that device and
				// push onto the end of the queue
				IRouteControl destinationControl = GetControl<IRouteControl>(connection.Destination.Device,
				                                                             connection.Destination.Control);

				if (!(destinationControl is IRouteMidpointControl))
					continue;

				IRouteMidpointControl midpointControl = destinationControl as IRouteMidpointControl;

				process.EnqueueRange(midpointControl.GetOutputs(connection.Destination.Address, type)
													.Where(c => c.ConnectionType.HasFlag(type))
				                                    .Select(c =>
				                                            new EndpointInfo(connection.Destination.Device,
				                                                             connection.Destination.Control, 
																			 c.Address)));
			}

			return destinations;
		}

		private IEnumerable<EndpointInfo> GetSourceEndpointsRecursive(EndpointInfo inputEndpointInfo, eConnectionType flag)
		{
			Connection inputConnection = m_Connections.GetInputConnection(inputEndpointInfo);
			if (inputConnection == null)
				yield break;

			// Narrow the type by what the connection supports
			if (!inputConnection.ConnectionType.HasFlag(flag))
				yield break;

			yield return inputConnection.Source;

			IRouteSourceControl sourceControl = this.GetSourceControl(inputConnection);
			if (sourceControl == null)
				yield break;

			IRouteMidpointControl sourceAsMidpoint = sourceControl as IRouteMidpointControl;
			if (sourceAsMidpoint == null)
				yield break;

			ConnectorInfo? sourceConnector = sourceAsMidpoint.GetInput(inputConnection.Source.Address, flag);
			if (sourceConnector == null)
				yield break;

			foreach (EndpointInfo endpoint in GetSourceEndpointsRecursive(sourceAsMidpoint.GetInputEndpointInfo(sourceConnector.Value.Address), flag))
				yield return endpoint;
		}

		#endregion

		#region Settings

		protected override void ClearSettingsFinal()
		{
			m_Connections.OnChildrenChanged -= ConnectionsOnConnectionsChanged;

			base.ClearSettingsFinal();

			m_Connections.SetChildren(Enumerable.Empty<Connection>());
			m_StaticRoutes.SetChildren(Enumerable.Empty<StaticRoute>());
			m_Sources.SetChildren(Enumerable.Empty<ISource>());
			m_Destinations.SetChildren(Enumerable.Empty<IDestination>());
			m_DestinationGroups.SetChildren(Enumerable.Empty<IDestinationGroup>());

			SubscribeSwitchers();
			SubscribeDestinations();
			SubscribeSources();

			m_Connections.OnChildrenChanged += ConnectionsOnConnectionsChanged;
		}

		protected override void ApplySettingsFinal(RoutingGraphSettings settings, IDeviceFactory factory)
		{
			m_Connections.OnChildrenChanged -= ConnectionsOnConnectionsChanged;

			base.ApplySettingsFinal(settings, factory);

			IEnumerable<Connection> connections = GetConnections(settings, factory);
			IEnumerable<StaticRoute> staticRoutes = GetStaticRoutes(settings, factory);
			IEnumerable<ISource> sources = GetSources(settings, factory);
			IEnumerable<IDestination> destinations = GetDestinations(settings, factory);
			IEnumerable<IDestinationGroup> destinationGroups = GetDestinationGroups(settings, factory);

			m_Connections.SetChildren(connections);
			m_StaticRoutes.SetChildren(staticRoutes);
			m_Sources.SetChildren(sources);
			m_Destinations.SetChildren(destinations);
			m_DestinationGroups.SetChildren(destinationGroups);

			SubscribeSwitchers();
			SubscribeDestinations();
			SubscribeSources();

			m_Connections.OnChildrenChanged += ConnectionsOnConnectionsChanged;

			m_Cache.RebuildCache();
		}

		private IEnumerable<StaticRoute> GetStaticRoutes(RoutingGraphSettings settings, IDeviceFactory factory)
		{
			return GetOriginatorsSkipExceptions<StaticRoute>(settings.StaticRouteSettings, factory);
		}

		private IEnumerable<ISource> GetSources(RoutingGraphSettings settings, IDeviceFactory factory)
		{
			return GetOriginatorsSkipExceptions<ISource>(settings.SourceSettings, factory);
		}

		private IEnumerable<IDestination> GetDestinations(RoutingGraphSettings settings, IDeviceFactory factory)
		{
			return GetOriginatorsSkipExceptions<IDestination>(settings.DestinationSettings, factory);
		}

		private IEnumerable<IDestinationGroup> GetDestinationGroups(RoutingGraphSettings settings, IDeviceFactory factory)
		{
			return GetOriginatorsSkipExceptions<IDestinationGroup>(settings.DestinationGroupSettings, factory);
		}

		private IEnumerable<Connection> GetConnections(RoutingGraphSettings settings, IDeviceFactory factory)
		{
			return GetOriginatorsSkipExceptions<Connection>(settings.ConnectionSettings, factory);
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
					Log(eSeverity.Error, "Failed to instantiate {0} with id {1} - {2}", typeof(T).Name, settings.Id, e.Message);
					continue;
				}

				yield return output;
			}
		}

		protected override void CopySettingsFinal(RoutingGraphSettings settings)
		{
			base.CopySettingsFinal(settings);

			settings.ConnectionSettings.SetRange(Connections.Where(c => c.Serialize)
			                                                .Select(r => r.CopySettings())
			                                                .Cast<ISettings>());
			settings.StaticRouteSettings.SetRange(StaticRoutes.Where(c => c.Serialize)
			                                                  .Select(r => r.CopySettings())
			                                                  .Cast<ISettings>());
			settings.SourceSettings.SetRange(Sources.Where(c => c.Serialize).Select(r => r.CopySettings()));
			settings.DestinationSettings.SetRange(Destinations.Where(c => c.Serialize).Select(r => r.CopySettings()));
			settings.DestinationGroupSettings.SetRange(DestinationGroups.Where(c => c.Serialize).Select(r => r.CopySettings()));
		}

		#endregion
		
		#region Console

		/// <summary>
		/// Gets the child console nodes.
		/// </summary>
		/// <returns></returns>
		public override IEnumerable<IConsoleNodeBase> GetConsoleNodes()
		{
			foreach (var node in GetBaseConsoleNodes())
			{
				yield return node;
			}

			yield return m_Cache;
		}

		private IEnumerable<IConsoleNodeBase> GetBaseConsoleNodes()
		{
			return base.GetConsoleNodes();
		} 

		#endregion
	}
}
