using ICD.Connect.Devices.Proxies.Devices;
using ICD.Connect.Settings.Attributes;

namespace ICD.Connect.Routing.SPlus.SPlusVolumeDevice.Proxy
{
    [KrangSettings("ProxySPlusVolumeDevice", typeof(ProxySPlusVolumeDevice))]
    public sealed class ProxySPlusVolumeDeviceSettings : AbstractProxyDeviceSettings
    {
    }
}