using ICD.Connect.Devices;

namespace ICD.Connect.Routing.CrestronPro.DigitalMedia.DmXio.DmXioDirectorBase
{
	public interface IDmXioDirectorBaseAdapterSettings : IDeviceSettings
	{
		uint? EthernetId { get; set; }
	}
}
