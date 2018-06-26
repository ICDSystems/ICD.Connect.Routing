using System;
using System.Collections.Generic;
using ICD.Common.Utils.EventArguments;
using ICD.Connect.API.Commands;
using ICD.Connect.Protocol.Network.Tcp;
using ICD.Connect.Protocol.Ports;
using ICD.Connect.Protocol.Ports.ComPort;
using ICD.Connect.Routing.Extron.Devices.Endpoints;
using ICD.Connect.Routing.Extron.Devices.Switchers;
using ICD.Connect.Settings.Core;

namespace ICD.Connect.Routing.Extron.Ports
{
	public class DtpCrosspointComPort : AbstractComPort<DtpCrosspointComPortSettings>
	{
		private readonly AsyncTcpClient m_Client;

		private IDtpHdmiDevice m_Parent;
		private eExtronPortInsertionMode m_Mode;
	    private HostInfo m_HostInfo;

		public DtpCrosspointComPort()
		{
			m_Client = new AsyncTcpClient();
            m_Client.OnSerialDataReceived += ClientOnSerialDataReceived;
		}

        protected override void DisposeFinal(bool disposing)
		{
			base.DisposeFinal(disposing);

            m_Client.OnSerialDataReceived -= null;
			m_Client.Dispose();
		}

		#region Methods

		public override void SetComPortSpec(eComBaudRates baudRate, eComDataBits numberOfDataBits, eComParityType parityType,
			eComStopBits numberOfStopBits, eComProtocolType protocolType, eComHardwareHandshakeType hardwareHandShake,
			eComSoftwareHandshakeType softwareHandshake, bool reportCtsChanges)
		{
			m_Parent.InitializeComPort(m_Mode, baudRate, numberOfDataBits, parityType, numberOfStopBits);
		}

		protected override bool SendFinal(string data)
		{
			PrintTx(data);
			return m_Client.Send(data);
		}

	    public override void Connect()
	    {
		    if (m_Mode == eExtronPortInsertionMode.Ethernet)
		    {
			    HostInfo? info = m_Parent.GetComPortHostInfo();
			    if (info == null)
				    throw new InvalidOperationException("Could not get host info to connect to DTP ComPort");

			    m_Client.Connect(info.Value);
		    }

			UpdateIsConnectedState();
	    }

	    protected override bool GetIsConnectedState()
	    {
		    if (m_Mode == eExtronPortInsertionMode.Ethernet)
			    return m_Client != null && m_Client.IsConnected;

			return true;
	    }

	    #endregion

		#region Settings

		protected override void ApplySettingsFinal(DtpCrosspointComPortSettings settings, IDeviceFactory factory)
		{
			base.ApplySettingsFinal(settings, factory);
			
            m_Mode = settings.Mode ?? eExtronPortInsertionMode.Ethernet;
			m_Parent = factory.GetOriginatorById<IDtpHdmiDevice>(settings.Parent);
            if (m_Parent != null)
			    Subscribe(m_Parent);
		}

		protected override void CopySettingsFinal(DtpCrosspointComPortSettings settings)
		{
			base.CopySettingsFinal(settings);

			settings.Parent = m_Parent.Id;
			settings.Mode = m_Mode;
		}

		protected override void ClearSettingsFinal()
		{
		    if (m_Parent != null)
		        Unsubscribe(m_Parent);
		    m_Parent = null;
			m_Mode = eExtronPortInsertionMode.CaptiveScrew;

			base.ClearSettingsFinal();
		}

		#endregion

		#region Parent Callbacks

		private void Subscribe(IDtpHdmiDevice parent)
		{
			parent.OnPortInitialized += ParentOnPortInitialized;
		}

		private void Unsubscribe(IDtpHdmiDevice parent)
		{
			parent.OnPortInitialized -= ParentOnPortInitialized;
		}

		private void ParentOnPortInitialized(object sender, BoolEventArgs boolEventArgs)
		{
			if(!IsConnected)
				Connect();
		}

        #endregion

        #region Client Callbacks

        private void ClientOnSerialDataReceived(object sender, StringEventArgs e)
        {
            PrintRx(e.Data);
            Receive(e.Data);
        }

        #endregion

        #region Console

        public override IEnumerable<IConsoleCommand> GetConsoleCommands()
        {
            foreach(var command in base.GetConsoleCommands())
                yield return command;

            yield return new GenericConsoleCommand<eComBaudRates, eComDataBits, eComParityType, eComStopBits>(
                "SetComPortSpec", "Sets the ComPort spec", 
                (a,b,c,d) => SetComPortSpec(a, b, c, d, 
                    eComProtocolType.ComspecProtocolRS232,
                    eComHardwareHandshakeType.ComspecHardwareHandshakeNone,
                    eComSoftwareHandshakeType.ComspecSoftwareHandshakeNone,
                    false));
        }

        #endregion
    }
}