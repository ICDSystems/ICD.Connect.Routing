using System;
using ICD.Connect.Devices.Controls;
using ICD.Connect.Routing.Extron.Controls.Routing;
using ICD.Connect.Settings;

namespace ICD.Connect.Routing.Extron.Devices.Switchers.SwHd4K.Sw8Hd4K
{
	public sealed class Sw8Hd4KDevice : AbstractSwHd4KDevice<Sw8Hd4KSettings>
	{
		/// <summary>
		/// Override to add controls to the device.
		/// </summary>
		/// <param name="settings"></param>
		/// <param name="factory"></param>
		/// <param name="addControl"></param>
		protected override void AddControls(Sw8Hd4KSettings settings, IDeviceFactory factory, Action<IDeviceControl> addControl)
		{
			base.AddControls(settings, factory, addControl);

			addControl(new GenericExtronSwitcherControl(this, 0, 8, 1, false));
		}
	}
}
