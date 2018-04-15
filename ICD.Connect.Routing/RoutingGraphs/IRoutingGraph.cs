using System;
using System.Collections.Generic;
using ICD.Common.Properties;
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
	public interface IRoutingGraph : IOriginator
	{
		#region Events

		/// <summary>
		/// Raised when a route operation fails or succeeds.
		/// </summary>
		event EventHandler<RouteFinishedEventArgs> OnRouteFinished;

		/// <summary>
		/// Raised when a switcher changes routing.
		/// </summary>
		event EventHandler<SwitcherRouteChangeEventArgs> OnRouteChanged;

		/// <summary>
		/// Raised when a source device starts/stops sending video.
		/// </summary>
		event EventHandler<EndpointStateEventArgs> OnSourceTransmissionStateChanged;

		/// <summary>
		/// Raised when a source device is connected or disconnected.
		/// </summary>
		event EventHandler<EndpointStateEventArgs> OnSourceDetectionStateChanged;

		/// <summary>
		/// Raised when a destination device changes active input state.
		/// </summary>
		event EventHandler<EndpointStateEventArgs> OnDestinationInputActiveStateChanged;

		#endregion

		#region Properties

		IConnectionsCollection Connections { get; }

		IConnectionUsageCollection ConnectionUsages { get; }

		IOriginatorCollection<StaticRoute> StaticRoutes { get; }

		ISourceCollection Sources { get; }

		IDestinationCollection Destinations { get; }

		IOriginatorCollection<IDestinationGroup> DestinationGroups { get; }

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
		IEnumerable<EndpointInfo> GetActiveSourceEndpoints(IDestination destination, eConnectionType type, bool signalDetected, bool inputActive);

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
		IEnumerable<EndpointInfo> GetActiveSourceEndpoints(EndpointInfo destinationInput, eConnectionType type,
		                                                   bool signalDetected, bool inputActive);

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
		EndpointInfo? GetActiveSourceEndpoint(IRouteDestinationControl destination, int input,
		                                      eConnectionType type, bool signalDetected, bool inputActive);

		/// <summary>
		/// Finds the actively routed source for the destination at the given input address.
		/// </summary>
		/// <param name="destination"></param>
		/// <param name="type"></param>
		/// <param name="signalDetected">When true skips inputs where no video is detected.</param>
		/// <param name="inputActive"></param>
		/// <exception cref="ArgumentNullException"></exception>
		/// <returns>The source</returns>
		EndpointInfo? GetActiveSourceEndpoint(EndpointInfo destination, eConnectionType type, bool signalDetected, bool inputActive);

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
		IEnumerable<EndpointInfo> GetActiveDestinationEndpoints(IRouteSourceControl sourceControl, int sourceOutput,
		                                                        eConnectionType type, bool signalDetected, bool inputActive);

		/// <summary>
		/// Recurses over all of the source devices that can be routed to the destination.
		/// </summary>
		/// <param name="destinationControl"></param>
		/// <param name="type"></param>
		/// <returns></returns>
		IEnumerable<IRouteSourceControl> GetSourceControlsRecursive(IRouteDestinationControl destinationControl,
		                                                            eConnectionType type);

		/// <summary>
		/// Simple check to see if the source is detected by the next node in the graph.
		/// </summary>
		/// <param name="sourceControl"></param>
		/// <param name="type"></param>
		/// <returns></returns>
		[PublicAPI]
		bool SourceDetected(IRouteSourceControl sourceControl, eConnectionType type);

		/// <summary>
		/// Returns true if the source is detected by the next node in the graph at the given output.
		/// </summary>
		/// <param name="sourceControl"></param>
		/// <param name="output"></param>
		/// <param name="type"></param>
		/// <returns></returns>
		[PublicAPI]
		bool SourceDetected(IRouteSourceControl sourceControl, int output, eConnectionType type);

		/// <summary>
		/// Returns true if there is a path from the given source to the given destination.
		/// </summary>
		/// <param name="source"></param>
		/// <param name="destination"></param>
		/// <param name="type"></param>
		/// <param name="roomId"></param>
		/// <returns></returns>
		bool HasPath(EndpointInfo source, EndpointInfo destination, eConnectionType type, int roomId);

		/// <summary>
		/// Finds the best available path from the source to the destination.
		/// </summary>
		/// <param name="source"></param>
		/// <param name="destination"></param>
		/// <param name="flag"></param>
		/// <param name="roomId"></param>
		/// <returns></returns>
		[CanBeNull]
		ConnectionPath FindPath(ISource source, IDestination destination, eConnectionType flag, int roomId);

		/// <summary>
		/// Finds the best available path from the source to the destination.
		/// </summary>
		/// <param name="sourceEndpoint"></param>
		/// <param name="destination"></param>
		/// <param name="flag"></param>
		/// <param name="roomId"></param>
		/// <returns></returns>
		[CanBeNull]
		ConnectionPath FindPath(EndpointInfo sourceEndpoint, IDestination destination, eConnectionType flag, int roomId);

		/// <summary>
		/// Finds the best available path from the source to the destination.
		/// </summary>
		/// <param name="source"></param>
		/// <param name="destinationEndpoint"></param>
		/// <param name="flag"></param>
		/// <param name="roomId"></param>
		/// <returns></returns>
		[CanBeNull]
		ConnectionPath FindPath(ISource source, EndpointInfo destinationEndpoint, eConnectionType flag, int roomId);

		/// <summary>
		/// Finds the shortest available path from the source to the destination.
		/// </summary>
		/// <param name="source"></param>
		/// <param name="destination"></param>
		/// <param name="flag"></param>
		/// <param name="roomId"></param>
		[CanBeNull]
		ConnectionPath FindPath(EndpointInfo source, EndpointInfo destination, eConnectionType flag, int roomId);

		/// <summary>
		/// Finds all of the available paths from the source to the destination.
		/// </summary>
		/// <param name="source"></param>
		/// <param name="destination"></param>
		/// <param name="flag"></param>
		/// <param name="roomId"></param>
		/// <returns></returns>
		IEnumerable<ConnectionPath> FindPaths(ISource source, IDestination destination, eConnectionType flag, int roomId);

		/// <summary>
		/// Returns the shortest paths from the source to the given destinations.
		/// </summary>
		/// <param name="source"></param>
		/// <param name="destinations"></param>
		/// <param name="flag"></param>
		/// <param name="roomId"></param>
		/// <returns></returns>
		[NotNull]
		IEnumerable<KeyValuePair<EndpointInfo, ConnectionPath>> FindPaths(
			EndpointInfo source,
			IEnumerable<EndpointInfo> destinations,
			eConnectionType flag,
			int roomId);

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
		IEnumerable<Connection[]> FindActivePaths(ISource source, IDestination destination, eConnectionType type,
		                                          bool signalDetected, bool inputActive);

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
		IEnumerable<Connection[]> FindActivePaths(EndpointInfo source, EndpointInfo destination, eConnectionType type,
		                                          bool signalDetected, bool inputActive);

		/// <summary>
		/// Finds all of the active paths from the given source.
		/// </summary>
		/// <param name="source"></param>
		/// <param name="type"></param>
		/// <param name="signalDetected"></param>
		/// <param name="inputActive"></param>
		/// <returns></returns>
		IEnumerable<Connection[]> FindActivePaths(ISource source, eConnectionType type, bool signalDetected,
		                                          bool inputActive);

		/// <summary>
		/// Finds all of the active paths from the given source.
		/// </summary>
		/// <param name="source"></param>
		/// <param name="type"></param>
		/// <param name="signalDetected"></param>
		/// <param name="inputActive"></param>
		/// <returns></returns>
		IEnumerable<Connection[]> FindActivePaths(EndpointInfo source, eConnectionType type, bool signalDetected,
		                                          bool inputActive);

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
		void Route(ISource source, IDestination destination, eConnectionType type, int roomId);

		/// <summary>
		/// Routes the source to the destination.
		/// </summary>
		/// <param name="source"></param>
		/// <param name="destination"></param>
		/// <param name="type"></param>
		/// <param name="roomId"></param>
		/// <returns>False if route could not be established</returns>
		void Route(EndpointInfo source, EndpointInfo destination, eConnectionType type, int roomId);

		/// <summary>
		/// Routes the source to the destinations.
		/// </summary>
		/// <param name="source"></param>
		/// <param name="destinations"></param>
		/// <param name="type"></param>
		/// <param name="roomId"></param>
		void RouteMultiple(EndpointInfo source, IEnumerable<EndpointInfo> destinations, eConnectionType type, int roomId);

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
		void Route(IRouteSourceControl sourceControl, int sourceAddress, IRouteDestinationControl destinationControl,
		           int destinationAddress, eConnectionType type, int roomId);

		/// <summary>
		/// Performs the routing operation.
		/// </summary>
		/// <param name="op"></param>>
		/// <returns>False if route could not be established</returns>
		void Route(RouteOperation op);

		/// <summary>
		/// Applies the given path to the switchers.
		/// </summary>
		/// <param name="op"></param>
		/// <param name="path"></param>
		void RoutePath(RouteOperation op, IEnumerable<Connection> path);

		/// <summary>
		/// Applies the given path to the switchers.
		/// </summary>
		/// <param name="path"></param>
		/// <param name="type"></param>
		/// <param name="roomId"></param>
		void RoutePath(ConnectionPath path, eConnectionType type, int roomId);

		/// <summary>
		/// Searches for switchers currently routing the source to the destination and unroutes them.
		/// </summary>
		/// <param name="source"></param>
		/// <param name="destination"></param>
		/// <param name="type"></param>
		/// <param name="roomId"></param>
		/// <returns></returns>
		void Unroute(ISource source, IDestination destination, eConnectionType type, int roomId);

		/// <summary>
		/// Searches for switchers currently routing the source to the destination and unroutes them.
		/// </summary>
		/// <param name="source"></param>
		/// <param name="destination"></param>
		/// <param name="type"></param>
		/// <param name="roomId"></param>
		/// <returns></returns>
		void Unroute(EndpointInfo source, EndpointInfo destination, eConnectionType type, int roomId);

		/// <summary>
		/// Searches for switchers currently routing the source and unroutes them.
		/// </summary>
		/// <param name="sourceControl"></param>
		/// <param name="type"></param>
		/// <param name="roomId"></param>
		/// <returns></returns>
		void Unroute(IRouteSourceControl sourceControl, eConnectionType type, int roomId);

		/// <summary>
		/// Searches for switchers currently routing the source and unroutes them.
		/// </summary>
		/// <param name="sourceControl"></param>
		/// <param name="sourceAddress"></param>
		/// <param name="type"></param>
		/// <param name="roomId"></param>
		void Unroute(IRouteSourceControl sourceControl, int sourceAddress, eConnectionType type, int roomId);

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
		void Unroute(IRouteSourceControl sourceControl, int sourceAddress, IRouteDestinationControl destinationControl,
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
		void Unroute(IRouteSourceControl sourceControl, int sourceAddress, IRouteDestinationControl destinationControl,
		             eConnectionType type, int roomId);

		/// <summary>
		/// Unroutes every path from the given source to the destination.
		/// </summary>
		/// <param name="sourceControl"></param>
		/// <param name="destinationControl"></param>
		/// <param name="type"></param>
		/// <param name="roomId"></param>
		/// <returns>False if the devices could not be unrouted.</returns>
		void Unroute(IRouteSourceControl sourceControl, IRouteDestinationControl destinationControl, eConnectionType type,
		             int roomId);

		/// <summary>
		/// Unroutes the given connection path.
		/// </summary>
		/// <param name="path"></param>
		/// <param name="type"></param>
		/// <param name="roomId"></param>
		void Unroute(Connection[] path, eConnectionType type, int roomId);

		/// <summary>
		/// Unroutes all switchers routing the active source to the given destination.
		/// </summary>
		/// <param name="destination"></param>
		/// <param name="type"></param>
		/// <param name="roomId"></param>
		void Unroute(IDestination destination, eConnectionType type, int roomId);

		/// <summary>
		/// Unroutes all switchers routing the active source to the given destination.
		/// </summary>
		/// <param name="destination"></param>
		/// <param name="type"></param>
		/// <param name="roomId"></param>
		void UnrouteDestination(EndpointInfo destination, eConnectionType type, int roomId);

		#endregion

		#region Devices

		/// <summary>
		/// Gets the controls for the given connection.
		/// </summary>
		/// <param name="connection"></param>
		/// <returns></returns>
		IEnumerable<IRouteControl> GetControls(Connection connection);

		/// <summary>
		/// Gets the control for the given device and control ids.
		/// </summary>
		/// <param name="device"></param>
		/// <param name="control"></param>
		/// <returns></returns>
		T GetControl<T>(int device, int control) where T : IRouteControl;

		/// <summary>
		/// Gets the immediate destination control at the given address.
		/// </summary>
		/// <param name="sourceControl"></param>
		/// <param name="address"></param>
		/// <param name="type"></param>
		/// <param name="destinationInput"></param>
		/// <returns></returns>
		[CanBeNull]
		IRouteDestinationControl GetDestinationControl(IRouteSourceControl sourceControl, int address, eConnectionType type,
		                                               out int destinationInput);

		IRouteDestinationControl GetDestinationControl(int device, int control);

		/// <summary>
		/// Returns the immediate source controls from [1 -> input count] inclusive, including nulls.
		/// </summary>
		/// <param name="destination"></param>
		/// <param name="type"></param>
		/// <returns></returns>
		IEnumerable<IRouteSourceControl> GetSourceControls(IRouteDestinationControl destination, eConnectionType type);

		/// <summary>
		/// Gets the immediate source control at the given address.
		/// </summary>
		/// <param name="destination"></param>
		/// <param name="address"></param>
		/// <param name="type"></param>
		/// <param name="sourceOutput"></param>
		/// <returns></returns>
		[CanBeNull]
		IRouteSourceControl GetSourceControl(IRouteDestinationControl destination, int address, eConnectionType type,
		                                     out int sourceOutput);

		/// <summary>
		/// Gets the source control with the given device and control ids.
		/// </summary>
		/// <param name="device"></param>
		/// <param name="control"></param>
		/// <returns></returns>
		IRouteSourceControl GetSourceControl(int device, int control);

		#endregion
	}

	public static class RoutingGraphExtensions
	{
		/// <summary>
		/// Gets the control for the given endpoint info.
		/// </summary>
		/// <param name="extends"></param>
		/// <param name="endpoint"></param>
		/// <returns></returns>
		public static T GetControl<T>(this IRoutingGraph extends, EndpointInfo endpoint)
			where T : IRouteControl
		{
			if (extends == null)
				throw new ArgumentNullException("extends");

			return extends.GetControl<T>(endpoint.Device, endpoint.Control);
		}

		/// <summary>
		/// Gets the destination control for the given destination.
		/// </summary>
		/// <param name="extends"></param>
		/// <param name="destination"></param>
		/// <returns></returns>
		public static IRouteDestinationControl GetDestinationControl(this IRoutingGraph extends, IDestination destination)
		{
			if (extends == null)
				throw new ArgumentNullException("extends");

			if (destination == null)
				throw new ArgumentNullException("destination");

			return extends.GetDestinationControl(destination.Device, destination.Control);
		}

		/// <summary>
		/// Gets the destination device for the given connection.
		/// </summary>
		/// <param name="extends"></param>
		/// <param name="connection"></param>
		/// <returns></returns>
		public static IRouteDestinationControl GetDestinationControl(this IRoutingGraph extends, Connection connection)
		{
			if (extends == null)
				throw new ArgumentNullException("extends");

			if (connection == null)
				throw new ArgumentNullException("connection");

			return extends.GetDestinationControl(connection.Destination);
		}

		public static IRouteDestinationControl GetDestinationControl(this IRoutingGraph extends, EndpointInfo endpoint)
		{
			if (extends == null)
				throw new ArgumentNullException("extends");

			return extends.GetDestinationControl(endpoint.Device, endpoint.Control);
		}

		/// <summary>
		/// Gets the source control for the given source.
		/// </summary>
		/// <param name="extends"></param>
		/// <param name="source"></param>
		/// <returns></returns>
		public static IRouteSourceControl GetSourceControl(this IRoutingGraph extends, ISource source)
		{
			if (extends == null)
				throw new ArgumentNullException("extends");

			if (source == null)
				throw new ArgumentNullException("source");

			return extends.GetSourceControl(source.Device, source.Control);
		}

		/// <summary>
		/// Gets the source device for the given connection.
		/// </summary>
		/// <param name="extends"></param>
		/// <param name="connection"></param>
		/// <returns></returns>
		public static IRouteSourceControl GetSourceControl(this IRoutingGraph extends, Connection connection)
		{
			if (extends == null)
				throw new ArgumentNullException("extends");

			if (connection == null)
				throw new ArgumentNullException("connection");

			return extends.GetSourceControl(connection.Source);
		}

		/// <summary>
		/// Gets the source for the given endpoint.
		/// </summary>
		/// <param name="extends"></param>
		/// <param name="endpoint"></param>
		/// <returns></returns>
		public static IRouteSourceControl GetSourceControl(this IRoutingGraph extends, EndpointInfo endpoint)
		{
			if (extends == null)
				throw new ArgumentNullException("extends");

			return extends.GetSourceControl(endpoint.Device, endpoint.Control);
		}
	}
}
