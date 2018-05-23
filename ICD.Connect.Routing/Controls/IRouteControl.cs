using ICD.Connect.API.Attributes;
using ICD.Connect.Devices.Controls;
using ICD.Connect.Routing.Proxies;

namespace ICD.Connect.Routing.Controls
{
	/// <summary>
	/// The base interface for all routing controls.
	/// </summary>
	[ApiClass(typeof(ProxyRouteControl), typeof(IDeviceControl))]
	public interface IRouteControl : IDeviceControl
	{
	}
}
