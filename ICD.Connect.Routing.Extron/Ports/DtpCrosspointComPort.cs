using System.Collections.Generic;
using ICD.Common.Utils.EventArguments;
using ICD.Connect.API.Commands;
using ICD.Connect.Protocol;
using ICD.Connect.Protocol.Ports;
using ICD.Connect.Protocol.Ports.ComPort;
using ICD.Connect.Protocol.Settings;
using ICD.Connect.Routing.Extron.Devices.Endpoints;
using ICD.Connect.Settings;

namespace ICD.Connect.Routing.Extron.Ports
{
	public sealed class DtpCrosspointComPort : AbstractComPort<DtpCrosspointComPortSettings>
	{
		private readonly ComSpecProperties m_ComSpecProperties;

		private readonly ConnectionStateManager m_ConnectionStateManager;

		private ISerialPort m_Port;
		private IDtpHdmiDevice m_Parent;
		private bool m_Initialized;

		#region Properties

		/// <summary>
		/// Gets the Com Spec configuration properties.
		/// </summary>
		protected override IComSpecProperties ComSpecProperties { get { return m_ComSpecProperties; } }

		#endregion

		public DtpCrosspointComPort()
		{
			m_ComSpecProperties = new ComSpecProperties();
			m_ConnectionStateManager = new ConnectionStateManager(this);
			Subscribe(m_ConnectionStateManager);
		}

		protected override void DisposeFinal(bool disposing)
		{
			base.DisposeFinal(disposing);

			Unsubscribe(m_ConnectionStateManager);
			m_ConnectionStateManager.Dispose();

			m_Port.Dispose();
		}

		#region Methods

		public override void SetComPortSpec(eComBaudRates baudRate, eComDataBits numberOfDataBits, eComParityType parityType,
			eComStopBits numberOfStopBits, eComProtocolType protocolType, eComHardwareHandshakeType hardwareHandShake,
			eComSoftwareHandshakeType softwareHandshake, bool reportCtsChanges)
		{
			m_Parent.InitializeComPort(baudRate, numberOfDataBits, parityType, numberOfStopBits);

			var comPort = m_Port as IComPort;
			if (comPort != null)
				comPort.SetComPortSpec(
					baudRate, numberOfDataBits,
					parityType, numberOfStopBits,
					protocolType, hardwareHandShake,
					softwareHandshake, reportCtsChanges);
		}

		protected override bool SendFinal(string data)
		{
			PrintTx(data);
			return m_ConnectionStateManager.Send(data);
		}

		public override void Connect()
		{
			if (m_ConnectionStateManager.PortNumber == null)
			{
				m_Port = m_Parent.GetSerialInsertionPort();
				m_ConnectionStateManager.SetPort(m_Port);
			}

			m_ConnectionStateManager.Connect();

			UpdateIsConnectedState();
		}

		protected override bool GetIsConnectedState()
		{
			return m_ConnectionStateManager.IsConnected;
		}

		#endregion

		#region Settings

		protected override void ApplySettingsFinal(DtpCrosspointComPortSettings settings, IDeviceFactory factory)
		{
			base.ApplySettingsFinal(settings, factory);

			m_ComSpecProperties.Copy(settings);

			m_Parent = factory.GetOriginatorById<IDtpHdmiDevice>(settings.Parent);
			if (m_Parent != null)
				Subscribe(m_Parent);
		}

		protected override void CopySettingsFinal(DtpCrosspointComPortSettings settings)
		{
			base.CopySettingsFinal(settings);

			settings.Parent = m_Parent.Id;

			settings.Copy(m_ComSpecProperties);
		}

		protected override void ClearSettingsFinal()
		{
			if (m_Parent != null)
				Unsubscribe(m_Parent);
			m_Parent = null;

			m_ComSpecProperties.Clear();

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
			if (!IsConnected)
				Connect();
		}

		#endregion

		#region Port Callbacks

		private void Subscribe(ConnectionStateManager connectionStateManager)
		{
			connectionStateManager.OnSerialDataReceived += ConnectionStateManagerOnSerialDataReceived;
		}

		private void Unsubscribe(ConnectionStateManager connectionStateManager)
		{
			connectionStateManager.OnSerialDataReceived -= ConnectionStateManagerOnSerialDataReceived;
		}

		private void ConnectionStateManagerOnSerialDataReceived(object sender, StringEventArgs e)
		{
			PrintRx(e.Data);
			Receive(e.Data);
		}

		#endregion

		#region Console

		public override IEnumerable<IConsoleCommand> GetConsoleCommands()
		{
			foreach (IConsoleCommand command in GetBaseConsoleCommands())
				yield return command;

			yield return new GenericConsoleCommand<eComBaudRates, eComDataBits, eComParityType, eComStopBits>(
				"SetComPortSpec", "Sets the ComPort spec",
				(a, b, c, d) => SetComPortSpec(a, b, c, d,
					eComProtocolType.ComspecProtocolRS232,
					eComHardwareHandshakeType.ComspecHardwareHandshakeNone,
					eComSoftwareHandshakeType.ComspecSoftwareHandshakeNone,
					false));
		}

		/// <summary>
		/// Workaround for "unverifiable code" warning.
		/// </summary>
		/// <returns></returns>
		private IEnumerable<IConsoleCommand> GetBaseConsoleCommands()
		{
			return base.GetConsoleCommands();
		}

		#endregion
	}
}