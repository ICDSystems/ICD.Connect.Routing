using ICD.Connect.Routing.Extron.Controls;

namespace ICD.Connect.Routing.Extron.Devices.Switchers.DtpCrosspoint.DtpCrosspoint84
{
	public sealed class DtpCrosspoint84Device : AbstractDtpCrosspointDevice<DtpCrosspoint84Settings>
	{
		protected override int NumberOfDtpInputPorts { get { return 2; } }

		protected override int NumberOfDtpOutputPorts { get { return 2; } }

		public DtpCrosspoint84Device()
		{
			Controls.Add(new ExtronSwitcherControl(this, 0, 8, 4));
		}
	}
}