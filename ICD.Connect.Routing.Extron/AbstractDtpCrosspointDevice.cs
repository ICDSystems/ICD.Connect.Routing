using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Crestron.SimplSharp;
using ICD.Common.Properties;
using ICD.Common.Utils.EventArguments;
using ICD.Common.Utils.Extensions;
using ICD.Common.Utils.Services.Logging;
using ICD.Common.Utils.Timers;
using ICD.Connect.Devices;
using ICD.Connect.Protocol;
using ICD.Connect.Protocol.Extensions;
using ICD.Connect.Protocol.Ports;
using ICD.Connect.Protocol.Ports.ComPort;
using ICD.Connect.Settings.Core;

namespace ICD.Connect.Routing.Extron
{
	public abstract class AbstractDtpCrosspointDevice<TSettings> : AbstractDevice<TSettings>
		where TSettings: AbstractDtpCrosspointSettings, new()
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

		private readonly DtpCrosspointSerialBuffer m_SerialBuffer;
		private readonly SafeTimer m_KeepAliveTimer;
		private readonly ConnectionStateManager m_ConnectionStateManager;

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

		public AbstractDtpCrosspointDevice()
		{
			m_ConnectionStateManager = new ConnectionStateManager(this) { ConfigurePort = ConfigurePort };
			m_ConnectionStateManager.OnConnectedStateChanged += PortOnConnectionStatusChanged;
			m_ConnectionStateManager.OnIsOnlineStateChanged += PortOnIsOnlineStateChanged;
			m_ConnectionStateManager.OnSerialDataReceived += PortOnSerialDataReceived;

			m_SerialBuffer = new DtpCrosspointSerialBuffer();
			Subscribe(m_SerialBuffer);

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
		/// Sets the port for communicating with the device.
		/// </summary>
		/// <param name="port"></param>
		[PublicAPI]
		public void ConfigurePort(ISerialPort port)
		{
			if (port is IComPort)
				ConfigureComPort(port as IComPort);
		}

		[PublicAPI]
		public static void ConfigureComPort(IComPort port)
		{
			port.SetComPortSpec(eComBaudRates.ComspecBaudRate9600,
								eComDataBits.ComspecDataBits8,
								eComParityType.ComspecParityNone,
								eComStopBits.ComspecStopBits1,
								eComProtocolType.ComspecProtocolRS232,
								eComHardwareHandshakeType.ComspecHardwareHandshakeNone,
								eComSoftwareHandshakeType.ComspecSoftwareHandshakeNone,
								false);
		}

		protected override bool GetIsOnlineStatus()
		{
			return m_ConnectionStateManager != null && m_ConnectionStateManager.IsOnline;
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

			if (!IsConnected)
			{
				Log(eSeverity.Critical, "Unable to connect");
				return;
			}

			m_ConnectionStateManager.Send(command + '\r');
		}

		private void Initialize()
		{
			SetVerboseMode(eExtronVerbosity.All);
			Initialized = true;
		}

		private void KeepAliveCallback()
		{
			if (Initialized)
				SetVerboseMode(eExtronVerbosity.All);
		}

		private void SetVerboseMode(eExtronVerbosity all)
		{
			SendCommand("{0}CV", (ushort)all);
		}

		#endregion

		#region Buffer Callbacks

		private void Subscribe(DtpCrosspointSerialBuffer buffer)
		{
			buffer.OnPasswordPrompt += BufferOnOnPasswordPrompt;
			buffer.OnEmptyPrompt += BufferOnOnEmptyPrompt;
			buffer.OnCompletedSerial += BufferOnOnCompletedSerial;
		}

		private void Unsubscribe(DtpCrosspointSerialBuffer buffer)
		{
			buffer.OnPasswordPrompt -= BufferOnOnPasswordPrompt;
			buffer.OnEmptyPrompt -= BufferOnOnEmptyPrompt;
			buffer.OnCompletedSerial -= BufferOnOnCompletedSerial;
		}

		private void BufferOnOnPasswordPrompt(object sender, EventArgs eventArgs)
		{
			SendCommand(Password);
		}

		private void BufferOnOnEmptyPrompt(object sender, EventArgs eventArgs)
		{
			Initialize();
		}

		private void BufferOnOnCompletedSerial(object sender, StringEventArgs args)
		{
			OnResponseReceived.Raise(this, new StringEventArgs(args.Data));
		}

		#endregion

		#region Port Callbacks

		private void PortOnIsOnlineStateChanged(object sender, BoolEventArgs e)
		{
			UpdateCachedOnlineStatus();
		}

		private void PortOnConnectionStatusChanged(object sender, BoolEventArgs e)
		{
			m_SerialBuffer.Clear();

			OnConnectedStateChanged(this, new BoolEventArgs(e.Data));
		}

		private void PortOnSerialDataReceived(object sender, StringEventArgs e)
		{
			m_SerialBuffer.Enqueue(e.Data);
		}

		#endregion

		#region Settings

		protected override void ApplySettingsFinal(TSettings settings, IDeviceFactory factory)
		{
			base.ApplySettingsFinal(settings, factory);

			ISerialPort port = null;

			if (settings.Port != null)
			{
				port = factory.GetPortById((int)settings.Port) as ISerialPort;
				if (port == null)
					Log(eSeverity.Error, "No serial port with id {0}", settings.Port);
			}
		}

		#endregion
	}

	[Flags]
	internal enum eExtronVerbosity
	{
		/// <summary>
		/// Default for Telnet connection
		/// </summary>
		None = 0,

		/// <summary>
		/// Default for RS-232 or USB connection
		/// </summary>
		VerboseMode = 1,

		/// <summary>
		/// If tagged responses is enabled, all read commands return the constant string and the value as the set command does.
		/// </summary>
		TaggedResponses = 2,

		/// <summary>
		/// Verbose mode and tagged responses for queries
		/// </summary>
		All = VerboseMode | TaggedResponses
	}
}