using ICD.Connect.Settings;

namespace ICD.Connect.Routing.Extron.Devices.Endpoints.Tx
{
	public sealed class DtpHdmiTx : AbstractDtpHdmiDevice<DtpHdmiTxSettings>
	{
		private int m_DtpInput;

		#region Properties

		/// <summary>
		/// Gets the address where this endpoint is connected to the switcher.
		/// </summary>
		public override int SwitcherAddress { get { return m_DtpInput; } }

		/// <summary>
		/// Returns Input for TX, Output for RX.
		/// </summary>
		public override eDtpInputOuput SwitcherInputOutput { get { return eDtpInputOuput.Input; } }

		#endregion

		#region Settings

		protected override void ApplySettingsFinal(DtpHdmiTxSettings settings, IDeviceFactory factory)
		{
			base.ApplySettingsFinal(settings, factory);

			m_DtpInput = settings.DtpInput;
		}

		protected override void CopySettingsFinal(DtpHdmiTxSettings settings)
		{
			base.CopySettingsFinal(settings);

			settings.DtpInput = m_DtpInput;
		}

		protected override void ClearSettingsFinal()
		{
			base.ClearSettingsFinal();

			m_DtpInput = 1;
		}

		#endregion
	}
}