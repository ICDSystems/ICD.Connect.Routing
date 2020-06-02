using System;
using System.Collections.Generic;
using ICD.Common.Utils.EventArguments;
using ICD.Common.Utils.Extensions;
using ICD.Common.Utils.Services.Logging;
using ICD.Connect.API.Nodes;
using ICD.Connect.Devices;
using ICD.Connect.Devices.Controls;
using ICD.Connect.Protocol;
using ICD.Connect.Protocol.Extensions;
using ICD.Connect.Protocol.Network.Ports;
using ICD.Connect.Protocol.Network.Settings;
using ICD.Connect.Protocol.Ports;
using ICD.Connect.Protocol.Ports.ComPort;
using ICD.Connect.Protocol.SerialBuffers;
using ICD.Connect.Protocol.Settings;
using ICD.Connect.Routing.AVPro.Controls;
using ICD.Connect.Settings;

namespace ICD.Connect.Routing.AVPro.Devices.Switchers
{
	public abstract class AbstractAvProSwitcherDevice<TSettings> : AbstractDevice<TSettings>, IAvProSwitcherDevice
		where TSettings : IAvProSwitcherDeviceSettings, new()
	{
		/// <summary>
		/// Raised when the class initializes.
		/// </summary>
		public event EventHandler<BoolEventArgs> OnInitializedChanged;

		/// <summary>
		/// Raised when the device sends a response.
		/// </summary>
		public event EventHandler<StringEventArgs> OnResponseReceived;

		private readonly ISerialBuffer m_SerialBuffer;
		private readonly ConnectionStateManager m_ConnectionStateManager;

		private readonly NetworkProperties m_NetworkProperties;
		private readonly ComSpecProperties m_ComSpecProperties;

		private bool m_Initialized;
		
		#region Properties

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

		/// <summary>
		/// Gets the number of AV inputs.
		/// </summary>
		public abstract int NumberOfInputs { get; }

		/// <summary>
		/// Gets the number of AV outputs.
		/// </summary>
		public abstract int NumberOfOutputs { get; }

		#endregion

		/// <summary>
		/// Constructor.
		/// </summary>
		protected AbstractAvProSwitcherDevice()
		{
			m_NetworkProperties = new NetworkProperties();
			m_ComSpecProperties = new ComSpecProperties();

			m_SerialBuffer = new MultiDelimiterSerialBuffer('\r', '\n');
			m_ConnectionStateManager = new ConnectionStateManager(this) { ConfigurePort = ConfigurePort };

			Subscribe(m_SerialBuffer);
			Subscribe(m_ConnectionStateManager);
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

			m_ConnectionStateManager.Send(command + "\r\n");
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
		/// Called when we receive a complete response from the device.
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="args"></param>
		protected virtual void BufferOnCompletedSerial(object sender, StringEventArgs args)
		{
			OnResponseReceived.Raise(this, new StringEventArgs(args.Data));
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

			Initialized = e.Data;
		}

		/// <summary>
		/// Called when the port receives serial data from the device.
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void PortOnSerialDataReceived(object sender, StringEventArgs e)
		{
			m_SerialBuffer.Enqueue(e.Data);

			Initialized = true;
		}

		#endregion

		#region Settings

		/// <summary>
		/// Override to clear the instance settings.
		/// </summary>
		protected override void ClearSettingsFinal()
		{
			base.ClearSettingsFinal();

			m_ComSpecProperties.ClearComSpecProperties();
			m_NetworkProperties.ClearNetworkProperties();

			m_ConnectionStateManager.SetPort(null);
		}

		/// <summary>
		/// Override to apply properties to the settings instance.
		/// </summary>
		/// <param name="settings"></param>
		protected override void CopySettingsFinal(TSettings settings)
		{
			base.CopySettingsFinal(settings);

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

			m_ConnectionStateManager.SetPort(port);
		}

		/// <summary>
		/// Override to add controls to the device.
		/// </summary>
		/// <param name="settings"></param>
		/// <param name="factory"></param>
		/// <param name="addControl"></param>
		protected override void AddControls(TSettings settings, IDeviceFactory factory, Action<IDeviceControl> addControl)
		{
			base.AddControls(settings, factory, addControl);

			addControl(new AvProSwitcherControl(this, 0));
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