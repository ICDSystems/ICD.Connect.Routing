using System;
using ICD.Connect.Devices.Controls;
using ICD.Connect.Routing.Extron.Controls;
using ICD.Connect.Settings;

namespace ICD.Connect.Routing.Extron.Devices.Switchers.SwHd4K.Sw6Hd4K
{
	public sealed class Sw6Hd4KDevice : AbstractSwHd4KDevice<Sw6Hd4KSettings>
	{
		/// <summary>
		/// Override to add controls to the device.
		/// </summary>
		/// <param name="settings"></param>
		/// <param name="factory"></param>
		/// <param name="addControl"></param>
		protected override void AddControls(Sw6Hd4KSettings settings, IDeviceFactory factory, Action<IDeviceControl> addControl)
		{
			base.AddControls(settings, factory, addControl);

			addControl(new ExtronSwitcherControl(this, 0, 6, 1, false));
		}
	}
}
