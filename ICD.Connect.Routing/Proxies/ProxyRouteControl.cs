using ICD.Connect.Devices.Proxies.Devices;

namespace ICD.Connect.Routing.Proxies
{
	public sealed class ProxyRouteControl : AbstractProxyRouteControl
	{
		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="parent"></param>
		/// <param name="id"></param>
		public ProxyRouteControl(IProxyDeviceBase parent, int id)
			: base(parent, id)
		{
		}
	}
}
