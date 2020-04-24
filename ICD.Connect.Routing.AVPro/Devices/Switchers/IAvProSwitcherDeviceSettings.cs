using ICD.Connect.Devices;
using ICD.Connect.Protocol.Network.Settings;
using ICD.Connect.Protocol.Ports;
using ICD.Connect.Protocol.Settings;
using ICD.Connect.Settings.Attributes.SettingsProperties;

namespace ICD.Connect.Routing.AVPro.Devices.Switchers
{
	public interface IAvProSwitcherDeviceSettings : IDeviceSettings, INetworkSettings, IComSpecSettings
	{
		/// <summary>
		/// The port id.
		/// </summary>
		[OriginatorIdSettingsProperty(typeof(ISerialPort))]
		int? Port { get; set; }
	}
}