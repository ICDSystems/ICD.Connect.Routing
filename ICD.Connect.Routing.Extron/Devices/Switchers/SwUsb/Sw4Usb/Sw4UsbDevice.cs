using System;
using ICD.Connect.Devices.Controls;
using ICD.Connect.Routing.Extron.Controls.Routing;
using ICD.Connect.Settings;

namespace ICD.Connect.Routing.Extron.Devices.Switchers.SwUsb.Sw4Usb
{
	public sealed class Sw4UsbDevice : AbstractSwUsbDevice<Sw4UsbSettings>
	{
		/// <summary>
		/// Override to add controls to the device.
		/// </summary>
		/// <param name="settings"></param>
		/// <param name="factory"></param>
		/// <param name="addControl"></param>
		protected override void AddControls(Sw4UsbSettings settings, IDeviceFactory factory, Action<IDeviceControl> addControl)
		{
			base.AddControls(settings, factory, addControl);

			addControl(new GenericExtronUsbSwitcherControl(this, 0, 4, 4));
		}
	}
}
