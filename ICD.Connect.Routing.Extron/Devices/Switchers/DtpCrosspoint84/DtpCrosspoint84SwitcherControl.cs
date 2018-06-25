namespace ICD.Connect.Routing.Extron.Devices.Switchers.DtpCrosspoint84
{
	public sealed class DtpCrosspoint84SwitcherControl : AbstractDtpCrosspointSwitcherControl<DtpCrosspoint84, DtpCrosspoint84Settings>
	{
		public DtpCrosspoint84SwitcherControl(DtpCrosspoint84 parent, int id) : base(parent, id)
		{
		}

		public override int NumberOfInputs
		{
			get { return 8; }
		}

		public override int NumberOfOutputs
		{
			get { return 4; }
		}
	}
}