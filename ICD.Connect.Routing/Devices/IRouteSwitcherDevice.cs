using System;
using ICD.Connect.Routing.Connections;
using ICD.Connect.Routing.EventArguments;

namespace ICD.Connect.Routing.Devices
{
	public interface IRouteSwitcherDevice : IRouteMidpointDevice
	{
		/// <summary>
		/// Called when a route changes.
		/// </summary>
		event EventHandler<RouteChangeEventArgs> OnRouteChange;

		/// <summary>
		/// Performs the given route operation.
		/// </summary>
		/// <param name="info"></param>
		/// <returns></returns>
		bool Route(RouteOperation info);

		/// <summary>
		/// Stops routing to the given output.
		/// </summary>
		/// <param name="output"></param>
		/// <param name="type"></param>
		/// <returns>True if successfully cleared.</returns>
		bool ClearOutput(int output, eConnectionType type);
	}
}
