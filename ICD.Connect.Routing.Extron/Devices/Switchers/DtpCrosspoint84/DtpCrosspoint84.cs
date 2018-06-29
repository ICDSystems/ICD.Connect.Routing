namespace ICD.Connect.Routing.Extron.Devices.Switchers.DtpCrosspoint84
{
	public sealed class DtpCrosspoint84 : AbstractDtpCrosspointDevice<DtpCrosspoint84Settings>
	{
		protected override int DtpInputPorts { get { return 2; } }

		protected override int DtpOutputPorts { get { return 2; } }

		public DtpCrosspoint84()
		{
			Controls.Add(new DtpCrosspoint84SwitcherControl(this, 0));
		}
	}
}