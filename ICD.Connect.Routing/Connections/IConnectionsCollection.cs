using System.Collections.Generic;
using ICD.Common.Properties;
using ICD.Connect.Routing.Controls;
using ICD.Connect.Routing.Endpoints;
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
		/// Gets the input connections for the device matching any of the given type flags.
		/// </summary>
		/// <param name="destinationDeviceId"></param>
		/// <param name="destinationControlId"></param>
		/// <param name="type"></param>
		/// <returns></returns>
		IEnumerable<Connection> GetInputConnectionsAny(int destinationDeviceId, int destinationControlId,
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
		/// <param name="type"></param>
		/// <returns></returns>
		IEnumerable<Connection> GetOutputConnections(int sourceDeviceId, int sourceControlId, eConnectionType type);

		/// <summary>
		/// Gets the output connections for the given source device matching any of the given type flags.
		/// </summary>
		/// <param name="sourceDeviceId"></param>
		/// <param name="sourceControlId"></param>
		/// <param name="type"></param>
		/// <returns></returns>
		IEnumerable<Connection> GetOutputConnectionsAny(int sourceDeviceId, int sourceControlId, eConnectionType type);

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
