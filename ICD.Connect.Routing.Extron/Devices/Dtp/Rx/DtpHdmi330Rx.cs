using ICD.Connect.Protocol.Ports;
using ICD.Connect.Protocol.Ports.ComPort;
using ICD.Connect.Settings.Core;

namespace ICD.Connect.Routing.Extron.Devices.Dtp.Rx
{
	public sealed class DtpHdmi330Rx : AbstractDtpHdmiDevice<DtpHdmi330RxSettings>
	{
		private int? m_DtpOutput;

		#region Methods

		public override HostInfo? GetComPortHostInfo()
		{
			if (m_DtpOutput == null)
				return null;

			return Parent.GetOutputComPortHostInfo(m_DtpOutput.Value);
		}

		public override void SetComPortSpec(eComBaudRates baudRate, eComDataBits dataBits, eComParityType parityType, eComStopBits stopBits)
		{
			if (m_DtpOutput == null)
				return;

			Parent.SetOutputComPortSpec(m_DtpOutput.Value, baudRate, dataBits, parityType, stopBits);
		}

		#endregion

		#region Settings

		protected override void ApplySettingsFinal(DtpHdmi330RxSettings settings, IDeviceFactory factory)
		{
			base.ApplySettingsFinal(settings, factory);

			m_DtpOutput = settings.DtpOutput;
		}

		protected override void CopySettingsFinal(DtpHdmi330RxSettings settings)
		{
			base.CopySettingsFinal(settings);

			settings.DtpOutput = m_DtpOutput;
		}

		protected override void ClearSettingsFinal()
		{
			base.ClearSettingsFinal();

			m_DtpOutput = null;
		}

		#endregion
	}
}