using ICD.Connect.Devices.Proxies.Devices;

namespace ICD.Connect.Routing.Proxies
{
	public sealed class ProxyRouteSourceControl : AbstractProxyRouteSourceControl
	{
		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="parent"></param>
		/// <param name="id"></param>
		public ProxyRouteSourceControl(IProxyDevice parent, int id)
			: base(parent, id)
		{
		}
	}
}
