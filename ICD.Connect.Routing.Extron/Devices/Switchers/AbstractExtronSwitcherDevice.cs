﻿using System;
using System.Collections.Generic;
using ICD.Common.Utils.EventArguments;
using ICD.Common.Utils.Extensions;
using ICD.Common.Utils.Services.Logging;
using ICD.Common.Utils.Timers;
using ICD.Connect.API.Nodes;
using ICD.Connect.Devices;
using ICD.Connect.Protocol;
using ICD.Connect.Protocol.Extensions;
using ICD.Connect.Protocol.Network.Ports;
using ICD.Connect.Protocol.Network.Settings;
using ICD.Connect.Protocol.Ports;
using ICD.Connect.Protocol.Ports.ComPort;
using ICD.Connect.Protocol.Settings;
using ICD.Connect.Routing.Extron.SerialBuffers;
using ICD.Connect.Settings;

namespace ICD.Connect.Routing.Extron.Devices.Switchers
{
	public abstract class AbstractExtronSwitcherDevice<TSettings> : AbstractDevice<TSettings>, IExtronSwitcherDevice
		where TSettings : IExtronSwitcherDeviceSettings, new()
	{
		/// <summary>
		/// The device likes to drop connection if there's no activity for 5 mins,
		/// so lets occasionally send something to keep connection from dropping.
		/// </summary>
		private const long KEEPALIVE_INTERVAL = 2 * 60 * 1000;

		/// <summary>
		/// Raised when the class initializes.
		/// </summary>
		public event EventHandler<BoolEventArgs> OnInitializedChanged;

		/// <summary>
		/// Raised when the device sends a response.
		/// </summary>
		public event EventHandler<StringEventArgs> OnResponseReceived;

		private readonly DtpCrosspointSerialBuffer m_SerialBuffer;
		private readonly SafeTimer m_KeepAliveTimer;
		private readonly ConnectionStateManager m_ConnectionStateManager;

		private readonly NetworkProperties m_NetworkProperties;
		private readonly ComSpecProperties m_ComSpecProperties;

		private bool m_Initialized;
		
		#region Properties

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
		protected AbstractExtronSwitcherDevice()
		{
			m_NetworkProperties = new NetworkProperties();
			m_ComSpecProperties = new ComSpecProperties();

			m_SerialBuffer = new DtpCrosspointSerialBuffer();
			Subscribe(m_SerialBuffer);

			m_ConnectionStateManager = new ConnectionStateManager(this) { ConfigurePort = ConfigurePort };
			Subscribe(m_ConnectionStateManager);

			m_KeepAliveTimer = SafeTimer.Stopped(KeepAliveCallback);
			m_KeepAliveTimer.Reset(KEEPALIVE_INTERVAL, KEEPALIVE_INTERVAL);
		}

		/// <summary>
		/// Release resources.
		/// </summary>
		protected override void DisposeFinal(bool disposing)
		{
			OnInitializedChanged = null;
			OnResponseReceived = null;

			Unsubscribe(m_ConnectionStateManager);
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
		/// <param name="args"></param>
		public void SendCommand(string command, params object[] args)
		{
			if (args != null)
				command = string.Format(command, args);

			m_ConnectionStateManager.Send(command + '\r');
		}

		/// <summary>
		/// Configures the given port for communication with the device.
		/// </summary>
		/// <param name="port"></param>
		private void ConfigurePort(IPort port)
		{
			// Com
			if (port is IComPort)
				(port as IComPort).ApplyDeviceConfiguration(m_ComSpecProperties);

			// TCP
			else if (port is INetworkPort)
				(port as INetworkPort).ApplyDeviceConfiguration(m_NetworkProperties);
		}

		/// <summary>
		/// Gets the current online status of the device.
		/// </summary>
		/// <returns></returns>
		protected override bool GetIsOnlineStatus()
		{
			return m_ConnectionStateManager != null && m_ConnectionStateManager.IsOnline;
		}

		#endregion

		#region Private Methods

		protected virtual void Initialize()
		{
			SetVerboseMode(eExtronVerbosity.All);
		}

		private void KeepAliveCallback()
		{
			if (Initialized)
				SetVerboseMode(eExtronVerbosity.All);
		}

		private void SetVerboseMode(eExtronVerbosity verbosity)
		{
			SendCommand("W{0}CV", (ushort)verbosity);
		}

		private void SetPort(ISerialPort port)
		{
			m_ConnectionStateManager.SetPort(port, false);
		}

		#endregion

		#region Buffer Callbacks

		/// <summary>
		/// Subscribe to the buffer events.
		/// </summary>
		/// <param name="buffer"></param>
		private void Subscribe(DtpCrosspointSerialBuffer buffer)
		{
			buffer.OnPasswordPrompt += BufferOnPasswordPrompt;
			buffer.OnEmptyPrompt += BufferOnEmptyPrompt;
			buffer.OnCompletedSerial += BufferOnCompletedSerial;
		}

		/// <summary>
		/// Unsubscribe from the buffer events.
		/// </summary>
		/// <param name="buffer"></param>
		private void Unsubscribe(DtpCrosspointSerialBuffer buffer)
		{
			buffer.OnPasswordPrompt -= BufferOnPasswordPrompt;
			buffer.OnEmptyPrompt -= BufferOnEmptyPrompt;
			buffer.OnCompletedSerial -= BufferOnCompletedSerial;
		}

		/// <summary>
		/// Called when the device prompts for a password.
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="eventArgs"></param>
		private void BufferOnPasswordPrompt(object sender, EventArgs eventArgs)
		{
			SendCommand(Password);
		}

		/// <summary>
		/// Called when the device provides an empty prompt.
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="eventArgs"></param>
		private void BufferOnEmptyPrompt(object sender, EventArgs eventArgs)
		{
			if (!Initialized)
				Initialize();
		}

		/// <summary>
		/// Called when we receive a complete response from the device.
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="args"></param>
		protected virtual void BufferOnCompletedSerial(object sender, StringEventArgs args)
		{
			OnResponseReceived.Raise(this, new StringEventArgs(args.Data));

			if (args.Data.StartsWith("Vrb"))
			{
				eExtronVerbosity verbosity = (eExtronVerbosity)ushort.Parse(args.Data.Substring(3));
				if (verbosity == eExtronVerbosity.All)
					Initialized = true;
				else
				{
					Initialized = false;
					SetVerboseMode(eExtronVerbosity.All);
				}
			}
		}

		#endregion

		#region Port Callbacks

		/// <summary>
		/// Subscribe to the connection state manager events.
		/// </summary>
		/// <param name="connectionStateManager"></param>
		private void Subscribe(ConnectionStateManager connectionStateManager)
		{
			connectionStateManager.OnConnectedStateChanged += PortOnConnectionStatusChanged;
			connectionStateManager.OnIsOnlineStateChanged += PortOnIsOnlineStateChanged;
			connectionStateManager.OnSerialDataReceived += PortOnSerialDataReceived;
		}

		/// <summary>
		/// Unsubscribe from the connection state manager events.
		/// </summary>
		/// <param name="connectionStateManager"></param>
		private void Unsubscribe(ConnectionStateManager connectionStateManager)
		{
			connectionStateManager.OnConnectedStateChanged -= PortOnConnectionStatusChanged;
			connectionStateManager.OnIsOnlineStateChanged -= PortOnIsOnlineStateChanged;
			connectionStateManager.OnSerialDataReceived -= PortOnSerialDataReceived;
		}

		/// <summary>
		/// Called when the port online status changes.
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void PortOnIsOnlineStateChanged(object sender, BoolEventArgs e)
		{
			UpdateCachedOnlineStatus();
		}

		/// <summary>
		/// Called when the port connection status changes.
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void PortOnConnectionStatusChanged(object sender, BoolEventArgs e)
		{
			m_SerialBuffer.Clear();

			if (!e.Data)
				Initialized = false;
		}

		/// <summary>
		/// Called when the port receives serial data from the device.
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void PortOnSerialDataReceived(object sender, StringEventArgs e)
		{
			m_SerialBuffer.Enqueue(e.Data);
		}

		#endregion

		#region Settings

		/// <summary>
		/// Override to clear the instance settings.
		/// </summary>
		protected override void ClearSettingsFinal()
		{
			base.ClearSettingsFinal();

			Password = null;

			m_ComSpecProperties.ClearComSpecProperties();
			m_NetworkProperties.ClearNetworkProperties();

			SetPort(null);
		}

		/// <summary>
		/// Override to apply properties to the settings instance.
		/// </summary>
		/// <param name="settings"></param>
		protected override void CopySettingsFinal(TSettings settings)
		{
			base.CopySettingsFinal(settings);

			settings.Password = Password;
			settings.Port = m_ConnectionStateManager.PortNumber;

			settings.Copy(m_ComSpecProperties);
			settings.Copy(m_NetworkProperties);
		}

		/// <summary>
		/// Override to apply settings to the instance.
		/// </summary>
		/// <param name="settings"></param>
		/// <param name="factory"></param>
		protected override void ApplySettingsFinal(TSettings settings, IDeviceFactory factory)
		{
			base.ApplySettingsFinal(settings, factory);

			m_ComSpecProperties.Copy(settings);
			m_NetworkProperties.Copy(settings);

			Password = settings.Password;

			ISerialPort port = null;

			if (settings.Port != null)
			{
				try
				{
					port = factory.GetPortById((int)settings.Port) as ISerialPort;
				}
				catch (KeyNotFoundException)
				{
					Logger.Log(eSeverity.Error, "No serial port with id {0}", settings.Port);
				}
			}

			SetPort(port);
		}

		/// <summary>
		/// Override to add actions on StartSettings
		/// This should be used to start communications with devices and perform initial actions
		/// </summary>
		protected override void StartSettingsFinal()
		{
			base.StartSettingsFinal();

			m_ConnectionStateManager.Start();
		}

		#endregion

		#region Console
		/// <summary>
		/// Gets the child console nodes.
		/// </summary>
		/// <returns></returns>
		public override IEnumerable<IConsoleNodeBase> GetConsoleNodes()
		{
			foreach (IConsoleNodeBase node in GetBaseConsoleNodes())
				yield return node;

			if (m_ConnectionStateManager != null)
				yield return m_ConnectionStateManager.Port;
		}

		/// <summary>
		/// Workaround for "unverifiable code" warning.
		/// </summary>
		/// <returns></returns>
		private IEnumerable<IConsoleNodeBase> GetBaseConsoleNodes()
		{
			return base.GetConsoleNodes();
		}
		#endregion
	}
}
