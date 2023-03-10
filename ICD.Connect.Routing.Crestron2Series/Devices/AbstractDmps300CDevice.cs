using System;
using System.Collections.Generic;
using ICD.Common.Utils;
using ICD.Common.Utils.EventArguments;
using ICD.Common.Utils.Extensions;
using ICD.Connect.API.Nodes;
using ICD.Connect.Devices;
using ICD.Connect.Protocol;
using ICD.Connect.Protocol.EventArguments;
using ICD.Connect.Protocol.Network.Ports.Tcp;
using ICD.Connect.Protocol.Ports;
using ICD.Connect.Protocol.SerialBuffers;
using ICD.Connect.Protocol.XSig;

namespace ICD.Connect.Routing.Crestron2Series.Devices
{
	public abstract class AbstractDmps300CDevice<TSettings> : AbstractDevice<TSettings>, IDmps300CDevice
		where TSettings : IDmps300CDeviceSettings, new()
	{
		public event EventHandler<XSigEventArgs> OnSigEvent;

		private readonly IcdTcpClient m_Client;
		private readonly XSigSerialBuffer m_Buffer;
		private readonly ConnectionStateManager m_ConnectionStateManager;

		#region Properties

		/// <summary>
		/// Gets the network address of the device.
		/// </summary>
		public string Address { get { return m_Client.Address; } set { m_Client.Address = value; } }

		/// <summary>
		/// Gets the network port of the device.
		/// </summary>
		public ushort Port { get { return m_Client.Port; } protected set { m_Client.Port = value; } }

		#endregion

		/// <summary>
		/// Constructor.
		/// </summary>
		protected AbstractDmps300CDevice()
		{
            m_Client = new IcdTcpClient();
			m_Buffer = new XSigSerialBuffer();

			Subscribe(m_Buffer);

			m_ConnectionStateManager = new ConnectionStateManager(this);
			m_ConnectionStateManager.OnIsOnlineStateChanged += ClientOnIsOnlineStateChanged;
			m_ConnectionStateManager.OnSerialDataReceived += ClientOnSerialDataReceived;
			
			SetPort(m_Client);
		}

		/// <summary>
		/// Release resources.
		/// </summary>
		protected override void DisposeFinal(bool disposing)
		{
			OnSigEvent = null;

			base.DisposeFinal(disposing);

			Unsubscribe(m_Buffer);

			SetPort(null);
			m_ConnectionStateManager.OnIsOnlineStateChanged -= ClientOnIsOnlineStateChanged;
			m_ConnectionStateManager.OnSerialDataReceived -= ClientOnSerialDataReceived;
            m_ConnectionStateManager.Dispose();

			m_Client.Dispose();
		}

		/// <summary>
		/// Gets the current online status of the device.
		/// </summary>
		/// <returns></returns>
		protected override bool GetIsOnlineStatus()
		{
			return m_ConnectionStateManager != null && m_ConnectionStateManager.IsOnline;
		}

		#region Methods

		/// <summary>
		/// Sends the sig data to the device.
		/// </summary>
		/// <param name="sig"></param>
		public bool SendData(IXSig sig)
		{
			string data = StringUtils.ToString(sig.Data);
			return m_ConnectionStateManager.Send(data);
		}

		private void SetPort(ISerialPort port)
		{
			m_ConnectionStateManager.SetPort(port, false);
		}

		#endregion

		#region Client Callbacks

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
		/// <param name="args"></param>
		private void ClientOnIsOnlineStateChanged(object sender, BoolEventArgs args)
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

		#region Settings

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
