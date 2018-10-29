using ICD.Connect.Devices;
using ICD.Connect.Protocol.Ports;
using ICD.Connect.Settings.Attributes.SettingsProperties;

namespace ICD.Connect.Routing.Extron.Devices.Switchers
{
	public interface IExtronSwitcherDeviceSettings : IDeviceSettings
	{
		/// <summary>
		/// The port id.
		/// </summary>
		[OriginatorIdSettingsProperty(typeof(ISerialPort))]
		int? Port { get; set; }

		string Password { get; set; }
	}
}