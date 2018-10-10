using ICD.Connect.Devices;

namespace ICD.Connect.Routing.CrestronPro.DigitalMedia.DmXioDirectorBase
{
	public interface IDmXioDirectorBaseAdapterSettings : IDeviceSettings
	{
		uint? EthernetId { get; set; }
	}
}
