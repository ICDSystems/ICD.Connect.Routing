using ICD.Common.Utils.EventArguments;
using ICD.Connect.Protocol.EventArguments;
using ICD.Connect.Protocol.Ports.DigitalInput;
using ICD.Connect.Protocol.XSig;
using ICD.Connect.Routing.Crestron2Series.Devices;
using ICD.Connect.Settings.Core;

namespace ICD.Connect.Routing.Crestron2Series.Ports.DigitalInputPort
{
	public sealed class Dmps300CDigitalInputPort : AbstractDigitalInputPort<Dmps300CDigitalInputPortSettings>
	{
		private const ushort START_DIGITAL_JOIN = 295;

		private IDmps300CDevice m_Device;

		#region Properties

		/// <summary>
		/// Gets the port address on the DMPS.
		/// </summary>
		public int Address { get; set; }

		/// <summary>
		/// Gets the digital join index for the relay.
		/// </summary>
		private ushort DigitalJoinIndex { get { return (ushort)(START_DIGITAL_JOIN + (Address - 1)); } }

		#endregion

		#region Methods

		/// <summary>
		/// Sets the wrapped parent device.
		/// </summary>
		/// <param name="device"></param>
		public void SetDevice(IDmps300CDevice device)
		{
			if (device == m_Device)
				return;

			Unsubscribe(m_Device);
			m_Device = device;
			Subscribe(m_Device);

			UpdateCachedOnlineStatus();
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
		protected override void CopySettingsFinal(Dmps300CDigitalInputPortSettings settings)
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
		protected override void ApplySettingsFinal(Dmps300CDigitalInputPortSettings settings, IDeviceFactory factory)
		{
			base.ApplySettingsFinal(settings, factory);

			Address = settings.Address;

			IDmps300CDevice device = factory.GetOriginatorById<IDmps300CDevice>(settings.Device);
			SetDevice(device);
		}

		#endregion

		#region Parent Callbacks

		/// <summary>
		/// Subscribe to the parent events.
		/// </summary>
		/// <param name="device"></param>
		private void Subscribe(IDmps300CDevice device)
		{
			if (device == null)
				return;

			device.OnIsOnlineStateChanged += DeviceOnIsOnlineStateChanged;
			device.OnSigEvent += DeviceOnSigEvent;
		}

		/// <summary>
		/// Unsubscribe from the parent events.
		/// </summary>
		/// <param name="device"></param>
		private void Unsubscribe(IDmps300CDevice device)
		{
			if (device == null)
				return;

			device.OnIsOnlineStateChanged -= DeviceOnIsOnlineStateChanged;
			device.OnSigEvent -= DeviceOnSigEvent;
		}

		/// <summary>
		/// Called when the parent online state changes.
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="boolEventArgs"></param>
		private void DeviceOnIsOnlineStateChanged(object sender, BoolEventArgs boolEventArgs)
		{
			UpdateCachedOnlineStatus();
		}

		/// <summary>
		/// Called when we receive a sig from the device.
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="xSigEventArgs"></param>
		private void DeviceOnSigEvent(object sender, XSigEventArgs xSigEventArgs)
		{
			if (xSigEventArgs.Data is DigitalXSig)
				HandleDigitalSigEvent((DigitalXSig)xSigEventArgs.Data);
		}

		/// <summary>
		/// Called when we receive a digital sig from the device.
		/// </summary>
		/// <param name="data"></param>
		private void HandleDigitalSigEvent(DigitalXSig data)
		{
			if (data.Index == DigitalJoinIndex)
				State = data.Value;
		}

		#endregion
	}
}
