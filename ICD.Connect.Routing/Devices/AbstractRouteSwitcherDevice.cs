using System;
using ICD.Connect.Devices;
using ICD.Connect.Routing.Connections;
using ICD.Connect.Routing.EventArguments;

namespace ICD.Connect.Routing.Devices
{
	public abstract class AbstractRouteSwitcherDevice<TSettings> : AbstractRouteMidpointDevice<TSettings>, IRouteSwitcherDevice
		where TSettings : IDeviceSettings, new()
	{
		/// <summary>
		/// Called when a route changes.
		/// </summary>
		public abstract event EventHandler<RouteChangeEventArgs> OnRouteChange;

		/// <summary>
		/// Performs the given route operation.
		/// </summary>
		/// <param name="info"></param>
		/// <returns></returns>
		public abstract bool Route(RouteOperation info);

		/// <summary>
		/// Stops routing to the given output.
		/// </summary>
		/// <param name="output"></param>
		/// <param name="type"></param>
		/// <returns>True if successfully cleared.</returns>
		public abstract bool ClearOutput(int output, eConnectionType type);
	}
}
