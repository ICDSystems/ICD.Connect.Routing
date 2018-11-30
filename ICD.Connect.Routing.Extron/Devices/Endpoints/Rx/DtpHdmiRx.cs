using ICD.Connect.Settings;

namespace ICD.Connect.Routing.Extron.Devices.Endpoints.Rx
{
	public sealed class DtpHdmiRx : AbstractDtpHdmiDevice<DtpHdmiRxSettings>
	{
		private int m_DtpOutput;

		#region Properties

		/// <summary>
		/// Gets the address where this endpoint is connected to the switcher.
		/// </summary>
		public override int SwitcherAddress { get { return m_DtpOutput; } }

		/// <summary>
		/// Returns Input for TX, Output for RX.
		/// </summary>
		public override eDtpInputOuput SwitcherInputOutput { get { return eDtpInputOuput.Output; } }

		#endregion

		#region Settings

		protected override void ApplySettingsFinal(DtpHdmiRxSettings settings, IDeviceFactory factory)
		{
			base.ApplySettingsFinal(settings, factory);

			m_DtpOutput = settings.DtpOutput;
		}

		protected override void CopySettingsFinal(DtpHdmiRxSettings settings)
		{
			base.CopySettingsFinal(settings);

			settings.DtpOutput = m_DtpOutput;
		}

		protected override void ClearSettingsFinal()
		{
			base.ClearSettingsFinal();

			m_DtpOutput = 1;
		}

		#endregion
	}
}