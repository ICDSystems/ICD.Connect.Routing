using ICD.Connect.Devices;

namespace ICD.Connect.Routing.CrestronPro.Receivers
{
	public interface IEndpointReceiverBaseAdapterSettings : IDeviceSettings
	{
		byte? Ipid { get; set; }

		int? DmSwitch { get; set; }

		int? DmOutputAddress { get; set; }
	}
}
