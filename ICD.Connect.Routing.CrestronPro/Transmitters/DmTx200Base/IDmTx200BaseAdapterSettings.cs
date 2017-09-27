using ICD.Connect.Devices;

namespace ICD.Connect.Routing.CrestronPro.Transmitters.DmTx200Base
{
	public interface IDmTx200BaseAdapterSettings : IDeviceSettings
	{
		byte? Ipid { get; set; }
		int? DmSwitch { get; set; }
		int? DmInputAddress { get; set; }
	}
}
