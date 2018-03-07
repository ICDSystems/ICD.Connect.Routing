using System;
using ICD.Common.Properties;
using ICD.Common.Utils.EventArguments;
using ICD.Common.Utils.Extensions;
using ICD.Common.Utils.Services.Logging;
using ICD.Connect.API.Nodes;
using ICD.Connect.Devices;
using ICD.Connect.Protocol.Extensions;
using ICD.Connect.Protocol.Heartbeat;
using ICD.Connect.Protocol.Ports;
using ICD.Connect.Protocol.Ports.ComPort;
using ICD.Connect.Settings.Core;

namespace ICD.Connect.Routing.Atlona
{
	public sealed class AtUhdHdvs300Device : AbstractDevice<AtUhdHdvs300DeviceSettings>, IConnectable
	{
		/// <summary>
		/// Raised when the device becomes connected or disconnected.
		/// </summary>
		public event EventHandler<BoolEventArgs> OnConnectedStateChanged;

		/// <summary>
		/// Raised when the class initializes.
		/// </summary>
		public event EventHandler<BoolEventArgs> OnInitializedChanged;

		/// <summary>
		/// Raised when the device sends a response.
		/// </summary>
		public event EventHandler<StringEventArgs> OnResponseReceived; 

		private readonly AtUhdHdvs300DeviceSerialBuffer m_SerialBuffer;

		private bool m_Initialized;
		private bool m_IsConnected;
		private ISerialPort m_Port;

		#region Properties

		/// <summary>
		/// Gets/sets the username for logging into the device.
		/// </summary>
		public string Username { get; set; }

		/// <summary>
		/// Gets/sets the password for logging into the device.
		/// </summary>
		public string Password { get; set; }

		public Heartbeat Heartbeat { get; private set; }

		/// <summary>
		/// Returns true when the codec is connected.
		/// </summary>
		public bool IsConnected
		{
			get { return m_IsConnected; }
			private set
			{
				if (value == m_IsConnected)
					return;

				m_IsConnected = value;

				if (!m_IsConnected)
					Initialized = false;

				OnConnectedStateChanged.Raise(this, new BoolEventArgs(m_IsConnected));
			}
		}

		/// <summary>
		/// Device Initialized Status.
		/// </summary>
		public bool Initialized
		{
			get { return m_Initialized; }
			private set
			{
				if (value == m_Initialized)
					return;

				m_Initialized = value;

				OnInitializedChanged.Raise(this, new BoolEventArgs(m_Initialized));
			}
		}

		#endregion

		/// <summary>
		/// Constructor.
		/// </summary>
		public AtUhdHdvs300Device()
		{
			Heartbeat = new Heartbeat(this);

			m_SerialBuffer = new AtUhdHdvs300DeviceSerialBuffer();
			Subscribe(m_SerialBuffer);

			Controls.Add(new AtUhdHdvs300SwitcherControl(this, 0));
		}

		/// <summary>
		/// Release resources.
		/// </summary>
		protected override void DisposeFinal(bool disposing)
		{
			OnConnectedStateChanged = null;
			OnInitializedChanged = null;
			OnResponseReceived = null;

			Heartbeat.StopMonitoring();
			Heartbeat.Dispose();

			Unsubscribe(m_SerialBuffer);
			Unsubscribe(m_Port);

			base.DisposeFinal(disposing);
		}

		#region Methods

		/// <summary>
		/// Send command.
		/// </summary>
		/// <param name="command"></param>
		public void SendCommand(string command)
		{
			SendCommand(command, null);
		}

		/// <summary>
		/// Send command.
		/// </summary>
		/// <param name="command"></param>
		/// <param name="args"></param>
		public void SendCommand(string command, params object[] args)
		{
			if (args != null)
				command = string.Format(command, args);

			if (m_Port == null)
			{
				Logger.AddEntry(eSeverity.Error, "{0} - Unable to communicate - port is null");
				return;
			}

			if (!IsConnected)
			{
				Logger.AddEntry(eSeverity.Warning, "{0} - Disconnected, attempting reconnect");
				Connect();
			}

			if (!IsConnected)
			{
				Logger.AddEntry(eSeverity.Critical, "{0} - Unable to connect");
				return;
			}

			m_Port.Send(command + '\r');
		}

		/// <summary>
		/// Sets the port for communicating with the device.
		/// </summary>
		/// <param name="port"></param>
		[PublicAPI]
		public void SetPort(ISerialPort port)
		{
			if (port == m_Port)
				return;

			if (port is IComPort)
				ConfigureComPort(port as IComPort);

			if (m_Port != null)
				Disconnect();

			Unsubscribe(m_Port);
			m_Port = port;
			Subscribe(m_Port);

			if (m_Port != null)
			{
				m_Port.DebugRx = true;
				m_Port.DebugTx = true;
				Connect();
			}

			Heartbeat.StartMonitoring();

			UpdateCachedOnlineStatus();
		}

		/// <summary>
		/// Configures a com port for communication with the hardware.
		/// </summary>
		/// <param name="port"></param>
		[PublicAPI]
		public static void ConfigureComPort(IComPort port)
		{
			port.SetComPortSpec(eComBaudRates.ComspecBaudRate115200,
			                    eComDataBits.ComspecDataBits8,
			                    eComParityType.ComspecParityNone,
			                    eComStopBits.ComspecStopBits1,
			                    eComProtocolType.ComspecProtocolRS232,
			                    eComHardwareHandshakeType.ComspecHardwareHandshakeNone,
			                    eComSoftwareHandshakeType.ComspecSoftwareHandshakeNone,
			                    false);
		}

		/// <summary>
		/// Connect to the codec.
		/// </summary>
		[PublicAPI]
		public void Connect()
		{
			if (m_Port == null)
			{
				Logger.AddEntry(eSeverity.Critical, "{0} - Unable to connect, port is null");
				return;
			}

			m_Port.Connect();
		}

		/// <summary>
		/// Disconnect from the codec.
		/// </summary>
		[PublicAPI]
		public void Disconnect()
		{
			if (m_Port == null)
			{
				Logger.AddEntry(eSeverity.Critical, "{0} - Unable to disconnect, port is null");
				return;
			}

			m_Port.Disconnect();
		}

		/// <summary>
		/// Gets the current online status of the device.
		/// </summary>
		/// <returns></returns>
		protected override bool GetIsOnlineStatus()
		{
			return m_Port != null && m_Port.IsOnline;
		}

		/// <summary>
		/// Initialize the CODEC.
		/// </summary>
		private void Initialize()
		{
			SendCommand("Broadcast on");
			SendCommand("InputBroadcast on");

			Initialized = true;
		}

		#endregion

		#region Port Callbacks

		/// <summary>
		/// Subscribes to the port events.
		/// </summary>
		/// <param name="port"></param>
		private void Subscribe(ISerialPort port)
		{
			if (port == null)
				return;

			port.OnSerialDataReceived += PortOnSerialDataReceived;
			port.OnConnectedStateChanged += PortOnConnectionStatusChanged;
			port.OnIsOnlineStateChanged += PortOnIsOnlineStateChanged;
		}

		/// <summary>
		/// Unsubscribe from the port events.
		/// </summary>
		/// <param name="port"></param>
		private void Unsubscribe(ISerialPort port)
		{
			if (port == null)
				return;

			port.OnSerialDataReceived -= PortOnSerialDataReceived;
			port.OnConnectedStateChanged -= PortOnConnectionStatusChanged;
			port.OnIsOnlineStateChanged -= PortOnIsOnlineStateChanged;
		}

		/// <summary>
		/// Called when serial data is recieved from the port.
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="args"></param>
		private void PortOnSerialDataReceived(object sender, StringEventArgs args)
		{
			m_SerialBuffer.Enqueue(args.Data);
		}

		/// <summary>
		/// Called when the port connection status changes.
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="args"></param>
		private void PortOnConnectionStatusChanged(object sender, BoolEventArgs args)
		{
			m_SerialBuffer.Clear();

			IsConnected = args.Data;
		}

		/// <summary>
		/// Called when the port online status changes.
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="boolEventArgs"></param>
		private void PortOnIsOnlineStateChanged(object sender, BoolEventArgs boolEventArgs)
		{
			UpdateCachedOnlineStatus();
		}

		#endregion

		#region Buffer Callbacks

		/// <summary>
		/// Subscribes to the buffer events.
		/// </summary>
		/// <param name="buffer"></param>
		private void Subscribe(AtUhdHdvs300DeviceSerialBuffer buffer)
		{
			buffer.OnCompletedSerial += BufferCompletedSerial;
			buffer.OnLoginPrompt += BufferOnOnLoginPrompt;
			buffer.OnPasswordPrompt += BufferOnOnPasswordPrompt;
			buffer.OnEmptyPrompt += BufferOnOnEmptyPrompt;
		}

		/// <summary>
		/// Unsubscribe from the buffer events.
		/// </summary>
		/// <param name="buffer"></param>
		private void Unsubscribe(AtUhdHdvs300DeviceSerialBuffer buffer)
		{
			buffer.OnCompletedSerial -= BufferCompletedSerial;
			buffer.OnLoginPrompt -= BufferOnOnLoginPrompt;
			buffer.OnPasswordPrompt -= BufferOnOnPasswordPrompt;
			buffer.OnEmptyPrompt -= BufferOnOnEmptyPrompt;
		}

		private void BufferOnOnEmptyPrompt(object sender, EventArgs eventArgs)
		{
			// The empty prompt represents a successful login
			Initialize();
		}

		private void BufferOnOnPasswordPrompt(object sender, EventArgs eventArgs)
		{
			SendCommand(Password);
		}

		private void BufferOnOnLoginPrompt(object sender, EventArgs eventArgs)
		{
			SendCommand(Username);
		}

		/// <summary>
		/// Called when the buffer completes a string.
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="args"></param>
		private void BufferCompletedSerial(object sender, StringEventArgs args)
		{
			OnResponseReceived.Raise(this, new StringEventArgs(args.Data));

			if (args.Data.StartsWith("Command FAILED:"))
				Logger.AddEntry(eSeverity.Error, "{0} - {1}", this, args.Data);
		}

		#endregion

		#region Settings

		/// <summary>
		/// Override to apply properties to the settings instance.
		/// </summary>
		/// <param name="settings"></param>
		protected override void CopySettingsFinal(AtUhdHdvs300DeviceSettings settings)
		{
			base.CopySettingsFinal(settings);

			settings.Port = m_Port == null ? (int?)null : m_Port.Id;
			settings.Username = Username;
			settings.Password = Password;
		}

		/// <summary>
		/// Override to clear the instance settings.
		/// </summary>
		protected override void ClearSettingsFinal()
		{
			base.ClearSettingsFinal();

			Username = null;
			Password = null;
			SetPort(null);
		}

		/// <summary>
		/// Override to apply settings to the instance.
		/// </summary>
		/// <param name="settings"></param>
		/// <param name="factory"></param>
		protected override void ApplySettingsFinal(AtUhdHdvs300DeviceSettings settings, IDeviceFactory factory)
		{
			base.ApplySettingsFinal(settings, factory);

			Username = settings.Username;
			Password = settings.Password;

			ISerialPort port = null;

			if (settings.Port != null)
			{
				port = factory.GetPortById((int)settings.Port) as ISerialPort;
				if (port == null)
					Logger.AddEntry(eSeverity.Error, "No serial port with id {0}", settings.Port);
			}

			SetPort(port);
		}

		#endregion

		#region Console

		/// <summary>
		/// Calls the delegate for each console status item.
		/// </summary>
		/// <param name="addRow"></param>
		public override void BuildConsoleStatus(AddStatusRowDelegate addRow)
		{
			base.BuildConsoleStatus(addRow);

			addRow("Connected", IsConnected);
		}

		#endregion
	}
}
