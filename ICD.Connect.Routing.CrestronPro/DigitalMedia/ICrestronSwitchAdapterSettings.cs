using ICD.Connect.Devices;

namespace ICD.Connect.Routing.CrestronPro.DigitalMedia
{
	public interface ICrestronSwitchAdapterSettings : IDeviceSettings
	{
		byte? Ipid { get; set; }
	}
}
