using ICD.Connect.Devices;

namespace ICD.Connect.Routing.CrestronPro.Receivers.DmRmcScalerCBase
{
	public interface IDmRmcScalerCAdapterSettings : IDeviceSettings
	{
		byte? Ipid { get; set; }

		int? DmSwitch { get; set; }

		int? DmOutputAddress { get; set; }
	}
}