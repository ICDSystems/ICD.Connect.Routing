using System.Collections.Generic;
using ICD.Common.Properties;
using ICD.Connect.Devices.Controls;
using ICD.Connect.Routing.Controls;
using ICD.Connect.Routing.Endpoints;
using ICD.Connect.Routing.Endpoints.Destinations;
using ICD.Connect.Routing.Endpoints.Sources;
using ICD.Connect.Settings;

namespace ICD.Connect.Routing.Connections
{
	public interface IConnectionsCollection : IOriginatorCollection<Connection>
	{
		#region Methods

		/// <summary>
		/// Gets the connection for the given endpoint.
		/// </summary>
		/// <param name="destination"></param>
		/// <returns></returns>
		[CanBeNull]
		Connection GetInputConnection(EndpointInfo destination);

		/// <summary>
		/// Gets the connection for the given endpoint.
		/// </summary>
		/// <param name="destination"></param>
		/// <param name="input"></param>
		/// <returns></returns>
		[CanBeNull]
		Connection GetInputConnection(EndpointInfo destination, eConnectionType input);

		/// <summary>
		/// Gets the connection for the given endpoint.
		/// </summary>
		/// <param name="destinationControl"></param>
		/// <param name="input"></param>
		/// <returns></returns>
		[CanBeNull]
		Connection GetInputConnection(IRouteDestinationControl destinationControl, int input);

		/// <summary>
		/// Gets the input connections for the device with the given type.
		/// </summary>
		/// <param name="destinationDeviceId"></param>
		/// <param name="destinationControlId"></param>
		/// <param name="type"></param>
		/// <returns></returns>
		IEnumerable<Connection> GetInputConnections(int destinationDeviceId, int destinationControlId,
		                                            eConnectionType type);

		/// <summary>
		/// Gets the connection for the given endpoint.
		/// </summary>
		/// <param name="source"></param>
		/// <returns></returns>
		[CanBeNull]
		Connection GetOutputConnection(EndpointInfo source);

		/// <summary>
		/// Gets the connection for the given endpoint.
		/// </summary>
		/// <param name="sourceControl"></param>
		/// <param name="output"></param>
		/// <returns></returns>
		[CanBeNull]
		Connection GetOutputConnection(IRouteSourceControl sourceControl, int output);

		/// <summary>
		/// Gets the connection for the given endpoint.
		/// </summary>
		/// <param name="sourceEndpoint"></param>
		/// <param name="type"></param>
		/// <returns></returns>
		Connection GetOutputConnection(EndpointInfo sourceEndpoint, eConnectionType type);

		/// <summary>
		/// Given a source endpoint and a final destination endpoint,
		/// returns the possible output connection from the source to reach the destination.
		/// </summary>
		/// <param name="source"></param>
		/// <param name="finalDestination"></param>
		/// <param name="flag"></param>
		/// <returns></returns>
		[CanBeNull]
		Connection GetOutputConnection(EndpointInfo source, EndpointInfo finalDestination, eConnectionType flag);

		/// <summary>
		/// Gets the output connections for the given source device.
		/// </summary>
		/// <param name="sourceDeviceId"></param>
		/// <param name="sourceControlId"></param>
		/// <returns></returns>
		IEnumerable<Connection> GetOutputConnections(int sourceDeviceId, int sourceControlId);

		/// <summary>
		/// Gets the output connections for the given source device.
		/// </summary>
		/// <param name="sourceDeviceId"></param>
		/// <param name="sourceControlId"></param>
		/// <param name="flag"></param>
		/// <returns></returns>
		IEnumerable<Connection> GetOutputConnections(int sourceDeviceId, int sourceControlId, eConnectionType flag);

		/// <summary>
		/// Gets the output connections from the given source control in order to reach the given destination endpoint.
		/// </summary>
		/// <param name="sourceEndpoint"></param>
		/// <param name="finalDestination"></param>
		/// <param name="flag"></param>
		/// <returns></returns>
		IEnumerable<Connection> GetOutputConnections(DeviceControlInfo sourceEndpoint, EndpointInfo finalDestination,
		                                             eConnectionType flag);

		/// <summary>
		/// Gets the output connections from the given source control in order to reach the given destination endpoints.
		/// </summary>
		/// <param name="sourceEndpoint"></param>
		/// <param name="finalDestinations"></param>
		/// <param name="flag"></param>
		/// <returns></returns>
		IEnumerable<Connection> GetOutputConnections(DeviceControlInfo sourceEndpoint,
		                                             IEnumerable<EndpointInfo> finalDestinations, eConnectionType flag);

		/// <summary>
		/// Gets filtered endpoints for the given destination.
		/// </summary>
		/// <param name="destination"></param>
		/// <param name="flag"></param>
		/// <returns></returns>
		IEnumerable<EndpointInfo> FilterEndpoints(IDestination destination, eConnectionType flag);

		/// <summary>
		/// Gets filtered endpoints for the given source.
		/// </summary>
		/// <param name="source"></param>
		/// <param name="flag"></param>
		/// <returns></returns>
		IEnumerable<EndpointInfo> FilterEndpoints(ISource source, eConnectionType flag);

		/// <summary>
		/// Gets filtered endpoints matching any of the given connection flags for the given destination.
		/// </summary>
		/// <param name="destination"></param>
		/// <param name="type"></param>
		/// <returns></returns>
		IEnumerable<EndpointInfo> FilterEndpointsAny(IDestination destination, eConnectionType type);

		/// <summary>
		/// Gets filtered endpoints matching any of the given connection flags for the given source.
		/// </summary>
		/// <param name="source"></param>
		/// <param name="type"></param>
		/// <returns></returns>
		IEnumerable<EndpointInfo> FilterEndpointsAny(ISource source, eConnectionType type);

		#endregion

		#region Adjacency

		/// <summary>
		/// Returns the destination input addresses where source and destination are directly connected.
		/// </summary>
		/// <param name="sourceControl"></param>
		/// <param name="destinationControl"></param>
		/// <param name="type"></param>
		/// <returns></returns>
		IEnumerable<int> GetInputs(IRouteSourceControl sourceControl, IRouteDestinationControl destinationControl,
		                           eConnectionType type);

		/// <summary>
		/// Returns the destination input addresses.
		/// </summary>
		/// <param name="destinationControl"></param>
		/// <param name="type"></param>
		/// <returns></returns>
		IEnumerable<int> GetInputs(IRouteDestinationControl destinationControl, eConnectionType type);

		/// <summary>
		/// Gets the mapped output addresses for the given source device.
		/// </summary>
		/// <param name="sourceControl"></param>
		/// <param name="type"></param>
		/// <returns></returns>
		IEnumerable<int> GetOutputs(IRouteSourceControl sourceControl, eConnectionType type);

		/// <summary>
		/// Gets the mapped output addresses for the given source device.
		/// </summary>
		/// <param name="sourceDeviceId"></param>
		/// <param name="sourceControlId"></param>
		/// <param name="type"></param>
		/// <returns></returns>
		IEnumerable<int> GetOutputs(int sourceDeviceId, int sourceControlId, eConnectionType type);

		/// <summary>
		/// Returns the source output addresses where source and destination are directly connected.
		/// </summary>
		/// <param name="sourceControl"></param>
		/// <param name="destinationControl"></param>
		/// <param name="type"></param>
		/// <returns></returns>
		IEnumerable<int> GetOutputs(IRouteSourceControl sourceControl, IRouteDestinationControl destinationControl,
		                            eConnectionType type);

		#endregion
	}
}
