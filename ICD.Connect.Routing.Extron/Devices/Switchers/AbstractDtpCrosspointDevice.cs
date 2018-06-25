﻿using System;
using System.Text.RegularExpressions;
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
using ICD.Connect.Protocol.Utils;
using ICD.Connect.Routing.Extron.SerialBuffers;
using ICD.Connect.Settings.Core;

namespace ICD.Connect.Routing.Extron.Devices.Switchers
{
	public abstract class AbstractDtpCrosspointDevice<TSettings> : AbstractDevice<TSettings>, IDtpCrosspointDevice
		where TSettings: AbstractDtpCrosspointSettings, new()
	{
        private const string PORT_INITIALIZED_REGEX = @"Lrpt(I|O)(\d{1,2})\*((?:0|1){1,2})";
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
        /// Raised when an input serial port is initialized.
        /// </summary>
		public event EventHandler<IntEventArgs> OnInputPortInitialized;

        /// <summary>
        /// Raised when an output serial port is initialized.
        /// </summary>
		public event EventHandler<IntEventArgs> OnOutputPortInitialized;

		/// <summary>
		/// Raised when the device sends a response.
		/// </summary>
		public event EventHandler<StringEventArgs> OnResponseReceived;

		private readonly DtpCrosspointSerialBuffer m_SerialBuffer;
		private readonly SafeTimer m_KeepAliveTimer;
		private readonly ConnectionStateManager m_ConnectionStateManager;

		private bool m_Initialized;

		private ushort m_StartingComPort = 2000;

		#region Properties

		public string Address { get; private set; }

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
			m_SerialBuffer = new DtpCrosspointSerialBuffer();
			Subscribe(m_SerialBuffer);

			m_ConnectionStateManager = new ConnectionStateManager(this) { ConfigurePort = ConfigurePort };
			m_ConnectionStateManager.OnConnectedStateChanged += PortOnConnectionStatusChanged;
			m_ConnectionStateManager.OnIsOnlineStateChanged += PortOnIsOnlineStateChanged;
			m_ConnectionStateManager.OnSerialDataReceived += PortOnSerialDataReceived;

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

		public HostInfo? GetInputComPortHostInfo(int input)
		{
			int? portOffset = GetPortOffsetFromInput(input);

			if (portOffset == null)
				return null;

			return new HostInfo(Address, (ushort)(m_StartingComPort + portOffset));
		}

		public HostInfo? GetOutputComPortHostInfo(int output)
		{
			int? portOffset = GetPortOffsetFromOutput(output);

			if (portOffset == null)
				return null;

			return new HostInfo(Address, (ushort)(m_StartingComPort + portOffset));
		}

		public void InitializeTxComPort(int input, eExtronPortInsertionMode mode, eComBaudRates baudRate, eComDataBits dataBits, eComParityType parityType,
			eComStopBits stopBits)
		{
			int? portOffset = GetPortOffsetFromInput(input);
			if (portOffset == null)
				return;

			SendCommand(string.Format("WI{0}*{1}LRPT", input, (ushort)mode));
			SetComPortSpec(portOffset.Value, baudRate, dataBits, parityType, stopBits);
		}

		public void InitializeRxComPort(int output, eExtronPortInsertionMode mode, eComBaudRates baudRate, eComDataBits dataBits, eComParityType parityType,
			eComStopBits stopBits)
		{
			int? portOffset = GetPortOffsetFromOutput(output);
			if (portOffset == null)
				return;
			SendCommand(string.Format("WO{0}*{1}LRPT", output, (ushort)mode));
			SetComPortSpec(portOffset.Value, baudRate, dataBits, parityType, stopBits);
		}

		private void SetComPortSpec(int portOffset, eComBaudRates baudRate, eComDataBits dataBits, eComParityType parityType,
			eComStopBits stopBits)
		{
			SendCommand(string.Format("W{0}*{1},{2},{3},{4}CP",
				portOffset + 1,
				ComSpecUtils.BaudRateToRate(baudRate),
				GetParityChar(parityType),
				(ushort)dataBits,
				(ushort)stopBits ));
		}

		private char GetParityChar(eComParityType parityType)
		{
			switch (parityType)
			{
				case eComParityType.ComspecParityEven:
					return 'e';
				case eComParityType.ComspecParityOdd:
					return 'o';
				case eComParityType.ComspecParityZeroStick:
					return 's';
				case eComParityType.ComspecParityNone:
				default:
					return 'n';
			}
		}

		/// <summary>
		/// Get the ComPort offset based on switcher input.
		/// Port offsets are 1 and 2 for the last two inputs respectively.
		/// </summary>
		/// <param name="input"></param>
		/// <returns></returns>
		private int? GetPortOffsetFromInput(int input)
		{
			var switcherControl = Controls.GetControl(0) as IDtpCrosspointSwitcherControl;
			if (switcherControl == null)
				return null;

			int portOffset = input - switcherControl.NumberOfInputs + 2;
			if (portOffset <= 0)
				return null;
			return portOffset;
		}

		private int? GetInputFromPortOffset(int offset)
		{
			var switcherControl = Controls.GetControl(0) as IDtpCrosspointSwitcherControl;
			if (switcherControl == null)
				return null;

			if (offset <= 0 || offset > 2)
				return null;

			return switcherControl.NumberOfInputs + offset - 2;
		}

		/// <summary>
		/// Get the ComPort offset based on switcher output.
		/// Port offsets are 3 and 4 for the last two outputs respectively.
		/// </summary>
		/// <param name="output"></param>
		/// <returns></returns>
		private int? GetPortOffsetFromOutput(int output)
		{
			var switcherControl = Controls.GetControl(0) as IDtpCrosspointSwitcherControl;
			if (switcherControl == null)
				return null;

			int portOffset = output - switcherControl.NumberOfOutputs + 4;
			if (portOffset <= 0)
				return null;
			return portOffset;
		}

		private int? GetOutputFromPortOffset(int offset)
		{
			var switcherControl = Controls.GetControl(0) as IDtpCrosspointSwitcherControl;
			if (switcherControl == null)
				return null;

			if (offset <= 2 || offset > 4)
				return null;

			return switcherControl.NumberOfOutputs + offset - 4;
		}

		private void Initialize()
		{
			SetVerboseMode(eExtronVerbosity.All);
			GetStartingComPort();
		}

		private void GetStartingComPort()
		{
			SendCommand("WMD");
		}

		private void KeepAliveCallback()
		{
			if (Initialized)
				SetVerboseMode(eExtronVerbosity.All);
		}

		private void SetVerboseMode(eExtronVerbosity all)
		{
			SendCommand("W{0}CV", (ushort)all);
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

			if (args.Data.StartsWith("Pmd"))
			{
				m_StartingComPort = ushort.Parse(args.Data.Substring(3));
			}

			if (args.Data.StartsWith("Vrb"))
			{
				var verbosity = (eExtronVerbosity)ushort.Parse(args.Data.Substring(3));
				if (verbosity == eExtronVerbosity.All)
					Initialized = true;
				else
				{
					Initialized = false;
					SetVerboseMode(eExtronVerbosity.All);
				}
			}

            Match match = Regex.Match(args.Data, PORT_INITIALIZED_REGEX);
            if (match.Success)
            {
                int address = int.Parse(match.Groups[2].Value);
                //eExtronPortInsertionMode mode = (eExtronPortInsertionMode)int.Parse(match.Groups[3].Value);
                if (match.Groups[1].Value == "I")
                    OnInputPortInitialized.Raise(this, new IntEventArgs(address));
                if (match.Groups[1].Value == "O")
                    OnOutputPortInitialized.Raise(this, new IntEventArgs(address));
            }
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

			OnConnectedStateChanged.Raise(this, new BoolEventArgs(e.Data));
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

			Password = settings.Password;
		    Address = settings.Address;

			ISerialPort port = null;

			if (settings.Port != null)
			{
				port = factory.GetPortById((int)settings.Port) as ISerialPort;
				if (port == null)
					Log(eSeverity.Error, "No serial port with id {0}", settings.Port);
			}

			m_ConnectionStateManager.SetPort(port);
		}

		protected override void CopySettingsFinal(TSettings settings)
		{
			base.CopySettingsFinal(settings);

			settings.Password = Password;
			settings.Port = m_ConnectionStateManager.PortNumber;
		}

		#endregion
	}

	/// <summary>
	/// For initializing the verbose mode of the switcher
	/// </summary>
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

	/// <summary>
	/// For initializing the DTP serial insertion
	/// </summary>
	public enum eExtronPortInsertionMode
	{
		/// <summary>
		/// RS-232 passthrough of the serial port
		/// </summary>
		CaptiveScrew = 0,
		
		/// <summary>
		/// TCP port insertion of serial data
		/// </summary>
		Ethernet
	}
}