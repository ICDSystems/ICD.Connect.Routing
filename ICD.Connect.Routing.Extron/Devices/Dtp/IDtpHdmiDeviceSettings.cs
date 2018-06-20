using ICD.Connect.Devices;
using ICD.Connect.Routing.Extron.Devices.DtpCrosspointBase;
using ICD.Connect.Settings.Attributes.SettingsProperties;

namespace ICD.Connect.Routing.Extron.Devices.Dtp
{
	public interface IDtpHdmiDeviceSettings : IDeviceSettings
	{
		[OriginatorIdSettingsProperty(typeof(IDtpCrosspointDevice))]
		int? DtpSwitch { get; set; }
	}
}