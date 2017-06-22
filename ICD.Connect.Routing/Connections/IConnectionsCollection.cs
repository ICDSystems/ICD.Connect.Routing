using System.Collections.Generic;
using ICD.Connect.Routing.Controls;

namespace ICD.Connect.Routing.Connections
{
	public interface IConnectionsCollection : IEnumerable<Connection>
	{
		/// <summary>
		/// Gets all of the connections.
		/// </summary>
		/// <returns></returns>
		IEnumerable<Connection> GetConnections();

		/// <summary>
		/// Clears and sets the connections.
		/// </summary>
		/// <param name="connections"></param>
		void SetConnections(IEnumerable<Connection> connections);

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
	}
}
