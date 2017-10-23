using ICD.Connect.Devices;

namespace ICD.Connect.Routing.CrestronPro.DigitalMedia.Dm100xStrBase
{
	public interface IDm100XStrBaseAdapterSettings : IDeviceSettings
	{
		byte EthernetId { get; set; }
	}
}