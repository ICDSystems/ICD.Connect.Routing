using ICD.Connect.Devices.Proxies.Devices;
using ICD.Connect.Settings.Attributes;

namespace ICD.Connect.Routing.SPlus.SPlusSwitcher.Proxy
{
	[KrangSettings("ProxySPlusSwitcherDevice",typeof(ProxySPlusSwitcherDevice))]
	public sealed class ProxySPlusSwitcherDeviceSettings : AbstractProxyDeviceSettings
	{
	}
}