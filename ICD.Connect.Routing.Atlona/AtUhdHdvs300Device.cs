﻿using System;
using ICD.Common.Properties;
using ICD.Common.Utils.EventArguments;
using ICD.Common.Utils.Extensions;
using ICD.Common.Utils.Services.Logging;
using ICD.Common.Utils.Timers;
using ICD.Connect.API.Nodes;
using ICD.Connect.Devices;
using ICD.Connect.Protocol;
using ICD.Connect.Protocol.Extensions;
using ICD.Connect.Protocol.Ports;
using ICD.Connect.Protocol.Ports.ComPort;
using ICD.Connect.Settings.Core;

namespace ICD.Connect.Routing.Atlona
{
	public sealed class AtUhdHdvs300Device : AbstractDevice<AtUhdHdvs300DeviceSettings>
	{
		/// <summary>
		/// The device likes to drop connection if there's no activity for 5 mins,
		/// so lets occasionally send something to keep connection from dropping.
		/// </summary>
		private const long KEEPALIVE_INTERVAL = 2 * 60 * 1000;

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
		private readonly SafeTimer m_KeepAliveTimer;
		private readonly ConnectionStateManager m_ConnectionStateManager;

		private bool m_Initialized;

		#region Properties

		/// <summary>
		/// Gets/sets the username for logging into the device.
		/// </summary>
		public string Username { get; set; }

		/// <summary>
		/// Gets/sets the password for logging into the device.
		/// </summary>
		public string Password { get; set; }

		/// <summary>
		/// Returns true when the codec is connected.
		/// </summary>
		public bool IsConnected
		{
			get { return m_ConnectionStateManager.IsConnected; }
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
			m_ConnectionStateManager = new ConnectionStateManager(this){ ConfigurePort = ConfigurePort };
			m_ConnectionStateManager.OnConnectedStateChanged += PortOnConnectionStatusChanged;
			m_ConnectionStateManager.OnIsOnlineStateChanged += PortOnIsOnlineStateChanged;
			m_ConnectionStateManager.OnSerialDataReceived += PortOnSerialDataReceived;

			m_SerialBuffer = new AtUhdHdvs300DeviceSerialBuffer();
			Subscribe(m_SerialBuffer);

			Controls.Add(new AtUhdHdvs300SwitcherControl(this, 0));

			m_KeepAliveTimer = SafeTimer.Stopped(KeepAliveCallback);
			m_KeepAliveTimer.Reset(KEEPALIVE_INTERVAL, KEEPALIVE_INTERVAL);
		}

		/// <summary>
		/// Release resources.
		/// </summary>
		protected override void DisposeFinal(bool disposing)
		{
			OnConnectedStateChanged = null;
			OnInitializedChanged = null;
			OnResponseReceived = null;

			m_ConnectionStateManager.OnConnectedStateChanged -= PortOnConnectionStatusChanged;
			m_ConnectionStateManager.OnIsOnlineStateChanged -= PortOnIsOnlineStateChanged;
			m_ConnectionStateManager.OnSerialDataReceived -= PortOnSerialDataReceived;
			m_ConnectionStateManager.Dispose();

			m_KeepAliveTimer.Dispose();

			Unsubscribe(m_SerialBuffer);

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

			m_ConnectionStateManager.Send(command + '\r');
		}

		/// <summary>
		/// Sets the port for communicating with the device.
		/// </summary>
		/// <param name="port"></param>
		[PublicAPI]
		public void ConfigurePort(ISerialPort port)
		{
			if (port is IComPort)
				ConfigureComPort(port as IComPort);
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
		/// Gets the current online status of the device.
		/// </summary>
		/// <returns></returns>
		protected override bool GetIsOnlineStatus()
		{
			return m_ConnectionStateManager != null && m_ConnectionStateManager.IsOnline;
		}

		/// <summary>
		/// Initialize the CODEC.
		/// </summary>
		private void Initialize()
		{
			EnableFeedback();

			Initialized = true;
		}

		/// <summary>
		/// Called periodically to keep the connection alive.
		/// </summary>
		private void KeepAliveCallback()
		{
			if (Initialized)
				EnableFeedback();
		}

		/// <summary>
		/// Tell the device to echo any status changes.
		/// </summary>
		private void EnableFeedback()
		{
			SendCommand("Broadcast on");
			SendCommand("InputBroadcast on");
		}

		#endregion

		#region Port Callbacks

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

			OnConnectedStateChanged.Raise(this, new BoolEventArgs(args.Data));
		}

		/// <summary>
		/// Called when the port online status changes.
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="args"></param>
		private void PortOnIsOnlineStateChanged(object sender, BoolEventArgs args)
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
				Log(eSeverity.Error, args.Data);
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

			settings.Port = m_ConnectionStateManager.PortNumber;
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
			m_ConnectionStateManager.SetPort(null);
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
					Log(eSeverity.Error, "No serial port with id {0}", settings.Port);
			}

			m_ConnectionStateManager.SetPort(port);
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
