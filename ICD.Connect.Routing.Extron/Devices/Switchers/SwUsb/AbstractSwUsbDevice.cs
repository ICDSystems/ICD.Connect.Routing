namespace ICD.Connect.Routing.Extron.Devices.Switchers.SwUsb
{
	public abstract class AbstractSwUsbDevice<TSettings> : AbstractExtronSwitcherDevice<TSettings>, ISwUsbDevice
		where TSettings : ISwUsbSettings, new()
	{
	}
}
