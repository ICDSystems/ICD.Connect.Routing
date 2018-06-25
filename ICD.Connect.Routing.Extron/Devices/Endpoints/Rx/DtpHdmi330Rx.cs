﻿using ICD.Common.Utils.EventArguments;
using ICD.Connect.Protocol.Ports;
using ICD.Connect.Protocol.Ports.ComPort;
using ICD.Connect.Routing.Extron.Devices.Switchers;
using ICD.Connect.Settings.Core;

namespace ICD.Connect.Routing.Extron.Devices.Endpoints.Rx
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

		public override void InitializeComPort(eExtronPortInsertionMode mode, eComBaudRates baudRate, eComDataBits dataBits, eComParityType parityType, eComStopBits stopBits)
		{
			if (m_DtpOutput == null)
				return;

			Parent.InitializeRxComPort(m_DtpOutput.Value, mode, baudRate, dataBits, parityType, stopBits);
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