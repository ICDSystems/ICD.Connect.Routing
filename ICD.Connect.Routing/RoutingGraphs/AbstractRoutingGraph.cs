using System;
using System.Collections.Generic;
using ICD.Common.Utils.Services;
using ICD.Connect.API.Commands;
using ICD.Connect.API.Nodes;
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

namespace ICD.Connect.Routing.RoutingGraphs
{
	public abstract class AbstractRoutingGraph<TSettings> : AbstractOriginator<TSettings>, IRoutingGraph
		where TSettings : IRoutingGraphSettings, new()
	{
		public abstract event EventHandler<RouteFinishedEventArgs> OnRouteFinished;
		public abstract event EventHandler<SwitcherRouteChangeEventArgs> OnRouteChanged;
		public abstract event EventHandler<EndpointStateEventArgs> OnSourceTransmissionStateChanged;
		public abstract event EventHandler<EndpointStateEventArgs> OnSourceDetectionStateChanged;
		public abstract event EventHandler<EndpointStateEventArgs> OnDestinationInputActiveStateChanged;

		#region Properties

		public abstract IConnectionsCollection Connections { get; }
		public abstract IConnectionUsageCollection ConnectionUsages { get; }
		public abstract IOriginatorCollection<StaticRoute> StaticRoutes { get; }
		public abstract ISourceCollection Sources { get; }
		public abstract IDestinationCollection Destinations { get; }
		public abstract IOriginatorCollection<IDestinationGroup> DestinationGroups { get; }

		/// <summary>
		/// Gets the help information for the node.
		/// </summary>
		public override string ConsoleHelp { get { return "Maps the routing of device outputs to inputs."; } }

		#endregion

		/// <summary>
		/// Constructor.
		/// </summary>
		protected AbstractRoutingGraph()
		{
			ServiceProvider.AddService<IRoutingGraph>(this);
		}

		/// <summary>
		/// Override to release resources.
		/// </summary>
		/// <param name="disposing"></param>
		protected override void DisposeFinal(bool disposing)
		{
			base.DisposeFinal(disposing);

			ServiceProvider.RemoveService<IRoutingGraph>(this);
		}

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
		public abstract IEnumerable<EndpointInfo> GetActiveSourceEndpoints(IDestination destination, eConnectionType type,
		                                                                   bool signalDetected,
		                                                                   bool inputActive);

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
		public abstract IEnumerable<EndpointInfo> GetActiveSourceEndpoints(EndpointInfo destinationInput,
		                                                                   eConnectionType type,
		                                                                   bool signalDetected,
		                                                                   bool inputActive);

		/// <summary>
		/// Finds the actively routed source for the destination at the given input address.
		/// </summary>
		/// <param name="destination"></param>
		/// <param name="input"></param>
		/// <param name="type"></param>
		/// <param name="signalDetected">When true skips inputs where no video is detected.</param>
		/// <param name="inputActive"></param>
		/// <exception cref="ArgumentNullException"></exception>
		/// <returns>The source</returns>
		public abstract EndpointInfo? GetActiveSourceEndpoint(IRouteDestinationControl destination, int input,
		                                                      eConnectionType type,
		                                                      bool signalDetected, bool inputActive);

		/// <summary>
		/// Finds the actively routed source for the destination at the given input address.
		/// </summary>
		/// <param name="destination"></param>
		/// <param name="type"></param>
		/// <param name="signalDetected">When true skips inputs where no video is detected.</param>
		/// <param name="inputActive"></param>
		/// <exception cref="ArgumentNullException"></exception>
		/// <returns>The source</returns>
		public EndpointInfo? GetActiveSourceEndpoint(EndpointInfo destination, eConnectionType type, bool signalDetected,
		                                             bool inputActive)
		{
			IRouteDestinationControl control = GetDestinationControl(destination.Device, destination.Control);
			return GetActiveSourceEndpoint(control, destination.Address, type, signalDetected, inputActive);
		}

		/// <summary>
		/// Finds the destinations that the source is actively routed to.
		/// </summary>
		/// <param name="source"></param>
		/// <param name="type"></param>
		/// <param name="signalDetected">When true skips inputs where no video is detected.</param>
		/// <param name="inputActive"></param>
		/// <exception cref="ArgumentNullException"></exception>
		/// <returns>The sources</returns>
		public abstract IEnumerable<EndpointInfo> GetActiveDestinationEndpoints(ISource source, eConnectionType type,
		                                                                        bool signalDetected, bool inputActive);

		/// <summary>
		/// Finds the destinations that the source is actively routed to.
		/// </summary>
		/// <param name="sourceOutput"></param>
		/// <param name="type"></param>
		/// <param name="signalDetected">When true skips inputs where no video is detected.</param>
		/// <param name="inputActive"></param>
		/// <exception cref="ArgumentNullException"></exception>
		/// <returns>The sources</returns>
		public abstract IEnumerable<EndpointInfo> GetActiveDestinationEndpoints(EndpointInfo sourceOutput,
		                                                                        eConnectionType type, bool signalDetected,
		                                                                        bool inputActive);

		/// <summary>
		/// Finds the destinations that the source is actively routed to.
		/// </summary>
		/// <param name="sourceControl"></param>
		/// <param name="sourceOutput"></param>
		/// <param name="type"></param>
		/// <param name="signalDetected">When true skips inputs where no video is detected.</param>
		/// <param name="inputActive"></param>
		/// <exception cref="ArgumentNullException"></exception>
		/// <returns>The sources</returns>
		public abstract IEnumerable<EndpointInfo> GetActiveDestinationEndpoints(IRouteSourceControl sourceControl,
		                                                                        int sourceOutput, eConnectionType type,
		                                                                        bool signalDetected, bool inputActive);

		/// <summary>
		/// Recurses over all of the source devices that can be routed to the destination.
		/// </summary>
		/// <param name="destinationControl"></param>
		/// <param name="type"></param>
		/// <returns></returns>
		public abstract IEnumerable<IRouteSourceControl> GetSourceControlsRecursive(
			IRouteDestinationControl destinationControl, eConnectionType type);

		/// <summary>
		/// Simple check to see if the source is detected by the next node in the graph.
		/// </summary>
		/// <param name="sourceControl"></param>
		/// <param name="type"></param>
		/// <returns></returns>
		public abstract bool SourceDetected(IRouteSourceControl sourceControl, eConnectionType type);

		/// <summary>
		/// Returns true if the source is detected by the next node in the graph at the given output.
		/// </summary>
		/// <param name="sourceControl"></param>
		/// <param name="output"></param>
		/// <param name="type"></param>
		/// <returns></returns>
		public abstract bool SourceDetected(IRouteSourceControl sourceControl, int output, eConnectionType type);

		/// <summary>
		/// Returns true if there is a path from the given source to the given destination.
		/// </summary>
		/// <param name="source"></param>
		/// <param name="destination"></param>
		/// <param name="type"></param>
		/// <param name="roomId"></param>
		/// <returns></returns>
		public abstract bool HasPath(EndpointInfo source, EndpointInfo destination, eConnectionType type, int roomId);

		/// <summary>
		/// Finds the best available path from the source to the destination.
		/// </summary>
		/// <param name="source"></param>
		/// <param name="destination"></param>
		/// <param name="flag"></param>
		/// <param name="roomId"></param>
		/// <returns></returns>
		public abstract ConnectionPath FindPath(ISource source, IDestination destination, eConnectionType flag, int roomId);

		/// <summary>
		/// Finds the best available path from the source to the destination.
		/// </summary>
		/// <param name="sourceEndpoint"></param>
		/// <param name="destination"></param>
		/// <param name="flag"></param>
		/// <param name="roomId"></param>
		/// <returns></returns>
		public abstract ConnectionPath FindPath(EndpointInfo sourceEndpoint, IDestination destination, eConnectionType flag,
												int roomId);

		/// <summary>
		/// Finds the best available path from the source to the destination.
		/// </summary>
		/// <param name="source"></param>
		/// <param name="destinationEndpoint"></param>
		/// <param name="flag"></param>
		/// <param name="roomId"></param>
		/// <returns></returns>
		public abstract ConnectionPath FindPath(ISource source, EndpointInfo destinationEndpoint, eConnectionType flag,
		                                        int roomId);

		/// <summary>
		/// Finds the best available paths from the source to the destination.
		/// </summary>
		/// <param name="source"></param>
		/// <param name="destination"></param>
		/// <param name="type"></param>
		/// <param name="roomId"></param>
		/// <returns></returns>
		public abstract IEnumerable<ConnectionPath> FindPaths(ISource source, IDestination destination, eConnectionType type,
		                                                      int roomId);

		/// <summary>
		/// Finds the best available paths from the source to the destination.
		/// </summary>
		/// <param name="sourceEndpoint"></param>
		/// <param name="destination"></param>
		/// <param name="type"></param>
		/// <param name="roomId"></param>
		/// <returns></returns>
		public abstract IEnumerable<ConnectionPath> FindPaths(EndpointInfo sourceEndpoint, IDestination destination,
		                                                      eConnectionType type, int roomId);

		/// <summary>
		/// Finds the best available paths from the source to the destination.
		/// </summary>
		/// <param name="source"></param>
		/// <param name="destinationEndpoint"></param>
		/// <param name="type"></param>
		/// <param name="roomId"></param>
		/// <returns></returns>
		public abstract IEnumerable<ConnectionPath> FindPaths(ISource source, EndpointInfo destinationEndpoint,
		                                                      eConnectionType type, int roomId);

		/// <summary>
		/// Finds the best available paths from the source to the destination.
		/// </summary>
		/// <param name="source"></param>
		/// <param name="destination"></param>
		/// <param name="type"></param>
		/// <param name="roomId"></param>
		public abstract IEnumerable<ConnectionPath> FindPaths(EndpointInfo source, EndpointInfo destination,
		                                                      eConnectionType type, int roomId);

		/// <summary>
		/// Finds the shortest available path from the source to the destination.
		/// </summary>
		/// <param name="source"></param>
		/// <param name="destination"></param>
		/// <param name="flag"></param>
		/// <param name="roomId"></param>
		public abstract ConnectionPath FindPath(EndpointInfo source, EndpointInfo destination, eConnectionType flag,
		                                        int roomId);

		/// <summary>
		/// Returns the shortest paths from the source to the given destinations.
		/// </summary>
		/// <param name="source"></param>
		/// <param name="destinations"></param>
		/// <param name="flag"></param>
		/// <param name="roomId"></param>
		/// <returns></returns>
		public abstract IEnumerable<KeyValuePair<EndpointInfo, ConnectionPath>> FindPathsMulti(EndpointInfo source,
		                                                                                       IEnumerable<EndpointInfo>
			                                                                                       destinations,
		                                                                                       eConnectionType flag,
		                                                                                       int roomId);

		/// <summary>
		/// Returns the best available paths from the source to the given destinations.
		/// </summary>
		/// <param name="source"></param>
		/// <param name="destinations"></param>
		/// <param name="flag"></param>
		/// <param name="roomId"></param>
		/// <returns></returns>
		public abstract IEnumerable<KeyValuePair<EndpointInfo, ConnectionPath>> FindPathsMulti(ISource source,
		                                                                                       IEnumerable<IDestination>
			                                                                                       destinations,
		                                                                                       eConnectionType flag,
		                                                                                       int roomId);

		/// <summary>
		/// Finds all of the available paths from the source to the destination.
		/// </summary>
		/// <param name="source"></param>
		/// <param name="destination"></param>
		/// <param name="flag"></param>
		/// <param name="roomId"></param>
		/// <returns></returns>
		public abstract IEnumerable<ConnectionPath> FindAllPaths(ISource source, IDestination destination,
		                                                         eConnectionType flag, int roomId);

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
		public abstract IEnumerable<Connection[]> FindActivePaths(ISource source, IDestination destination,
		                                                          eConnectionType type, bool signalDetected,
		                                                          bool inputActive);

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
		public abstract IEnumerable<Connection[]> FindActivePaths(EndpointInfo source, EndpointInfo destination,
		                                                          eConnectionType type, bool signalDetected,
		                                                          bool inputActive);

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
		public abstract IEnumerable<Connection[]> FindActivePaths(EndpointInfo source, IDestination destination,
		                                                          eConnectionType type, bool signalDetected,
		                                                          bool inputActive);

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
		public abstract IEnumerable<Connection[]> FindActivePaths(ISource source, EndpointInfo destination,
		                                                          eConnectionType type, bool signalDetected,
		                                                          bool inputActive);

		/// <summary>
		/// Finds all of the active paths from the given source.
		/// </summary>
		/// <param name="source"></param>
		/// <param name="type"></param>
		/// <param name="signalDetected"></param>
		/// <param name="inputActive"></param>
		/// <returns></returns>
		public abstract IEnumerable<Connection[]> FindActivePaths(ISource source, eConnectionType type, bool signalDetected,
		                                                          bool inputActive);

		/// <summary>
		/// Finds all of the active paths from the given source.
		/// </summary>
		/// <param name="source"></param>
		/// <param name="type"></param>
		/// <param name="signalDetected"></param>
		/// <param name="inputActive"></param>
		/// <returns></returns>
		public abstract IEnumerable<Connection[]> FindActivePaths(EndpointInfo source, eConnectionType type,
		                                                          bool signalDetected, bool inputActive);

		#endregion

		#region Routing

		/// <summary>
		/// Routes the best available path from the source to the destination.
		/// </summary>
		/// <param name="source"></param>
		/// <param name="destination"></param>
		/// <param name="type"></param>
		/// <param name="roomId"></param>
		/// <returns>False if route could not be established</returns>
		public abstract void Route(ISource source, IDestination destination, eConnectionType type, int roomId);

		/// <summary>
		/// Routes the source to the destination.
		/// </summary>
		/// <param name="source"></param>
		/// <param name="destination"></param>
		/// <param name="type"></param>
		/// <param name="roomId"></param>
		/// <returns>False if route could not be established</returns>
		public abstract void Route(EndpointInfo source, EndpointInfo destination, eConnectionType type, int roomId);

		/// <summary>
		/// Routes the source to the destinations.
		/// </summary>
		/// <param name="source"></param>
		/// <param name="destinations"></param>
		/// <param name="type"></param>
		/// <param name="roomId"></param>
		public abstract void RouteMultiple(EndpointInfo source, IEnumerable<EndpointInfo> destinations, eConnectionType type,
		                                   int roomId);

		/// <summary>
		/// Applies the given path to the switchers.
		/// </summary>
		/// <param name="op"></param>
		/// <param name="path"></param>
		public abstract void RoutePath(RouteOperation op, IEnumerable<Connection> path);

		/// <summary>
		/// Applies the given path to the switchers.
		/// </summary>
		/// <param name="path"></param>
		/// <param name="roomId"></param>
		public abstract void RoutePath(ConnectionPath path, int roomId);

		/// <summary>
		/// Routes the source to the destination.
		/// </summary>
		/// <param name="sourceControl"></param>
		/// <param name="sourceAddress"></param>
		/// <param name="destinationControl"></param>
		/// <param name="destinationAddress"></param>
		/// <param name="type"></param>
		/// <param name="roomId"></param>
		/// <returns>False if route could not be established</returns>
		public abstract void Route(IRouteSourceControl sourceControl, int sourceAddress,
		                           IRouteDestinationControl destinationControl,
		                           int destinationAddress, eConnectionType type, int roomId);

		/// <summary>
		/// Performs the routing operation.
		/// </summary>
		/// <param name="op"></param>>
		/// <returns>False if route could not be established</returns>
		public abstract void Route(RouteOperation op);

		/// <summary>
		/// Searches for switchers currently routing the source to the destination and unroutes them.
		/// </summary>
		/// <param name="source"></param>
		/// <param name="destination"></param>
		/// <param name="type"></param>
		/// <param name="roomId"></param>
		/// <returns></returns>
		public abstract void Unroute(ISource source, IDestination destination, eConnectionType type, int roomId);

		/// <summary>
		/// Searches for switchers currently routing the source to the destination and unroutes them.
		/// </summary>
		/// <param name="source"></param>
		/// <param name="destination"></param>
		/// <param name="type"></param>
		/// <param name="roomId"></param>
		/// <returns></returns>
		public abstract void Unroute(EndpointInfo source, EndpointInfo destination, eConnectionType type, int roomId);

		/// <summary>
		/// Searches for switchers currently routing the source to the destination and unroutes them.
		/// </summary>
		/// <param name="source"></param>
		/// <param name="destination"></param>
		/// <param name="type"></param>
		/// <param name="roomId"></param>
		/// <returns></returns>
		public abstract void Unroute(ISource source, EndpointInfo destination, eConnectionType type, int roomId);

		/// <summary>
		/// Searches for switchers currently routing the source and unroutes them.
		/// </summary>
		/// <param name="sourceControl"></param>
		/// <param name="type"></param>
		/// <param name="roomId"></param>
		/// <returns></returns>
		public abstract void Unroute(IRouteSourceControl sourceControl, eConnectionType type, int roomId);

		/// <summary>
		/// Searches for switchers currently routing the source and unroutes them.
		/// </summary>
		/// <param name="sourceControl"></param>
		/// <param name="sourceAddress"></param>
		/// <param name="type"></param>
		/// <param name="roomId"></param>
		public abstract void Unroute(IRouteSourceControl sourceControl, int sourceAddress, eConnectionType type, int roomId);

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
		public abstract void Unroute(IRouteSourceControl sourceControl, int sourceAddress,
		                             IRouteDestinationControl destinationControl,
		                             int destinationAddress, eConnectionType type, int roomId);

		/// <summary>
		/// Unroutes every path from the given source to the destination.
		/// </summary>
		/// <param name="sourceControl"></param>
		/// <param name="sourceAddress"></param>
		/// <param name="destinationControl"></param>
		/// <param name="type"></param>
		/// <param name="roomId"></param>
		/// <returns>False if the devices could not be unrouted.</returns>
		public abstract void Unroute(IRouteSourceControl sourceControl, int sourceAddress,
		                             IRouteDestinationControl destinationControl,
		                             eConnectionType type, int roomId);

		/// <summary>
		/// Unroutes every path from the given source to the destination.
		/// </summary>
		/// <param name="sourceControl"></param>
		/// <param name="destinationControl"></param>
		/// <param name="type"></param>
		/// <param name="roomId"></param>
		/// <returns>False if the devices could not be unrouted.</returns>
		public abstract void Unroute(IRouteSourceControl sourceControl, IRouteDestinationControl destinationControl,
		                             eConnectionType type,
		                             int roomId);

		/// <summary>
		/// Unroutes the given connection path.
		/// </summary>
		/// <param name="path"></param>
		/// <param name="type"></param>
		/// <param name="roomId"></param>
		public abstract void Unroute(Connection[] path, eConnectionType type, int roomId);

		/// <summary>
		/// Unroutes all switchers routing the active source to the given destination.
		/// </summary>
		/// <param name="destination"></param>
		/// <param name="type"></param>
		/// <param name="roomId"></param>
		public abstract void Unroute(IDestination destination, eConnectionType type, int roomId);

		/// <summary>
		/// Unroutes all switchers routing the active source to the given destination.
		/// </summary>
		/// <param name="destination"></param>
		/// <param name="type"></param>
		/// <param name="roomId"></param>
		public abstract void UnrouteDestination(EndpointInfo destination, eConnectionType type, int roomId);

		#endregion

		#region Devices

		/// <summary>
		/// Gets the controls for the given connection.
		/// </summary>
		/// <param name="connection"></param>
		/// <returns></returns>
		public abstract IEnumerable<IRouteControl> GetControls(Connection connection);

		/// <summary>
		/// Gets the control for the given device and control ids.
		/// </summary>
		/// <param name="device"></param>
		/// <param name="control"></param>
		/// <returns></returns>
		public abstract T GetControl<T>(int device, int control) where T : IRouteControl;

		/// <summary>
		/// Gets the immediate destination control at the given address.
		/// </summary>
		/// <param name="sourceControl"></param>
		/// <param name="address"></param>
		/// <param name="type"></param>
		/// <param name="destinationInput"></param>
		/// <returns></returns>
		public abstract IRouteDestinationControl GetDestinationControl(IRouteSourceControl sourceControl, int address,
		                                                               eConnectionType type,
		                                                               out int destinationInput);

		public abstract IRouteDestinationControl GetDestinationControl(int device, int control);

		/// <summary>
		/// Returns the immediate source controls from [1 -> input count] inclusive, including nulls.
		/// </summary>
		/// <param name="destination"></param>
		/// <param name="type"></param>
		/// <returns></returns>
		public abstract IEnumerable<IRouteSourceControl> GetSourceControls(IRouteDestinationControl destination,
		                                                                   eConnectionType type);

		/// <summary>
		/// Gets the immediate source control at the given address.
		/// </summary>
		/// <param name="destination"></param>
		/// <param name="address"></param>
		/// <param name="type"></param>
		/// <param name="sourceOutput"></param>
		/// <returns></returns>
		public abstract IRouteSourceControl GetSourceControl(IRouteDestinationControl destination, int address,
		                                                     eConnectionType type,
		                                                     out int sourceOutput);

		/// <summary>
		/// Gets the source control with the given device and control ids.
		/// </summary>
		/// <param name="device"></param>
		/// <param name="control"></param>
		/// <returns></returns>
		public abstract IRouteSourceControl GetSourceControl(int device, int control);

		#endregion

		#region Console

		/// <summary>
		/// Gets the child console nodes.
		/// </summary>
		/// <returns></returns>
		public override IEnumerable<IConsoleNodeBase> GetConsoleNodes()
		{
			foreach (IConsoleNodeBase node in GetBaseConsoleNodes())
				yield return node;

			foreach (IConsoleNodeBase node in RoutingGraphConsole.GetConsoleNodes(this))
				yield return node;
		}

		/// <summary>
		/// Wrokaround for "unverifiable code" warning.
		/// </summary>
		/// <returns></returns>
		private IEnumerable<IConsoleNodeBase> GetBaseConsoleNodes()
		{
			return base.GetConsoleNodes();
		}

		/// <summary>
		/// Calls the delegate for each console status item.
		/// </summary>
		/// <param name="addRow"></param>
		public override void BuildConsoleStatus(AddStatusRowDelegate addRow)
		{
			base.BuildConsoleStatus(addRow);

			RoutingGraphConsole.BuildConsoleStatus(this, addRow);
		}

		/// <summary>
		/// Gets the child console commands.
		/// </summary>
		/// <returns></returns>
		public override IEnumerable<IConsoleCommand> GetConsoleCommands()
		{
			foreach (IConsoleCommand command in GetBaseConsoleCommands())
				yield return command;

			foreach (IConsoleCommand command in RoutingGraphConsole.GetConsoleCommands(this))
				yield return command;
		}

		/// <summary>
		/// Workaround for "unverifiable code" warning.
		/// </summary>
		/// <returns></returns>
		private IEnumerable<IConsoleCommand> GetBaseConsoleCommands()
		{
			return base.GetConsoleCommands();
		}

		#endregion
	}
}
