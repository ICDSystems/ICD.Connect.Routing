using System;
using System.IO;
using ICD.Common.Utils.EventArguments;
using ICD.Connect.Protocol.Network.Tcp;
using ICD.Connect.Protocol.Ports;
using ICD.Connect.Protocol.Ports.ComPort;
using ICD.Connect.Routing.Extron.Devices.Dtp;
using ICD.Connect.Settings.Core;

namespace ICD.Connect.Routing.Extron.Ports
{
	public class DtpCrosspointComPort : AbstractComPort<DtpCrosspointComPortSettings>
	{
		private IDtpHdmiDevice m_Parent;
		private readonly AsyncTcpClient m_Client;

		public DtpCrosspointComPort()
		{
			m_Client = new AsyncTcpClient();
		}

		protected override void DisposeFinal(bool disposing)
		{
			base.DisposeFinal(disposing);

			m_Client.Dispose();
		}

		#region Methods

		public override void SetComPortSpec(eComBaudRates baudRate, eComDataBits numberOfDataBits, eComParityType parityType,
			eComStopBits numberOfStopBits, eComProtocolType protocolType, eComHardwareHandshakeType hardwareHandShake,
			eComSoftwareHandshakeType softwareHandshake, bool reportCtsChanges)
		{
			m_Parent.SetComPortSpec(baudRate, numberOfDataBits, parityType, numberOfStopBits);
		}

		protected override bool SendFinal(string data)
		{
			PrintTx(data);
			return m_Client.Send(data);
		}

		#endregion

		#region Settings

		protected override void ApplySettingsFinal(DtpCrosspointComPortSettings settings, IDeviceFactory factory)
		{
			base.ApplySettingsFinal(settings, factory);
			
			m_Parent = factory.GetOriginatorById<IDtpHdmiDevice>(settings.Parent);
			Subscribe(m_Parent);
		}

		protected override void CopySettingsFinal(DtpCrosspointComPortSettings settings)
		{
			base.CopySettingsFinal(settings);

			settings.Parent = m_Parent.Id;
		}

		protected override void ClearSettingsFinal()
		{
			base.ClearSettingsFinal();

			Unsubscribe(m_Parent);
			m_Parent = null;
		}

		#endregion

		#region Parent Callbacks

		private void Subscribe(IDtpHdmiDevice parent)
		{
			parent.OnInitializedChanged += ParentOnOnInitializedChanged;
		}

		private void Unsubscribe(IDtpHdmiDevice parent)
		{
			parent.OnInitializedChanged -= ParentOnOnInitializedChanged;
		}

		private void ParentOnOnInitializedChanged(object sender, BoolEventArgs boolEventArgs)
		{
			m_Client.Disconnect();
			HostInfo? info = m_Parent.GetComPortHostInfo();
			if (info == null)
				throw new InvalidDataException("Could not get host info to connect to DTP ComPort");

			m_Client.Connect(info.Value);
		}

		#endregion
	}
}