using System;
using ICD.Common.Services;
using ICD.Common.Utils.EventArguments;
using ICD.Connect.Protocol.Network.Tcp;
using ICD.Connect.Protocol.Ports;
using ICD.Connect.Protocol.Ports.ComPort;
using ICD.Connect.Routing.Crestron2Series.ControlSystem;
using ICD.Connect.Settings.Core;

namespace ICD.Connect.Routing.Crestron2Series.Ports.ComPort
{
	public sealed class Dmps300CComPort : AbstractComPort<Dmps300CComPortSettings>
	{
		private readonly AsyncTcpClient m_Client;

		private Dmps300CControlSystem m_Device;

		#region Properties

		/// <summary>
		/// Gets the port address on the DMPS.
		/// </summary>
		public int Address { get; set; }

		#endregion

		/// <summary>
		/// Constructor.
		/// </summary>
		public Dmps300CComPort()
		{
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
				return;

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

		public override void SetComPortSpec(eComBaudRates baudRate, eComDataBits numberOfDataBits, eComParityType parityType,
		                                    eComStopBits numberOfStopBits, eComProtocolType protocolType,
		                                    eComHardwareHandshakeType hardwareHandShake, eComSoftwareHandshakeType softwareHandshake,
		                                    bool reportCtsChanges)
		{
			throw new NotImplementedException();
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

			m_Device = factory.GetOriginatorById<Dmps300CControlSystem>(settings.Device);
			Address = settings.Address;
		}

		#endregion
	}
}