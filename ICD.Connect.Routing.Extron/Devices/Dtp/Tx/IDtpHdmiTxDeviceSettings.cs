namespace ICD.Connect.Routing.Extron.Devices.Dtp.Tx
{
	public interface IDtpHdmiTxDeviceSettings : IDtpHdmiDeviceSettings
	{
		int? DtpInput { get; set; }
	}
}