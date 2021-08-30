using System;
using ICD.Connect.Devices.Controls;
using ICD.Connect.Settings;
#if !NETSTANDARD
using Crestron.SimplSharpPro.DM;
#else
using System;
#endif

namespace ICD.Connect.Routing.CrestronPro.DigitalMedia.DmMd.DmMdMNXN
{
#if !NETSTANDARD
// ReSharper disable once InconsistentNaming
	public abstract class AbstractDmMdMNXNAdapter<TSwitcher, TSettings> : AbstractCrestronSwitchAdapter<TSwitcher, TSettings>,
	                                                                      IDmMdMNXNAdapter
		where TSwitcher : DmMDMnxn
#else
    public abstract class AbstractDmMdMNXNAdapter<TSettings> : AbstractCrestronSwitchAdapter<TSettings>, IDmMdMNXNAdapter
#endif
		where TSettings : IDmMdNXNAdapterSettings, new()
	{
#if !NETSTANDARD
		DmMDMnxn IDmMdMNXNAdapter.Switcher { get { return Switcher; } }

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

			addControl(new DmMdMNXNSwitcherControl(this));
		}
#endif
	}
}
