using ICD.Connect.Routing.CrestronPro.DigitalMedia.DmNvx.Dm100xStrBase;

namespace ICD.Connect.Routing.CrestronPro.DigitalMedia.DmNvx.DmNvxBaseClass
{
	public interface IDmNvxBaseClassAdapterSettings : IDm100XStrBaseAdapterSettings
	{
		eDeviceMode DeviceMode { get; set; }
	}
}