using ICD.Common.Utils.EventArguments;
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

		private readonly ComSpec m_ComSpec;

		private ISerialPort m_Port;
		private IDtpHdmiDevice m_Parent;

		#region Properties

		/// <summary>
		/// Gets the Com Spec configuration properties.
		/// </summary>
		public override IComSpecProperties ComSpecProperties { get { return m_ComSpecProperties; } }

		/// <summary>
		/// Gets the baud rate.
		/// </summary>
		public override eComBaudRates BaudRate { get { return m_ComSpec.BaudRate; } }

		/// <summary>
		/// Gets the number of data bits.
		/// </summary>
		public override eComDataBits NumberOfDataBits { get { return m_ComSpec.NumberOfDataBits; } }

		/// <summary>
		/// Gets the parity type.
		/// </summary>
		public override eComParityType ParityType { get { return m_ComSpec.ParityType; } }

		/// <summary>
		/// Gets the number of stop bits.
		/// </summary>
		public override eComStopBits NumberOfStopBits { get { return m_ComSpec.NumberOfStopBits; } }

		/// <summary>
		/// Gets the protocol type.
		/// </summary>
		public override eComProtocolType ProtocolType { get { return m_ComSpec.ProtocolType; } }

		/// <summary>
		/// Gets the hardware handshake mode.
		/// </summary>
		public override eComHardwareHandshakeType HardwareHandshake { get { return m_ComSpec.HardwareHandshake; } }

		/// <summary>
		/// Gets the software handshake mode.
		/// </summary>
		public override eComSoftwareHandshakeType SoftwareHandshake { get { return m_ComSpec.SoftwareHandshake; } }

		/// <summary>
		/// Gets the report CTS changes mode.
		/// </summary>
		public override bool ReportCtsChanges { get { return m_ComSpec.ReportCtsChanges; } }

		#endregion

		/// <summary>
		/// Constructor.
		/// </summary>
		public DtpCrosspointComPort()
		{
			m_ComSpec = new ComSpec();

			m_ComSpecProperties = new ComSpecProperties();
			m_ConnectionStateManager = new ConnectionStateManager(this);
			Subscribe(m_ConnectionStateManager);
		}

		/// <summary>
		/// Release resources.
		/// </summary>
		protected override void DisposeFinal(bool disposing)
		{
			base.DisposeFinal(disposing);

			Unsubscribe(m_ConnectionStateManager);
			m_ConnectionStateManager.Dispose();

			m_Port.Dispose();
		}

		#region Methods

		/// <summary>
		/// Configures the ComPort for communication.
		/// </summary>
		/// <param name="comSpec"></param>
		public override void SetComPortSpec(ComSpec comSpec)
		{
			m_Parent.InitializeComPort(comSpec.BaudRate, comSpec.NumberOfDataBits, comSpec.ParityType, comSpec.NumberOfStopBits);

			IComPort comPort = m_Port as IComPort;
			if (comPort != null)
				comPort.SetComPortSpec(comSpec);
		}

		/// <summary>
		/// Sends the data to the remote endpoint.
		/// </summary>
		protected override bool SendFinal(string data)
		{
			PrintTx(data);
			return m_ConnectionStateManager.Send(data);
		}

		/// <summary>
		/// Connects to the remote endpoint.
		/// </summary>
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

		/// <summary>
		/// Returns the connection state of the port.
		/// </summary>
		/// <returns></returns>
		protected override bool GetIsConnectedState()
		{
			return m_ConnectionStateManager.IsConnected;
		}

		#endregion

		#region Settings

		/// <summary>
		/// Override to apply settings to the instance.
		/// </summary>
		/// <param name="settings"></param>
		/// <param name="factory"></param>
		protected override void ApplySettingsFinal(DtpCrosspointComPortSettings settings, IDeviceFactory factory)
		{
			base.ApplySettingsFinal(settings, factory);

			m_Parent = factory.GetOriginatorById<IDtpHdmiDevice>(settings.Parent);
			if (m_Parent != null)
				Subscribe(m_Parent);
		}

		/// <summary>
		/// Override to apply properties to the settings instance.
		/// </summary>
		/// <param name="settings"></param>
		protected override void CopySettingsFinal(DtpCrosspointComPortSettings settings)
		{
			base.CopySettingsFinal(settings);

			settings.Parent = m_Parent.Id;

			ApplyConfiguration();
		}

		/// <summary>
		/// Override to clear the instance settings.
		/// </summary>
		protected override void ClearSettingsFinal()
		{
			if (m_Parent != null)
				Unsubscribe(m_Parent);
			m_Parent = null;

			base.ClearSettingsFinal();
		}

		#endregion

		#region Parent Callbacks

		/// <summary>
		/// Subscribe to the parent events.
		/// </summary>
		/// <param name="parent"></param>
		private void Subscribe(IDtpHdmiDevice parent)
		{
			parent.OnPortInitialized += ParentOnPortInitialized;
			parent.OnPortComSpecChanged += ParentOnPortComSpecChanged;
		}

		/// <summary>
		/// Unsubscribe from the parent events.
		/// </summary>
		/// <param name="parent"></param>
		private void Unsubscribe(IDtpHdmiDevice parent)
		{
			parent.OnPortInitialized -= ParentOnPortInitialized;
			parent.OnPortComSpecChanged -= ParentOnPortComSpecChanged;
		}

		/// <summary>
		/// Called when the parent initializes the serial port.
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="boolEventArgs"></param>
		private void ParentOnPortInitialized(object sender, BoolEventArgs boolEventArgs)
		{
			if (!IsConnected)
				Connect();
		}

		/// <summary>
		/// Called when the com spec changes.
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="genericEventArgs"></param>
		private void ParentOnPortComSpecChanged(object sender, GenericEventArgs<ComSpec> genericEventArgs)
		{
			m_ComSpec.Copy(genericEventArgs.Data);
		}

		#endregion

		#region Port Callbacks

		/// <summary>
		/// Subscribe to the connection state manager events.
		/// </summary>
		/// <param name="connectionStateManager"></param>
		private void Subscribe(ConnectionStateManager connectionStateManager)
		{
			connectionStateManager.OnSerialDataReceived += ConnectionStateManagerOnSerialDataReceived;
		}

		/// <summary>
		/// Unsubscribe from the connection state manager events.
		/// </summary>
		/// <param name="connectionStateManager"></param>
		private void Unsubscribe(ConnectionStateManager connectionStateManager)
		{
			connectionStateManager.OnSerialDataReceived -= ConnectionStateManagerOnSerialDataReceived;
		}

		/// <summary>
		/// Called when data is received from the remote endpoint.
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void ConnectionStateManagerOnSerialDataReceived(object sender, StringEventArgs e)
		{
			PrintRx(e.Data);
			Receive(e.Data);
		}

		#endregion
	}
}