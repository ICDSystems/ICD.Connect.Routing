using System;
using System.Collections.Generic;
using ICD.Common.Utils;
using ICD.Common.Utils.EventArguments;
using ICD.Common.Utils.Extensions;
using ICD.Connect.API.Nodes;
using ICD.Connect.Devices;
using ICD.Connect.Protocol.EventArguments;
using ICD.Connect.Protocol.Heartbeat;
using ICD.Connect.Protocol.Network.Tcp;
using ICD.Connect.Protocol.Ports;
using ICD.Connect.Protocol.SerialBuffers;
using ICD.Connect.Protocol.XSig;

namespace ICD.Connect.Routing.Crestron2Series.Devices
{
	public abstract class AbstractDmps300CDevice<TSettings> : AbstractDevice<TSettings>, IDmps300CDevice, IConnectable
		where TSettings : IDmps300CDeviceSettings, new()
	{
		public event EventHandler<XSigEventArgs> OnSigEvent;
        public event EventHandler<BoolEventArgs> OnConnectedStateChanged;

	    public bool IsConnected
	    {
	        get { return GetIsOnlineStatus(); }
	    }

	    public Heartbeat Heartbeat { get; private set; }

		private readonly AsyncTcpClient m_Client;
		private readonly XSigSerialBuffer m_Buffer;

		#region Properties

		/// <summary>
		/// Gets the network address of the device.
		/// </summary>
		public string Address { get { return m_Client.Address; } set { m_Client.Address = value; } }

		/// <summary>
		/// Gets the network port of the device.
		/// </summary>
		public ushort Port { get { return m_Client.Port; } set { m_Client.Port = value; } }

		#endregion

		/// <summary>
		/// Constructor.
		/// </summary>
		protected AbstractDmps300CDevice()
		{
            Heartbeat = new Heartbeat(this);
			m_Client = new AsyncTcpClient();
			m_Buffer = new XSigSerialBuffer();

			Subscribe(m_Buffer);
			Subscribe(m_Client);

            Heartbeat.StartMonitoring();
		}

		/// <summary>
		/// Release resources.
		/// </summary>
		protected override void DisposeFinal(bool disposing)
		{
			OnSigEvent = null;

			base.DisposeFinal(disposing);

			Unsubscribe(m_Buffer);
			Unsubscribe(m_Client);

            Heartbeat.StopMonitoring();
            Heartbeat.Dispose();

			m_Client.Dispose();
		}

		/// <summary>
		/// Gets the current online status of the device.
		/// </summary>
		/// <returns></returns>
		protected override bool GetIsOnlineStatus()
		{
		    bool value = m_Client != null && m_Client.IsOnline;
            OnConnectedStateChanged.Raise(this, new BoolEventArgs(value));
			return value;
		}

		#region Methods

	    /// <summary>
		/// Connect to the device.
		/// </summary>
		public void Connect()
		{
			m_Client.Connect();
		}

		/// <summary>
		/// Disconnect from the device.
		/// </summary>
		public void Disconnect()
		{
			m_Client.Disconnect();
		}

		/// <summary>
		/// Sends the sig data to the device.
		/// </summary>
		/// <param name="sig"></param>
		public bool SendData(IXSig sig)
		{
			string data = StringUtils.ToString(sig.Data);
			return m_Client.Send(data);
		}

		#endregion

		#region Client Callbacks

		/// <summary>
		/// Subscribe to the client events.
		/// </summary>
		/// <param name="client"></param>
		private void Subscribe(ISerialPort client)
		{
			client.OnIsOnlineStateChanged += ClientOnIsOnlineStateChanged;
			client.OnSerialDataReceived += ClientOnSerialDataReceived;
		}

		/// <summary>
		/// Unsubscribe from the client events.
		/// </summary>
		/// <param name="client"></param>
		private void Unsubscribe(ISerialPort client)
		{
			client.OnIsOnlineStateChanged -= ClientOnIsOnlineStateChanged;
			client.OnSerialDataReceived -= ClientOnSerialDataReceived;
		}

		/// <summary>
		/// Called when we receive data from the client.
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="stringEventArgs"></param>
		private void ClientOnSerialDataReceived(object sender, StringEventArgs stringEventArgs)
		{
			m_Buffer.Enqueue(stringEventArgs.Data);
		}

		/// <summary>
		/// Called when the client online status changes.
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="boolEventArgs"></param>
		private void ClientOnIsOnlineStateChanged(object sender, BoolEventArgs boolEventArgs)
		{
			UpdateCachedOnlineStatus();
		}

		#endregion

		#region Buffer Callbacks

		/// <summary>
		/// Subscribe to the buffer events.
		/// </summary>
		/// <param name="buffer"></param>
		private void Subscribe(ISerialBuffer buffer)
		{
			buffer.OnCompletedSerial += BufferOnCompletedSerial;
		}

		/// <summary>
		/// Unsubscribe from the buffer events.
		/// </summary>
		/// <param name="buffer"></param>
		private void Unsubscribe(ISerialBuffer buffer)
		{
			buffer.OnCompletedSerial -= BufferOnCompletedSerial;
		}

		/// <summary>
		/// Called when we receive a complete message from the client.
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="stringEventArgs"></param>
		private void BufferOnCompletedSerial(object sender, StringEventArgs stringEventArgs)
		{
			IXSig sig = XSigParser.Parse(stringEventArgs.Data);
			OnSigEvent.Raise(this, new XSigEventArgs(sig));
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

			yield return m_Client;
		}

		/// <summary>
		/// Workaround for "unverifiable code" warning.
		/// </summary>
		/// <returns></returns>
		private IEnumerable<IConsoleNodeBase> GetBaseConsoleNodes()
		{
			return base.GetConsoleNodes();
		}

		/// <summary>
		/// Calls the delegate for each console status item.
		/// </summary>
		/// <param name="addRow"></param>
		public override void BuildConsoleStatus(AddStatusRowDelegate addRow)
		{
			base.BuildConsoleStatus(addRow);

			addRow("Address", Address);
			addRow("Port", Port);
		}

		#endregion
	}
}