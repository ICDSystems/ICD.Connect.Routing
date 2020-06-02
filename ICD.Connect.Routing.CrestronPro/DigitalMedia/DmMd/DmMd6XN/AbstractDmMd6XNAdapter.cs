using System;
using ICD.Connect.Devices.Controls;
using ICD.Connect.Settings;

namespace ICD.Connect.Routing.CrestronPro.DigitalMedia.DmMd.DmMd6XN
{
#if SIMPLSHARP
	public abstract class AbstractDmMd6XNAdapter<TSwitcher, TSettings> : AbstractCrestronSwitchAdapter<TSwitcher, TSettings>, IDmMd6XNAdapter
		where TSwitcher : Crestron.SimplSharpPro.DM.DmMd6XN
#else
	public abstract class AbstractDmMd6XNAdapter<TSettings> : AbstractCrestronSwitchAdapter<TSettings>, IDmMd6XNAdapter
#endif
		where TSettings : IDmMd6XNAdapterSettings, new()
	{
#if SIMPLSHARP
		Crestron.SimplSharpPro.DM.DmMd6XN IDmMd6XNAdapter.Switcher { get { return Switcher; } }

		/// <summary>
		/// Override to control how the assigned switcher behaves.
		/// </summary>
		/// <param name="switcher"></param>
		protected override void ConfigureSwitcher(TSwitcher switcher)
		{
			base.ConfigureSwitcher(switcher);

			if (switcher == null)
				return;

			switcher.EnableAudioBreakaway.BoolValue = true;
			switcher.AudioEnter.BoolValue = true;
		}

		/// <summary>
		/// Override to add controls to the device.
		/// </summary>
		/// <param name="settings"></param>
		/// <param name="factory"></param>
		/// <param name="addControl"></param>
		protected override void AddControls(TSettings settings, IDeviceFactory factory, Action<IDeviceControl> addControl)
		{
			base.AddControls(settings, factory, addControl);

			addControl(new DmMd6XNSwitcherControl(this));
		}
#endif
	}
}
