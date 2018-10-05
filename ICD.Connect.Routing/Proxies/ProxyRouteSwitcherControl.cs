using ICD.Connect.Devices.Proxies.Devices;

namespace ICD.Connect.Routing.Proxies
{
	public sealed class ProxyRouteSwitcherControl : AbstractProxyRouteSwitcherControl
	{
		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="parent"></param>
		/// <param name="id"></param>
		public ProxyRouteSwitcherControl(IProxyDeviceBase parent, int id)
			: base(parent, id)
		{
		}
	}
}
