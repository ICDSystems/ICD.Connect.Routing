using ICD.Common.Utils.Services.Logging;
using ICD.Connect.Devices.EventArguments;
using ICD.Connect.Protocol.Ports.RelayPort;
using ICD.Connect.Protocol.XSig;
using ICD.Connect.Routing.Crestron2Series.Devices;
using ICD.Connect.Settings;

namespace ICD.Connect.Routing.Crestron2Series.Ports.RelayPort
{
	public sealed class Dmps300CRelayPort : AbstractRelayPort<Dmps300CRelayPortSettings>
	{
		private IDmps300CRelayPortDevice m_Device;

		#region Properties

		/// <summary>
		/// Gets the port address on the DMPS.
		/// </summary>
		public int Address { get; set; }

		/// <summary>
		/// Gets the digital join index for the relay.
		/// </summary>
		private ushort DigitalJoinIndex {
		    get
		    {
		        if (m_Device == null)
		            return 0;
		        return (ushort)(m_Device.RelayOutputStartJoin + (Address - 1));
		    } }

		#endregion

		#region Methods

		/// <summary>
		/// Sets the wrapped parent device.
		/// </summary>
		/// <param name="device"></param>
		public void SetDevice(IDmps300CRelayPortDevice device)
		{
			if (device == m_Device)
				return;

			Unsubscribe(m_Device);
			m_Device = device;
			Subscribe(m_Device);

			UpdateCachedOnlineStatus();
		}

		/// <summary>
		/// Open the relay
		/// </summary>
		public override void Open()
		{
			if (m_Device == null)
			{
				Logger.Log(eSeverity.Error, "Unable to open - Parent device is null");
				return;
			}

			if (m_Device.SendData(new DigitalXSig(false, DigitalJoinIndex)))
				Closed = false;
		}

		/// <summary>
		/// Close the relay
		/// </summary>
		public override void Close()
		{
			if (m_Device == null)
			{
				Logger.Log(eSeverity.Error, "Unable to open - Parent device is null");
				return;
			}

			if (m_Device.SendData(new DigitalXSig(true, DigitalJoinIndex)))
				Closed = true;
		}

		/// <summary>
		/// Gets the current online status of the device.
		/// </summary>
		/// <returns></returns>
		protected override bool GetIsOnlineStatus()
		{
			return m_Device != null && m_Device.IsOnline;
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
		protected override void CopySettingsFinal(Dmps300CRelayPortSettings settings)
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
		protected override void ApplySettingsFinal(Dmps300CRelayPortSettings settings, IDeviceFactory factory)
		{
			base.ApplySettingsFinal(settings, factory);

			Address = settings.Address;

			IDmps300CRelayPortDevice device = factory.GetOriginatorById<IDmps300CRelayPortDevice>(settings.Device);
			SetDevice(device);
		}

		#endregion

		#region Parent Callbacks 

		/// <summary>
		/// Subscribe to the parent events.
		/// </summary>
		/// <param name="device"></param>
		private void Subscribe(IDmps300CRelayPortDevice device)
		{
			if (device == null)
				return;

			device.OnIsOnlineStateChanged += DeviceOnIsOnlineStateChanged;
		}

		/// <summary>
		/// Unsubscribe from the parent events.
		/// </summary>
		/// <param name="device"></param>
		private void Unsubscribe(IDmps300CRelayPortDevice device)
		{
			if (device == null)
				return;

			device.OnIsOnlineStateChanged -= DeviceOnIsOnlineStateChanged;
		}

		/// <summary>
		/// Called when the parent online state changes.
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="args"></param>
		private void DeviceOnIsOnlineStateChanged(object sender, DeviceBaseOnlineStateApiEventArgs args)
		{
			UpdateCachedOnlineStatus();
		}

		#endregion
	}
}
