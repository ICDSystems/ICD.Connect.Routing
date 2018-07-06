using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using ICD.Common.Properties;
using ICD.Common.Utils;
using ICD.Common.Utils.EventArguments;
using ICD.Common.Utils.Extensions;
using ICD.Common.Utils.IO;
using ICD.Common.Utils.Services;
using ICD.Common.Utils.Services.Logging;
using ICD.Common.Utils.Timers;
using ICD.Connect.Devices;
using ICD.Connect.Devices.Controls;
using ICD.Connect.Protocol;
using ICD.Connect.Protocol.Extensions;
using ICD.Connect.Protocol.Network.Tcp;
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
		private string m_ConfigPath;
		private ushort m_StartingComPort = 2000;
		private readonly Dictionary<int, int> m_DtpInputPorts;
		private readonly SafeCriticalSection m_DtpInputPortsSection;
		private readonly Dictionary<int, int> m_DtpOutputPorts;
		private readonly SafeCriticalSection m_DtpOutputPortsSection;

		private readonly List<IDeviceControl> m_LoadedControls;

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

		protected abstract int NumberOfDtpInputPorts { get; }

		protected abstract int NumberOfDtpOutputPorts { get; }

		#endregion

		public AbstractDtpCrosspointDevice()
		{
			m_DtpInputPorts = new Dictionary<int, int>();
			m_DtpOutputPorts = new Dictionary<int, int>();
			m_DtpInputPortsSection = new SafeCriticalSection();
			m_DtpOutputPortsSection = new SafeCriticalSection();

			m_SerialBuffer = new DtpCrosspointSerialBuffer();
			Subscribe(m_SerialBuffer);

			m_ConnectionStateManager = new ConnectionStateManager(this) { ConfigurePort = ConfigurePort };
			Subscribe(m_ConnectionStateManager);

			m_KeepAliveTimer = SafeTimer.Stopped(KeepAliveCallback);
			m_KeepAliveTimer.Reset(KEEPALIVE_INTERVAL, KEEPALIVE_INTERVAL);

			m_LoadedControls = new List<IDeviceControl>();
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

			DisposeLoadedControls();

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
			
			if (!IsConnected)
			{
				Log(eSeverity.Critical, "Unable to connect");
				return;
			}

			m_ConnectionStateManager.Send(command + '\r');
		}

		/// <summary>
		/// Sets the port for communicating with the device.
		/// </summary>
		/// <param name="port"></param>
		[PublicAPI]
		public void ConfigurePort(ISerialPort port)
		{
			var comPort = port as IComPort;
			if (comPort != null)
				ConfigureComPort(comPort);
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

		public ISerialPort GetInputSerialInsertionPort(int input)
		{
			m_DtpInputPortsSection.Enter();
			try
			{
				if (m_DtpInputPorts.ContainsKey(input))
					return ServiceProvider
						.GetService<ICore>()
						.Originators
						.GetChild<IComPort>(m_DtpInputPorts[input]);
			}
			catch (Exception ex)
			{
				Log(eSeverity.Error, "Could not get input insertion port - {0}", ex.Message);
			}
			finally
			{
				m_DtpInputPortsSection.Leave();
			}

			int? portOffset = GetPortOffsetFromInput(input);
			return GetTcpClientForPortOffset(portOffset);
		}

		public ISerialPort GetOutputSerialInsertionPort(int output)
		{
			m_DtpOutputPortsSection.Enter();
			try
			{
				if (m_DtpOutputPorts.ContainsKey(output))
					return ServiceProvider
						.GetService<ICore>()
						.Originators
						.GetChild<IComPort>(m_DtpOutputPorts[output]);
			}
			catch (Exception ex)
			{
				Log(eSeverity.Error, "Could not get output insertion port - {0}", ex.Message);
			}
			finally
			{
				m_DtpOutputPortsSection.Leave();
			}
			int? portOffset = GetPortOffsetFromOutput(output);
			return GetTcpClientForPortOffset(portOffset);
		}

		public void SetTxComPortSpec(int input, eComBaudRates baudRate, eComDataBits dataBits, eComParityType parityType,
		                             eComStopBits stopBits)
		{
			int? portOffset = GetPortOffsetFromInput(input);
			if (portOffset == null)
				return;

			SetComPortSpec(portOffset.Value, baudRate, dataBits, parityType, stopBits);
		}

		public void SetRxComPortSpec(int output, eComBaudRates baudRate, eComDataBits dataBits, eComParityType parityType,
		                             eComStopBits stopBits)
		{
			int? portOffset = GetPortOffsetFromOutput(output);
			if (portOffset == null)
				return;
			SetComPortSpec(portOffset.Value, baudRate, dataBits, parityType, stopBits);
		}

		public void LoadControls(string path)
		{
			m_ConfigPath = path;

			string fullPath = PathUtils.GetDefaultConfigPath("DtpCrosspoint", path);

			try
			{
				string xml = IcdFile.ReadToEnd(fullPath, new UTF8Encoding(false));
				xml = EncodingUtils.StripUtf8Bom(xml);

				ParseXml(xml);
			}
			catch (Exception e)
			{
				Log(eSeverity.Error, "Failed to load integration config {0} - {1}", fullPath, e.Message);
			}
		}

		private void ParseXml(string xml)
		{
			DisposeLoadedControls();

			// Load and add the new controls
			foreach (IDeviceControl control in ExtronXmlUtils.GetControlsFromXml(xml, this))
			{
				Controls.Add(control);
				m_LoadedControls.Add(control);
			}
		}

		private void DisposeLoadedControls()
		{
			foreach (var control in m_LoadedControls)
				control.Dispose();

			m_LoadedControls.Clear();
		}

		#endregion

		#region Private Methods

		private void SetComPortSpec(int portOffset, eComBaudRates baudRate, eComDataBits dataBits, eComParityType parityType,
			eComStopBits stopBits)
		{
			SendCommand("W{0}*{1},{2},{3},{4}CP",
				portOffset + 1,
				ComSpecUtils.BaudRateToRate(baudRate),
				GetParityChar(parityType),
				(ushort)dataBits,
				(ushort)stopBits);
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
			int portOffset = input - GetNumberOfInputs() + NumberOfDtpInputPorts;
			if (portOffset <= 0)
				return null;
			return portOffset;
		}

		/// <summary>
		/// Get the ComPort offset based on switcher output.
		/// Port offsets are 3 and 4 for the last two outputs respectively.
		/// </summary>
		/// <param name="output"></param>
		/// <returns></returns>
		private int? GetPortOffsetFromOutput(int output)
		{
			

			int portOffset = output - GetNumberOfOutputs() + (NumberOfDtpInputPorts + NumberOfDtpOutputPorts);
			if (portOffset <= NumberOfDtpInputPorts)
				return null;
			return portOffset;
		}

		private int GetNumberOfInputs()
		{
			var switcherControl = Controls.GetControl<IDtpCrosspointSwitcherControl>();
			return switcherControl.NumberOfInputs;
		}

		private int GetNumberOfOutputs()
		{
			var switcherControl = Controls.GetControl<IDtpCrosspointSwitcherControl>();
			return switcherControl.NumberOfOutputs;
		}

		private ISerialPort GetTcpClientForPortOffset(int? portOffset)
		{
			if (portOffset == null)
				return null;

			return new AsyncTcpClient
			{
				Address = Address,
				Port = (ushort) (m_StartingComPort + portOffset)
			};
		}

		private void Initialize()
		{
			SetVerboseMode(eExtronVerbosity.All);
			GetStartingComPort();

			var inputs = GetNumberOfInputs();
			var startingDtpInput = inputs - NumberOfDtpInputPorts + 1;
			m_DtpInputPortsSection.Execute(() =>
			{
				foreach (var input in Enumerable.Range(startingDtpInput, NumberOfDtpInputPorts))
				{
					var comPortMode = m_DtpInputPorts.ContainsKey(input);
					SendCommand("WI{0}*{1}LRPT", input, comPortMode ? "0" : "1");
				}
			});

			var outputs = GetNumberOfOutputs();
			var startingDtpOutput = outputs - NumberOfDtpOutputPorts + 1;
			m_DtpOutputPortsSection.Execute(() =>
			{
				foreach (var output in Enumerable.Range(startingDtpOutput, NumberOfDtpOutputPorts))
				{
					var comPortMode = m_DtpOutputPorts.ContainsKey(output);
					SendCommand("WO{0}*{1}LRPT", output, comPortMode ? "0" : "1");
				}
			});
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

		private void SetVerboseMode(eExtronVerbosity verbosity)
		{
			SendCommand("W{0}CV", (ushort)verbosity);
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
			if (!Initialized)
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
                if (match.Groups[1].Value == "I")
                    OnInputPortInitialized.Raise(this, new IntEventArgs(address));
                if (match.Groups[1].Value == "O")
                    OnOutputPortInitialized.Raise(this, new IntEventArgs(address));
            }
        }

		#endregion

		#region Port Callbacks

		private void Subscribe(ConnectionStateManager connectionStateManager)
		{
			connectionStateManager.OnConnectedStateChanged += PortOnConnectionStatusChanged;
			connectionStateManager.OnIsOnlineStateChanged += PortOnIsOnlineStateChanged;
			connectionStateManager.OnSerialDataReceived += PortOnSerialDataReceived;;
		}

		private void Unsubscribe(ConnectionStateManager connectionStateManager)
		{
			connectionStateManager.OnConnectedStateChanged -= PortOnConnectionStatusChanged;
			connectionStateManager.OnIsOnlineStateChanged -= PortOnIsOnlineStateChanged;
			connectionStateManager.OnSerialDataReceived -= PortOnSerialDataReceived;
		}

		private void PortOnIsOnlineStateChanged(object sender, BoolEventArgs e)
		{
			UpdateCachedOnlineStatus();
		}

		private void PortOnConnectionStatusChanged(object sender, BoolEventArgs e)
		{
			m_SerialBuffer.Clear();
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

			if (!string.IsNullOrEmpty(settings.Config))
				LoadControls(settings.Config);

			m_DtpInputPortsSection.Enter();
			try 
			{
				m_DtpInputPorts.Clear();
				foreach (var pair in settings.DtpInputPorts)
				{
					if (GetPortOffsetFromInput(pair.Key) == null)
					{
						Log(eSeverity.Error, "{0} is not a valid DTP Input address", pair.Key);
						continue;
					}

					m_DtpInputPorts.Add(pair.Key, pair.Value);
				}
			}
			finally
			{
				m_DtpInputPortsSection.Leave();
			}

			m_DtpOutputPortsSection.Enter();
			try
			{
				m_DtpOutputPorts.Clear();
				foreach (var pair in settings.DtpOutputPorts)
				{
					if (GetPortOffsetFromOutput(pair.Key) == null)
					{
						Log(eSeverity.Error, "{0} is not a valid DTP Output address", pair.Key);
						continue;
					}
					
					m_DtpOutputPorts.Add(pair.Key, pair.Value);
				}
			}
			finally
			{
				m_DtpOutputPortsSection.Leave();
			}
		}

		protected override void CopySettingsFinal(TSettings settings)
		{
			base.CopySettingsFinal(settings);

			settings.Password = Password;
			settings.Address = Address;
			settings.Port = m_ConnectionStateManager.PortNumber;

			m_DtpInputPortsSection.Execute(() =>
				settings.DtpInputPorts = m_DtpInputPorts
					.Select(pair => new KeyValuePair<int, int>(pair.Key, pair.Value)));
			m_DtpOutputPortsSection.Execute(() =>
				settings.DtpOutputPorts = m_DtpOutputPorts
					.Select(pair => new KeyValuePair<int, int>(pair.Key, pair.Value)));
		}

		protected override void ClearSettingsFinal()
		{
			base.ClearSettingsFinal();

			Address = null;
			Password = null;
			m_ConnectionStateManager.SetPort(null);
			m_DtpInputPortsSection.Execute(() => m_DtpInputPorts.Clear());
			m_DtpOutputPortsSection.Execute(() => m_DtpOutputPorts.Clear());
		}

		#endregion
	}
}