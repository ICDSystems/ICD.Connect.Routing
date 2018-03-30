using ICD.Connect.Devices.Proxies.Controls;
using ICD.Connect.Devices.Proxies.Devices;

namespace ICD.Connect.Routing.Proxies
{
	public abstract class AbstractProxyRouteControl : AbstractProxyDeviceControl, IProxyRouteControl
	{
		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="parent"></param>
		/// <param name="id"></param>
		protected AbstractProxyRouteControl(IProxyDeviceBase parent, int id)
			: base(parent, id)
		{
		}
	}
}
