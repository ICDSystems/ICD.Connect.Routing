using System;
using ICD.Connect.Devices.Controls;
using ICD.Connect.Settings;

namespace ICD.Connect.Routing.Crestron2Series.Devices.Endpoints.Receiver
{
	public sealed class Dmps300CReceiver : AbstractDmps300CEndpointDevice<Dmps300CReceiverSettings>, IDmps300CComPortDevice
	{
		private const ushort START_PORT = 8730;
		private const ushort PORT_INCREMENT = 10;
		private const ushort START_DM_OUTPUT = 3;
		private const ushort SERIAL_COMSPEC_JOIN = 37;

		private int m_DmOutput;

		#region Properties

		/// <summary>
		/// Gets the com spec join for the device.
		/// </summary>
		public ushort ComSpecJoin
		{
			get { return SERIAL_COMSPEC_JOIN; }
		}

		#endregion

		#region Settings

		/// <summary>
		/// Override to clear the instance settings.
		/// </summary>
		protected override void ClearSettingsFinal()
		{
			base.ClearSettingsFinal();

			m_DmOutput = 0;
		}

		/// <summary>
		/// Override to apply properties to the settings instance.
		/// </summary>
		/// <param name="settings"></param>
		protected override void CopySettingsFinal(Dmps300CReceiverSettings settings)
		{
			base.CopySettingsFinal(settings);

			settings.DmOutput = m_DmOutput;
		}

		/// <summary>
		/// Override to apply settings to the instance.
		/// </summary>
		/// <param name="settings"></param>
		/// <param name="factory"></param>
		protected override void ApplySettingsFinal(Dmps300CReceiverSettings settings, IDeviceFactory factory)
		{
			base.ApplySettingsFinal(settings, factory);

			m_DmOutput = settings.DmOutput;
			Port = (ushort)(START_PORT + PORT_INCREMENT * (m_DmOutput - START_DM_OUTPUT));
		}

		/// <summary>
		/// Override to add controls to the device.
		/// </summary>
		/// <param name="settings"></param>
		/// <param name="factory"></param>
		/// <param name="addControl"></param>
		protected override void AddControls(Dmps300CReceiverSettings settings, IDeviceFactory factory, Action<IDeviceControl> addControl)
		{
			base.AddControls(settings, factory, addControl);

			addControl(new Dmps300CReceiverDestinationControl(this));
		}

		#endregion
	}
}
