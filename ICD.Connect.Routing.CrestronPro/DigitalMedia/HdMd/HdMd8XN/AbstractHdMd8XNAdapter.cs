using System;
using ICD.Connect.Devices.Controls;
using ICD.Connect.Settings;
#if !NETSTANDARD
using Crestron.SimplSharpPro.DM;
#endif

namespace ICD.Connect.Routing.CrestronPro.DigitalMedia.HdMd.HdMd8XN
{
#if !NETSTANDARD
	public abstract class AbstractHdMd8XNAdapter<TSwitch, TSettings> : AbstractCrestronSwitchAdapter<TSwitch, TSettings>, IHdMd8XNAdapter
		where TSwitch : HdMd8xN
#else
	public abstract class AbstractHdMd8XNAdapter<TSettings> : AbstractCrestronSwitchAdapter<TSettings>, IHdMd8XNAdapter
#endif
		where TSettings : IHdMd8XNAdapterSettings, new()
	{

#if !NETSTANDARD
		HdMd8xN IHdMd8XNAdapter.Switcher { get { return Switcher; } }

		/// <summary>
		/// Override to add controls to the device.
		/// </summary>
		/// <param name="settings"></param>
		/// <param name="factory"></param>
		/// <param name="addControl"></param>
		protected override void AddControls(TSettings settings, IDeviceFactory factory, Action<IDeviceControl> addControl)
		{
			base.AddControls(settings, factory, addControl);

			addControl(new HdMd8XNSwitcherControl(this));
		}
#endif
	}
}
