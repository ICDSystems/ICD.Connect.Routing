using ICD.Common.Utils.EventArguments;
using ICD.Connect.Protocol.Ports;
using ICD.Connect.Protocol.Ports.ComPort;
using ICD.Connect.Routing.Extron.Devices.Switchers;
using ICD.Connect.Routing.Extron.Devices.Switchers.DtpCrosspoint;
using ICD.Connect.Settings.Core;

namespace ICD.Connect.Routing.Extron.Devices.Endpoints.Rx
{
	public sealed class DtpHdmiRx : AbstractDtpHdmiDevice<DtpHdmiRxSettings>
	{
		private int? m_DtpOutput;

		#region Methods

		public override ISerialPort GetSerialInsertionPort()
		{
			if (m_DtpOutput == null)
				return null;

			return Parent.GetOutputSerialInsertionPort(m_DtpOutput.Value);
		}

		public override void InitializeComPort(eComBaudRates baudRate, eComDataBits dataBits, eComParityType parityType, eComStopBits stopBits)
		{
			if (m_DtpOutput == null)
				return;

			Parent.SetRxComPortSpec(m_DtpOutput.Value, baudRate, dataBits, parityType, stopBits);
		}

		#endregion

		#region Parent Callbacks

		protected override void Subscribe(IDtpCrosspointDevice parent)
		{
			base.Subscribe(parent);

			parent.OnOutputPortInitialized += ParentOnOutputPortInitialized;
		}

		protected override void Unsubscribe(IDtpCrosspointDevice parent)
		{
			base.Unsubscribe(parent);

			parent.OnOutputPortInitialized -= ParentOnOutputPortInitialized;
		}

		private void ParentOnOutputPortInitialized(object sender, IntEventArgs args)
		{
			if (args.Data == m_DtpOutput)
				PortInitialized = true;
		}

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

			m_DtpOutput = null;
		}

		#endregion
	}
}