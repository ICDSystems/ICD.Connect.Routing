using ICD.Connect.Devices;

namespace ICD.Connect.Routing.CrestronPro.DigitalMedia
{
	public interface IDmSwitcherAdapterSettings : IDeviceSettings
	{
		byte Ipid { get; set; }
	}
}
