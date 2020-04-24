namespace ICD.Connect.Routing.AVPro.Devices.Switchers.Auhd.AcCx42Auhd
{
	public sealed class AcCx42AuhdDevice : AbstractAuhdAvProSwitcherDevice<AcCx42AuhdDeviceSettings>
	{
		/// <summary>
		/// Gets the number of AV inputs.
		/// </summary>
		public override int NumberOfInputs { get { return 4; } }

		/// <summary>
		/// Gets the number of AV outputs.
		/// </summary>
		public override int NumberOfOutputs { get { return 2; } }
	}
}