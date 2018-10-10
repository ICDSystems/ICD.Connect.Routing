using ICD.Connect.Routing.CrestronPro.DigitalMedia.Dm100xStrBase;

namespace ICD.Connect.Routing.CrestronPro.DigitalMedia.DmNvxBaseClass
{
	public interface IDmNvxBaseClassAdapterSettings : IDm100XStrBaseAdapterSettings
	{
		eDeviceMode DeviceMode { get; set; }
	}
}