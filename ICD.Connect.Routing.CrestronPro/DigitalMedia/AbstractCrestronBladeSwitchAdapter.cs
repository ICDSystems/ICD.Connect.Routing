#if SIMPLSHARP
using Crestron.SimplSharpPro.DM;
#endif

namespace ICD.Connect.Routing.CrestronPro.DigitalMedia
{
#if SIMPLSHARP
	public abstract class AbstractCrestronBladeSwitchAdapter<TSwitch, TSettings> :
		AbstractCrestronSwitchAdapter<TSwitch, TSettings>,
		ICrestronBladeSwitchAdapter
		where TSwitch : BladeSwitch
#else
	public abstract class AbstractCrestronBladeSwitchAdapter<TSettings> :
		AbstractCrestronSwitchAdapter<TSettings>,
#endif
		where TSettings : ICrestronBladeSwitchAdapterSettings, new()
	{
	}
}
