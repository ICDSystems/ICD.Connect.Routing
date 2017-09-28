using ICD.Connect.Devices;

namespace ICD.Connect.Routing.CrestronPro.Transmitters
{
	public interface IEndpointTransmitterBaseAdapterSettings : IDeviceSettings
	{
		byte? Ipid { get; set; }
		int? DmSwitch { get; set; }
		int? DmInputAddress { get; set; }
	}
}
