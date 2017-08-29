using ICD.Connect.Devices;

namespace ICD.Connect.Routing.CrestronPro.DigitalMedia.DmMdNXN
{
// ReSharper disable once InconsistentNaming
	public interface IDmMdNXNAdapterSettings : IDeviceSettings
	{
		byte Ipid { get; set; }
	}
}
