using System;
using ICD.Connect.Devices.Proxies.Devices;
using ICD.Connect.Routing.Connections;
using ICD.Connect.Routing.EventArguments;

namespace ICD.Connect.Routing.Proxies
{
	public abstract class AbstractProxyRouteSwitcherControl : AbstractProxyRouteMidpointControl, IProxyRouteSwitcherControl
	{
		public event EventHandler<RouteChangeEventArgs> OnRouteChange;

		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="parent"></param>
		/// <param name="id"></param>
		protected AbstractProxyRouteSwitcherControl(IProxyDeviceBase parent, int id)
			: base(parent, id)
		{
		}

		/// <summary>
		/// Override to release resources.
		/// </summary>
		/// <param name="disposing"></param>
		protected override void DisposeFinal(bool disposing)
		{
			OnRouteChange = null;

			base.DisposeFinal(disposing);
		}

		/// <summary>
		/// Performs the given route operation.
		/// </summary>
		/// <param name="info"></param>
		/// <returns></returns>
		public bool Route(RouteOperation info)
		{
			// TODO
			return true;
		}

		/// <summary>
		/// Stops routing to the given output.
		/// </summary>
		/// <param name="output"></param>
		/// <param name="type"></param>
		/// <returns>True if successfully cleared.</returns>
		public bool ClearOutput(int output, eConnectionType type)
		{
			// TODO
			return true;
		}
	}
}
