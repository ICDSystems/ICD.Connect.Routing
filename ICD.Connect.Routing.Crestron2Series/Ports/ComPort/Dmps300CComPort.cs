using ICD.Common.Utils.EventArguments;
using ICD.Common.Utils.Services.Logging;
using ICD.Connect.Protocol.Network.Ports.Tcp;
using ICD.Connect.Protocol.Ports;
using ICD.Connect.Protocol.Ports.ComPort;
using ICD.Connect.Protocol.Settings;
using ICD.Connect.Protocol.Utils;
using ICD.Connect.Protocol.XSig;
using ICD.Connect.Routing.Crestron2Series.Devices;
using ICD.Connect.Settings;

namespace ICD.Connect.Routing.Crestron2Series.Ports.ComPort
{
	public sealed class Dmps300CComPort : AbstractComPort<Dmps300CComPortSettings>
	{
		private static readonly ComSpec s_DefaultComSpec = new ComSpec
		{
			BaudRate = eComBaudRates.BaudRate9600,
			NumberOfDataBits = eComDataBits.DataBits8,
			ParityType = eComParityType.None,
			NumberOfStopBits = eComStopBits.StopBits1,
			ProtocolType = eComProtocolType.Rs232,
			HardwareHandshake = eComHardwareHandshakeType.None,
			SoftwareHandshake = eComSoftwareHandshakeType.None,
			ReportCtsChanges = false,
		};

		private readonly IComSpecProperties m_ComSpecProperties;
		private readonly AsyncTcpClient m_Client;

		private readonly ComSpec m_ComSpec;

		private IDmps300CComPortDevice m_Device;

		#region Properties

		/// <summary>
		/// Gets the port address on the DMPS.
		/// </summary>
		public int Address { get; set; }

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
		public Dmps300CComPort()
		{
			m_ComSpec = s_DefaultComSpec.Copy();

			m_ComSpecProperties = new ComSpecProperties();
			m_Client = new AsyncTcpClient();

			Subscribe(m_Client);
		}

		/// <summary>
		/// Release resources.
		/// </summary>
		protected override void DisposeFinal(bool disposing)
		{
			base.DisposeFinal(disposing);

			Unsubscribe(m_Client);
			m_Client.Dispose();
		}

		#region Methods

		/// <summary>
		/// Sets IsConnected to true.
		/// </summary>
		public override void Connect()
		{
			if (m_Device == null)
			{
				Log(eSeverity.Error, "Unable to connect - parent device is null");
				return;
			}

			ushort port = (ushort)(m_Device.Port + Address);
			HostInfo info = new HostInfo(m_Device.Address, port);

			m_Client.Connect(info);
		}

		/// <summary>
		/// Sets IsConnected to false.
		/// </summary>
		public override void Disconnect()
		{
			m_Client.Disconnect();
		}

		/// <summary>
		/// Implements the actual sending logic. Wrapped by Send to handle connection status.
		/// </summary>
		protected override bool SendFinal(string data)
		{
			PrintTx(data);
			return m_Client.Send(data);
		}

		/// <summary>
		/// Configures the ComPort for communication.
		/// </summary>
		/// <param name="comSpec"></param>
		public override void SetComPortSpec(ComSpec comSpec)
		{
			if (m_Device == null)
			{
				Log(eSeverity.Error, "Unable to set comspec - parent device is null");
				return;
			}

			string spec = ComSpecUtils.AssembleComSpec(Address, comSpec);

			if (m_Device.SendData(new SerialXSig(spec, m_Device.ComSpecJoin)))
				m_ComSpec.Copy(comSpec);
		}

		#endregion

		#region Client Callbacks

		private void Subscribe(AsyncTcpClient client)
		{
			client.OnConnectedStateChanged += ClientOnConnectedStateChanged;
			client.OnSerialDataReceived += ClientOnSerialDataReceived;
		}

		private void Unsubscribe(AsyncTcpClient client)
		{
			client.OnConnectedStateChanged -= ClientOnConnectedStateChanged;
			client.OnSerialDataReceived -= ClientOnSerialDataReceived;
		}

		private void ClientOnSerialDataReceived(object sender, StringEventArgs stringEventArgs)
		{
			PrintRx(stringEventArgs.Data);
			Receive(stringEventArgs.Data);
		}

		private void ClientOnConnectedStateChanged(object sender, BoolEventArgs boolEventArgs)
		{
			IsConnected = boolEventArgs.Data;
		}

		#endregion

		#region Settings

		/// <summary>
		/// Override to clear the instance settings.
		/// </summary>
		protected override void ClearSettingsFinal()
		{
			base.ClearSettingsFinal();

			m_ComSpec.Copy(s_DefaultComSpec);

			m_Device = null;
			Address = 0;
		}

		/// <summary>
		/// Override to apply properties to the settings instance.
		/// </summary>
		/// <param name="settings"></param>
		protected override void CopySettingsFinal(Dmps300CComPortSettings settings)
		{
			base.CopySettingsFinal(settings);

			settings.Device = m_Device == null ? 0 : m_Device.Id;
			settings.Address = Address;
		}

		/// <summary>
		/// Override to apply settings to the instance.
		/// </summary>
		/// <param name="settings"></param>
		/// <param name="factory"></param>
		protected override void ApplySettingsFinal(Dmps300CComPortSettings settings, IDeviceFactory factory)
		{
			base.ApplySettingsFinal(settings, factory);

			m_Device = factory.GetOriginatorById<IDmps300CComPortDevice>(settings.Device);
			Address = settings.Address;

			ApplyConfiguration();
		}

		#endregion
	}
}