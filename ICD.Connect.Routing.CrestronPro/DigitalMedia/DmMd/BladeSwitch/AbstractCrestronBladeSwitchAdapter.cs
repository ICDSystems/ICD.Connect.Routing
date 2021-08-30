using System;
using ICD.Connect.Devices.Controls;
using ICD.Connect.Settings;

namespace ICD.Connect.Routing.CrestronPro.DigitalMedia.DmMd.BladeSwitch
{
#if !NETSTANDARD
	public abstract class AbstractCrestronBladeSwitchAdapter<TSwitch, TSettings> :
		AbstractCrestronSwitchAdapter<TSwitch, TSettings>, ICrestronBladeSwitchAdapter
		where TSwitch : Crestron.SimplSharpPro.DM.BladeSwitch
#else
	public abstract class AbstractCrestronBladeSwitchAdapter<TSettings> :
		AbstractCrestronSwitchAdapter<TSettings>, ICrestronBladeSwitchAdapter
#endif
		where TSettings : ICrestronBladeSwitchAdapterSettings, new()
	{
#if !NETSTANDARD
		Crestron.SimplSharpPro.DM.BladeSwitch ICrestronBladeSwitchAdapter.Switcher { get { return Switcher; } }
#endif

		/// <summary>
		/// Override to add controls to the device.
		/// </summary>
		/// <param name="settings"></param>
		/// <param name="factory"></param>
		/// <param name="addControl"></param>
		protected override void AddControls(TSettings settings, IDeviceFactory factory, Action<IDeviceControl> addControl)
		{
			base.AddControls(settings, factory, addControl);

#if !NETSTANDARD
			addControl(new BladeSwitchSwitcherControl(this));
#endif
		}
	}
}
