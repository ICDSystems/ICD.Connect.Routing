namespace ICD.Connect.Routing.Extron.Devices.Switchers.SwHd4K
{
	public abstract class AbstractSwHd4KDevice<TSettings> : AbstractExtronSwitcherDevice<TSettings>, ISwHd4KDevice
		where TSettings : ISwHd4KSettings, new()
	{
	}
}
