using System.Collections.Generic;
using ICD.Common.Properties;
using ICD.Connect.Routing.Connections;
using ICD.Connect.Routing.Controls;
using ICD.Connect.Routing.Endpoints;

namespace ICD.Connect.Routing.ConnectionUsage
{
	public interface IConnectionUsageCollection : IEnumerable<ConnectionUsageInfo>
	{
		/// <summary>
		/// Clears the cache of how the connections are being used. This can free up previously used connections for new routes.
		/// </summary>
		[PublicAPI]
		void Clear();

		/// <summary>
		/// Gets the usage info for the given connection.
		/// </summary>
		/// <param name="connection"></param>
		/// <returns></returns>
		[PublicAPI]
		ConnectionUsageInfo GetConnectionUsageInfo(Connection connection);

		/// <summary>
		/// Removes usages where connections no longer exist.
		/// </summary>
		void RemoveInvalid();

		/// <summary>
		/// Updates connection usages to match the new state of the switcher.
		/// </summary>
		/// <param name="switcher"></param>
		/// <param name="output"></param>
		/// <param name="type"></param>
		void UpdateConnectionsUsage(IRouteSwitcherControl switcher, int output, eConnectionType type);

		/// <summary>
		/// Returns true if the current usage of the connection will allow the given source for the given room.
		/// </summary>
		/// <param name="connection"></param>
		/// <param name="source"></param>
		/// <param name="roomId"></param>
		/// <param name="type"></param>
		/// <returns></returns>
		bool CanRouteConnection(Connection connection, EndpointInfo source, int roomId, eConnectionType type);

		/// <summary>
		/// Called before the RoutingGraph routes a switcher. Flags the connection as being in use.
		/// </summary>
		/// <param name="connection"></param>
		/// <param name="operation"></param>
		void ClaimConnection(Connection connection, RouteOperation operation);
	}
}
