namespace ICD.Connect.Routing.AVPro.Devices.Switchers.Auhd
{
	public abstract class AbstractAuhdAvProSwitcherDevice<TSettings> : AbstractAvProSwitcherDevice<TSettings>, IAuhdAvProSwitcherDevice
		where TSettings : IAuhdAvProSwitcherDeviceSettings, new()
	{
	}
}