using ICD.Connect.Devices.Proxies.Devices;

namespace ICD.Connect.Routing.Proxies
{
	public sealed class ProxyRouteMidpointControl : AbstractProxyRouteMidpointControl
	{
		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="parent"></param>
		/// <param name="id"></param>
		public ProxyRouteMidpointControl(IProxyDeviceBase parent, int id)
			: base(parent, id)
		{
		}
	}
}
