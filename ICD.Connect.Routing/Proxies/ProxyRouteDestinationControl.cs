using ICD.Connect.Devices.Proxies.Devices;

namespace ICD.Connect.Routing.Proxies
{
	public sealed class ProxyRouteDestinationControl : AbstractProxyRouteDestinationControl
	{
		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="parent"></param>
		/// <param name="id"></param>
		public ProxyRouteDestinationControl(IProxyDevice parent, int id)
			: base(parent, id)
		{
		}
	}
}
