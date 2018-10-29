using ICD.Connect.Devices;
using ICD.Connect.Routing.Extron.Devices.Switchers;
using ICD.Connect.Routing.Extron.Devices.Switchers.DtpCrosspoint;
using ICD.Connect.Settings.Attributes.SettingsProperties;

namespace ICD.Connect.Routing.Extron.Devices.Endpoints
{
	public interface IDtpHdmiDeviceSettings : IDeviceSettings
	{
		[OriginatorIdSettingsProperty(typeof(IDtpCrosspointDevice))]
		int? DtpSwitch { get; set; }
	}
}