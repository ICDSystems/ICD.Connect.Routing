using ICD.Connect.Devices.CrestronSPlus.Devices.SPlus;
using ICD.Connect.Settings.Attributes;

namespace ICD.Connect.Routing.SPlus.SPlusVolumeDevice.Device
{
    [KrangSettings("SPlusVolumeDevice", typeof(SPlusVolumeDevice))]
    public sealed class SPlusVolumeDeviceSettings : AbstractSPlusDeviceSettings
    {
    }
}