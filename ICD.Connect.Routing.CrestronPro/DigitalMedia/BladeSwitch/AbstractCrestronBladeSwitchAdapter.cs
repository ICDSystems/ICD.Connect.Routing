namespace ICD.Connect.Routing.CrestronPro.DigitalMedia.BladeSwitch
{
#if SIMPLSHARP
	public abstract class AbstractCrestronBladeSwitchAdapter<TSwitch, TSettings> :
		AbstractCrestronSwitchAdapter<TSwitch, TSettings>, ICrestronBladeSwitchAdapter
		where TSwitch : Crestron.SimplSharpPro.DM.BladeSwitch
#else
	public abstract class AbstractCrestronBladeSwitchAdapter<TSettings> :
		AbstractCrestronSwitchAdapter<TSettings>, ICrestronBladeSwitchAdapter
#endif
		where TSettings : ICrestronBladeSwitchAdapterSettings, new()
	{
		Crestron.SimplSharpPro.DM.BladeSwitch ICrestronBladeSwitchAdapter.Switcher { get { return Switcher; } }

		/// <summary>
		/// Constructor.
		/// </summary>
		protected AbstractCrestronBladeSwitchAdapter()
		{
#if SIMPLSHARP
			Controls.Add(new BladeSwitchSwitcherControl(this));
#endif
		}
	}
}
