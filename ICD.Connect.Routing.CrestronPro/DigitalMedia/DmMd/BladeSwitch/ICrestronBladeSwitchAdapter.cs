namespace ICD.Connect.Routing.CrestronPro.DigitalMedia.BladeSwitch
{
	public interface ICrestronBladeSwitchAdapter : ICrestronSwitchAdapter
	{
#if SIMPLSHARP
		new Crestron.SimplSharpPro.DM.BladeSwitch Switcher { get; }
#endif
	}
}