using ICD.Common.Utils.EventArguments;
using ICD.Connect.Protocol.Ports;
using ICD.Connect.Protocol.Ports.ComPort;
using ICD.Connect.Routing.Extron.Devices.Switchers;
using ICD.Connect.Settings.Core;

namespace ICD.Connect.Routing.Extron.Devices.Endpoints.Tx
{
	public class DtpHdmi330Tx : AbstractDtpHdmiDevice<DtpHdmi330TxSettings>
	{
		private int? m_DtpInput;

		#region Methods

		public override HostInfo? GetComPortHostInfo()
		{
			if (m_DtpInput == null)
				return null;

			return Parent.GetInputComPortHostInfo(m_DtpInput.Value);
		}

		public override void InitializeComPort(eExtronPortInsertionMode mode, eComBaudRates baudRate, eComDataBits dataBits, eComParityType parityType, eComStopBits stopBits)
		{
			if (m_DtpInput == null)
				return;

			Parent.InitializeTxComPort(m_DtpInput.Value, mode, baudRate, dataBits, parityType, stopBits);
		}

		#endregion

		#region Parent Callbacks

		protected override void Subscribe(IDtpCrosspointDevice parent)
		{
			base.Subscribe(parent);

			parent.OnInputPortInitialized += ParentOnOnInputPortInitialized;
		}

		protected override void Unsubscribe(IDtpCrosspointDevice parent)
		{
			base.Unsubscribe(parent);

			parent.OnInputPortInitialized -= ParentOnOnInputPortInitialized;
		}

		private void ParentOnOnInputPortInitialized(object sender, IntEventArgs args)
		{
			if (args.Data == m_DtpInput)
				PortInitialized = true;
		}

		#endregion

		#region Settings

		protected override void ApplySettingsFinal(DtpHdmi330TxSettings settings, IDeviceFactory factory)
		{
			base.ApplySettingsFinal(settings, factory);

			m_DtpInput = settings.DtpInput;
		}

		protected override void CopySettingsFinal(DtpHdmi330TxSettings settings)
		{
			base.CopySettingsFinal(settings);

			settings.DtpInput = m_DtpInput;
		}

		protected override void ClearSettingsFinal()
		{
			base.ClearSettingsFinal();

			m_DtpInput = null;
		}

		#endregion
	}
}