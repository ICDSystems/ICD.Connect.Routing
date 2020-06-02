using System;
using ICD.Connect.Devices.Controls;
using ICD.Connect.Routing.Extron.Controls;
using ICD.Connect.Settings;

namespace ICD.Connect.Routing.Extron.Devices.Switchers.DtpCrosspoint.DtpCrosspoint84
{
	public sealed class DtpCrosspoint84Device : AbstractDtpCrosspointDevice<DtpCrosspoint84Settings>
	{
		public override int NumberOfDtpInputPorts { get { return 2; } }

		public override int NumberOfDtpOutputPorts { get { return 2; } }

		/// <summary>
		/// Override to add controls to the device.
		/// </summary>
		/// <param name="settings"></param>
		/// <param name="factory"></param>
		/// <param name="addControl"></param>
		protected override void AddControls(DtpCrosspoint84Settings settings, IDeviceFactory factory, Action<IDeviceControl> addControl)
		{
			base.AddControls(settings, factory, addControl);

			addControl(new ExtronSwitcherControl(this, 0, 8, 4, true));
		}
	}
}