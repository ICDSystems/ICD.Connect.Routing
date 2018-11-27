using System.Collections.Generic;
using ICD.Connect.Settings.Attributes.SettingsProperties;

namespace ICD.Connect.Routing.Extron.Devices.Switchers.DtpCrosspoint
{
	public interface IDtpCrosspointSettings : IExtronSwitcherDeviceSettings
	{
		[IpAddressSettingsProperty]
		string Address { get; set; }

		string Config { get; set; }

		[HiddenSettingsProperty]
		IEnumerable<KeyValuePair<int, int>> DtpInputPorts { get; set; }

		[HiddenSettingsProperty]
		IEnumerable<KeyValuePair<int, int>> DtpOutputPorts { get; set; }
	}
}