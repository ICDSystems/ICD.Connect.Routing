namespace ICD.Connect.Routing.Extron.Devices.Dtp
{
	public interface IDtpHdmiRxDeviceSettings : IDtpHdmiDeviceSettings
	{
		int? DtpOutput { get; set; }
	}
}