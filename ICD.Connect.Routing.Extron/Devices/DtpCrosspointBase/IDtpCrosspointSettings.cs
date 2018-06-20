using ICD.Connect.Devices;

namespace ICD.Connect.Routing.Extron.Devices.DtpCrosspointBase
{
	public interface IDtpCrosspointSettings : IDeviceSettings
	{
		int? Port { get; set; }
		string Password { get; set; }
	}
}