using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using ICD.Common.Utils;
using ICD.Common.Utils.EventArguments;
using ICD.Common.Utils.IO;
using ICD.Common.Utils.Services;
using ICD.Common.Utils.Services.Logging;
using ICD.Connect.Devices.Controls;
using ICD.Connect.Protocol.Network.Ports.Tcp;
using ICD.Connect.Protocol.Ports;
using ICD.Connect.Protocol.Ports.ComPort;
using ICD.Connect.Protocol.Utils;
using ICD.Connect.Routing.Extron.Devices.Endpoints;
using ICD.Connect.Settings;
using ICD.Connect.Settings.Cores;

namespace ICD.Connect.Routing.Extron.Devices.Switchers.DtpCrosspoint
{
	public abstract class AbstractDtpCrosspointDevice<TSettings> : AbstractExtronSwitcherDevice<TSettings>, IDtpCrosspointDevice
		where TSettings : IDtpCrosspointSettings, new()
	{
		private const string PORT_INITIALIZED_REGEX = @"Lrpt(I|O)(\d{1,2})\*((?:0|1){1,2})";

		private const string PORT_COMSPEC_FEEDBACK_REGEX = @"Cpn(?'number'\d+) Ccp(?'baud'\d+),(?'parity'\w),(?'databits'\d),(?'stopbits'\d)";

		/// <summary>
		/// Raised when a serial port is initialized.
		/// </summary>
		public event PortInitializedCallback OnPortInitialized;

		/// <summary>
		/// Raised when a serial port comspec changes.
		/// </summary>
		public event PortComSpecCallback OnPortComSpecChanged;

		/// <summary>
		/// Maps input address to port originator id.
		/// </summary>
		private readonly Dictionary<int, int> m_DtpInputPorts;

		/// <summary>
		/// Maps output address to port originator id.
		/// </summary>
		private readonly Dictionary<int, int> m_DtpOutputPorts;

		private readonly SafeCriticalSection m_DtpPortsSection;

		private readonly List<IDeviceControl> m_LoadedControls;

		private string m_ConfigPath;
		private ushort m_StartingComPort = 2000;

		#region Properties

		public string Address { get; private set; }

		public abstract int NumberOfDtpInputPorts { get; }

		public abstract int NumberOfDtpOutputPorts { get; }

		#endregion

		/// <summary>
		/// Constructor.
		/// </summary>
		protected AbstractDtpCrosspointDevice()
		{
			m_DtpInputPorts = new Dictionary<int, int>();
			m_DtpOutputPorts = new Dictionary<int, int>();
			m_DtpPortsSection = new SafeCriticalSection();

			m_LoadedControls = new List<IDeviceControl>();
		}

		/// <summary>
		/// Release resources.
		/// </summary>
		protected override void DisposeFinal(bool disposing)
		{
			OnPortInitialized = null;
			OnPortComSpecChanged = null;

			DisposeLoadedControls();

			base.DisposeFinal(disposing);
		}

		#region Methods

		public ISerialPort GetSerialInsertionPort(int address, eDtpInputOuput inputOutput)
		{
			m_DtpPortsSection.Enter();
			try
			{
				switch (inputOutput)
				{
					case eDtpInputOuput.Input:
						if (m_DtpInputPorts.ContainsKey(address))
							return ServiceProvider
								.GetService<ICore>()
								.Originators
								.GetChild<ISerialPort>(m_DtpInputPorts[address]);
						break;

					case eDtpInputOuput.Output:
						if (m_DtpOutputPorts.ContainsKey(address))
							return ServiceProvider
								.GetService<ICore>()
								.Originators
								.GetChild<ISerialPort>(m_DtpOutputPorts[address]);
						break;

					default:
						throw new ArgumentOutOfRangeException("inputOutput");
				}
			}
			catch (Exception ex)
			{
				Log(eSeverity.Error, "Could not get insertion port - {0}", ex.Message);
			}
			finally
			{
				m_DtpPortsSection.Leave();
			}

			int? portOffset = DtpUtils.GetPortOffsetFromSwitcher(this, address, inputOutput);
			return GetTcpClientForPortOffset(portOffset);
		}

		public void SetComPortSpec(int address, eDtpInputOuput inputOutput, eComBaudRates baudRate, eComDataBits dataBits,
		                           eComParityType parityType, eComStopBits stopBits)
		{
			int? portOffset = DtpUtils.GetPortOffsetFromSwitcher(this, address, inputOutput);
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
						DtpUtils.GetParityChar(parityType),
			            (ushort)dataBits,
			            (ushort)stopBits);
		}

		private ISerialPort GetTcpClientForPortOffset(int? portOffset)
		{
			if (portOffset == null)
				return null;

			return new AsyncTcpClient
			{
				Address = Address,
				Port = (ushort)(m_StartingComPort + portOffset)
			};
		}

		protected override void Initialize()
		{
			base.Initialize();

			GetStartingComPort();

			// Enable ethernet passthrough on the input ComPorts
			var inputs = DtpUtils.GetNumberOfSwitcherInputs(this);
			var startingDtpInput = inputs - NumberOfDtpInputPorts + 1;
			m_DtpPortsSection.Execute(() =>
			{
				foreach (var input in Enumerable.Range(startingDtpInput, NumberOfDtpInputPorts))
				{
					var comPortMode = m_DtpInputPorts.ContainsKey(input);
					SendCommand("WI{0}*{1}LRPT", input, comPortMode ? "0" : "1");
				}
			});

			// Enable ethernet passthrough on the output ComPorts
			var outputs = DtpUtils.GetNumberOfSwitcherOutputs(this);
			var startingDtpOutput = outputs - NumberOfDtpOutputPorts + 1;
			m_DtpPortsSection.Execute(() =>
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

		#endregion

		#region Buffer Callbacks

		/// <summary>
		/// Called when we receive a complete response from the device.
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="args"></param>
		protected override void BufferOnCompletedSerial(object sender, StringEventArgs args)
		{
			base.BufferOnCompletedSerial(sender, args);

			if (args.Data.StartsWith("Pmd"))
				m_StartingComPort = ushort.Parse(args.Data.Substring(3));

			Match match;

			if (RegexUtils.Matches(args.Data, PORT_INITIALIZED_REGEX, out match))
			{
				int address = int.Parse(match.Groups[2].Value);
				PortInitializedCallback handler = OnPortInitialized;

				if (handler != null)
				{
					if (match.Groups[1].Value == "I")
						handler(this, address, eDtpInputOuput.Input);
					if (match.Groups[1].Value == "O")
						handler(this, address, eDtpInputOuput.Output);
				}
			}
			else if (RegexUtils.Matches(args.Data, PORT_COMSPEC_FEEDBACK_REGEX, out match))
			{
				int number = int.Parse(match.Groups["number"].Value);

				eDtpInputOuput inputOutput;
				int? address = DtpUtils.GetSwitcherAddressFromPortOffset(this, number, out inputOutput);
				if (address == null)
					return;
				
				int baud = int.Parse(match.Groups["baud"].Value);
				char parity = char.ToLower(match.Groups["parity"].Value[0]);
				int dataBits = int.Parse(match.Groups["databits"].Value);
				int stopBits = int.Parse(match.Groups["stopbits"].Value);

				ComSpec spec = new ComSpec
				{
					BaudRate = ComSpecUtils.BaudRateFromRate(baud),
					ParityType = DtpUtils.FromParityChar(parity),
					NumberOfDataBits = ComSpecUtils.DataBitsFromCount(dataBits),
					NumberOfStopBits = ComSpecUtils.StopBitsFromCount(stopBits)
				};

				var handler = OnPortComSpecChanged;
				if (handler != null)
					handler(this, address.Value, inputOutput, spec);
			}
		}

		#endregion

		#region Settings

		protected override void ClearSettingsFinal()
		{
			base.ClearSettingsFinal();

			Address = null;
			m_ConfigPath = null;

			m_DtpPortsSection.Execute(() => m_DtpInputPorts.Clear());
			m_DtpPortsSection.Execute(() => m_DtpOutputPorts.Clear());
		}

		protected override void CopySettingsFinal(TSettings settings)
		{
			base.CopySettingsFinal(settings);

			settings.Address = Address;
			settings.Config = m_ConfigPath;

			m_DtpPortsSection.Execute(() =>
			                          settings.DtpInputPorts = m_DtpInputPorts
				                                                   .Select(pair => new KeyValuePair<int, int>(pair.Key, pair.Value)));
			m_DtpPortsSection.Execute(() =>
			                          settings.DtpOutputPorts = m_DtpOutputPorts
				                                                    .Select(pair => new KeyValuePair<int, int>(pair.Key, pair.Value)));
		}

		protected override void ApplySettingsFinal(TSettings settings, IDeviceFactory factory)
		{
			base.ApplySettingsFinal(settings, factory);

			Address = settings.Address;

			if (!string.IsNullOrEmpty(settings.Config))
				LoadControls(settings.Config);

			m_DtpPortsSection.Enter();
			try
			{
				m_DtpInputPorts.Clear();
				foreach (var pair in settings.DtpInputPorts)
				{
					if (DtpUtils.GetPortOffsetFromSwitcher(this, pair.Key, eDtpInputOuput.Input) == null)
					{
						Log(eSeverity.Error, "{0} is not a valid DTP Input address", pair.Key);
						continue;
					}

					m_DtpInputPorts.Add(pair.Key, pair.Value);
				}
			}
			finally
			{
				m_DtpPortsSection.Leave();
			}

			m_DtpPortsSection.Enter();
			try
			{
				m_DtpOutputPorts.Clear();
				foreach (var pair in settings.DtpOutputPorts)
				{
					if (DtpUtils.GetPortOffsetFromSwitcher(this, pair.Key, eDtpInputOuput.Output) == null)
					{
						Log(eSeverity.Error, "{0} is not a valid DTP Output address", pair.Key);
						continue;
					}

					m_DtpOutputPorts.Add(pair.Key, pair.Value);
				}
			}
			finally
			{
				m_DtpPortsSection.Leave();
			}
		}

		#endregion
	}
}