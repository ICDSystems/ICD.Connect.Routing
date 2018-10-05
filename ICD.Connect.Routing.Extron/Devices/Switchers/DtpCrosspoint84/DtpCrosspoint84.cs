namespace ICD.Connect.Routing.Extron.Devices.Switchers.DtpCrosspoint84
{
	public sealed class DtpCrosspoint84 : AbstractDtpCrosspointDevice<DtpCrosspoint84Settings>
	{
		protected override int NumberOfDtpInputPorts { get { return 2; } }

		protected override int NumberOfDtpOutputPorts { get { return 2; } }

		public DtpCrosspoint84()
		{
			Controls.Add(new DtpCrosspointSwitcherControl(this, 0, 8, 4));
		}
	}
}