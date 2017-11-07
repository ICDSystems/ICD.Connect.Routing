using System;
using ICD.Common.Properties;
using ICD.Common.Utils;
using ICD.Common.Utils.EventArguments;
using ICD.Common.Utils.Extensions;
using ICD.Connect.API.Nodes;
using ICD.Connect.Devices;
using ICD.Connect.Protocol.EventArguments;
using ICD.Connect.Protocol.Network.Tcp;
using ICD.Connect.Protocol.Ports;
using ICD.Connect.Protocol.SerialBuffers;
using ICD.Connect.Protocol.XSig;
using ICD.Connect.Settings.Core;

namespace ICD.Connect.Routing.Crestron2Series.ControlSystem
{
	public sealed class Dmps300CControlSystem : AbstractDevice<Dmps300CControlSystemSettings>
	{
		private const ushort PORT = 8700;

		public event EventHandler<XSigEventArgs> OnSigEvent; 

		private readonly AsyncTcpClient m_Client;
		private readonly XSigSerialBuffer m_Buffer;

		#region Properties

		/// <summary>
		/// Gets/sets the network address of the 
		/// </summary>
		[PublicAPI]
		public string Address { get { return m_Client.Address; } set { m_Client.Address = value; } }

		#endregion

		/// <summary>
		/// Constructor.
		/// </summary>
		public Dmps300CControlSystem()
		{
			m_Client = new AsyncTcpClient
			{
				Port = PORT
			};
			m_Buffer = new XSigSerialBuffer();

			Subscribe(m_Buffer);
			Subscribe(m_Client);
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

			m_Client.Dispose();
		}

		/// <summary>
		/// Gets the current online status of the device.
		/// </summary>
		/// <returns></returns>
		protected override bool GetIsOnlineStatus()
		{
			return m_Client.IsOnline;
		}

		#region Methods

		/// <summary>
		/// Sends the sig data to the device.
		/// </summary>
		/// <param name="sig"></param>
		public void SendData(IXSig sig)
		{
			string data = StringUtils.ToString(sig.Data);
			m_Client.Send(data);
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

		#region Settings

		/// <summary>
		/// Override to clear the instance settings.
		/// </summary>
		protected override void ClearSettingsFinal()
		{
			base.ClearSettingsFinal();

			Address = null;
		}

		/// <summary>
		/// Override to apply properties to the settings instance.
		/// </summary>
		/// <param name="settings"></param>
		protected override void CopySettingsFinal(Dmps300CControlSystemSettings settings)
		{
			base.CopySettingsFinal(settings);

			settings.Address = Address;
		}

		/// <summary>
		/// Override to apply settings to the instance.
		/// </summary>
		/// <param name="settings"></param>
		/// <param name="factory"></param>
		protected override void ApplySettingsFinal(Dmps300CControlSystemSettings settings, IDeviceFactory factory)
		{
			base.ApplySettingsFinal(settings, factory);

			Address = settings.Address;
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

			addRow("Address", Address);
		}

		#endregion
	}
}
