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
		public ProxyRouteControl(IProxyDevice parent, int id)
			: base(parent, id)
		{
		}
	}
}
