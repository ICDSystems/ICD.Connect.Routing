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

namespace ICD.Connect.Routing
{
	public interface IRoutingGraph : IOriginator
	{
		#region Events

		event EventHandler<RouteFinishedEventArgs> OnRouteFinished;

		#endregion

		#region Properties

		IConnectionsCollection Connections { get; }

		IConnectionUsageCollection ConnectionUsages { get; }

		IStaticRoutesCollection StaticRoutes { get; }

		IOriginatorCollection<ISource> Sources { get; }

		IOriginatorCollection<IDestination> Destinations { get; }

		IOriginatorCollection<IDestinationGroup> DestinationGroups { get; }

		#endregion

		#region Recursion

		/// <summary>
		/// Finds the actively routed sources for the destination at the given input address.
		/// Will return multiple items when connection types are combined, e.g. seperate audio and video sources.
		/// </summary>
		/// <param name="destinationInput"></param>
		/// <param name="type"></param>
		/// <param name="signalDetected">When true skips inputs where no video is detected.</param>
		/// <exception cref="ArgumentNullException"></exception>
		/// <returns>The sources</returns>
		IEnumerable<EndpointInfo> GetActiveSourceEndpoints(EndpointInfo destinationInput, eConnectionType type,
		                                                   bool signalDetected);

		/// <summary>
		/// Finds the destinations that the source is actively routed to.
		/// </summary>
		/// <param name="sourceControl"></param>
		/// <param name="sourceOutput"></param>
		/// <param name="type"></param>
		/// <param name="signalDetected">When true skips inputs where no video is detected.</param>
		/// <exception cref="ArgumentNullException"></exception>
		/// <returns>The sources</returns>
		IEnumerable<EndpointInfo> GetActiveDestinationEndpoints(IRouteSourceControl sourceControl, int sourceOutput,
		                                                        eConnectionType type, bool signalDetected);

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

		#endregion

		#region Routing

		/// <summary>
		/// Routes the source to the destination.
		/// </summary>
		/// <param name="source"></param>
		/// <param name="destination"></param>
		/// <param name="type"></param>
		/// <param name="roomId"></param>
		/// <returns>False if route could not be established</returns>
		bool Route(EndpointInfo source, EndpointInfo destination, eConnectionType type, int roomId);

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
		bool Route(IRouteSourceControl sourceControl, int sourceAddress, IRouteDestinationControl destinationControl,
		           int destinationAddress, eConnectionType type, int roomId);

		/// <summary>
		/// Performs the routing operation.
		/// </summary>
		/// <param name="op"></param>>
		/// <returns>False if route could not be established</returns>
		bool Route(RouteOperation op);

		/// <summary>
		/// Searches for switchers currently routing the source to the destination and unroutes them.
		/// </summary>
		/// <param name="source"></param>
		/// <param name="destination"></param>
		/// <param name="type"></param>
		/// <param name="roomId"></param>
		/// <returns></returns>
		bool Unroute(EndpointInfo source, EndpointInfo destination, eConnectionType type, int roomId);

		/// <summary>
		/// Searches for switchers currently routing the source and unroutes them.
		/// </summary>
		/// <param name="sourceControl"></param>
		/// <param name="type"></param>
		/// <param name="roomId"></param>
		/// <returns></returns>
		bool Unroute(IRouteSourceControl sourceControl, eConnectionType type, int roomId);

		/// <summary>
		/// Searches for switchers currently routing the source and unroutes them.
		/// </summary>
		/// <param name="sourceControl"></param>
		/// <param name="sourceAddress"></param>
		/// <param name="type"></param>
		/// <param name="roomId"></param>
		bool Unroute(IRouteSourceControl sourceControl, int sourceAddress, eConnectionType type, int roomId);

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
		bool Unroute(IRouteSourceControl sourceControl, int sourceAddress, IRouteDestinationControl destinationControl,
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
		bool Unroute(IRouteSourceControl sourceControl, int sourceAddress, IRouteDestinationControl destinationControl,
		             eConnectionType type, int roomId);

		/// <summary>
		/// Unroutes every path from the given source to the destination.
		/// </summary>
		/// <param name="sourceControl"></param>
		/// <param name="destinationControl"></param>
		/// <param name="type"></param>
		/// <param name="roomId"></param>
		/// <returns>False if the devices could not be unrouted.</returns>
		bool Unroute(IRouteSourceControl sourceControl, IRouteDestinationControl destinationControl, eConnectionType type,
		             int roomId);

		#endregion

		#region Devices

		/// <summary>
		/// Gets the controls for the given connection.
		/// </summary>
		/// <param name="connection"></param>
		/// <returns></returns>
		IEnumerable<IRouteControl> GetControls(Connection connection);

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
		/// Finds the actively routed sources for the destination at the given input address.
		/// Will return multiple items when connection types are combined, e.g. seperate audio and video sources.
		/// </summary>
		/// <param name="extends"></param>
		/// <param name="destinationControl"></param>
		/// <param name="destinationInput"></param>
		/// <param name="type"></param>
		/// <param name="signalDetected">When true skips inputs where no video is detected.</param>
		/// <exception cref="ArgumentNullException"></exception>
		/// <returns>The sources</returns>
		public static IEnumerable<EndpointInfo> GetActiveSourceEndpoints(this IRoutingGraph extends,
		                                                                 IRouteDestinationControl destinationControl,
		                                                                 int destinationInput, eConnectionType type,
		                                                                 bool signalDetected)
		{
			if (extends == null)
				throw new ArgumentNullException("extends");

			if (destinationControl == null)
				throw new ArgumentNullException("destinationControl");

			EndpointInfo destinationEndpoint = destinationControl.GetInputEndpointInfo(destinationInput);
			return extends.GetActiveSourceEndpoints(destinationEndpoint, type, signalDetected);
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

			return extends.GetDestinationControl(destination.Endpoint);
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

			return extends.GetSourceControl(source.Endpoint);
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
