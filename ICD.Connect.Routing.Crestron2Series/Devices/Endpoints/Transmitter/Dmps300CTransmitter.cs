using System;
using ICD.Connect.Devices.Controls;
using ICD.Connect.Settings;

namespace ICD.Connect.Routing.Crestron2Series.Devices.Endpoints.Transmitter
{
	public sealed class Dmps300CTransmitter : AbstractDmps300CEndpointDevice<Dmps300CTransmitterSettings>
	{
		private const ushort START_PORT = 8710;
		private const ushort PORT_INCREMENT = 10;
		private const ushort START_DM_INPUT = 6;

		private int m_DmInput;

		#region Settings

		/// <summary>
		/// Override to clear the instance settings.
		/// </summary>
		protected override void ClearSettingsFinal()
		{
			base.ClearSettingsFinal();

			m_DmInput = 0;
		}

		/// <summary>
		/// Override to apply properties to the settings instance.
		/// </summary>
		/// <param name="settings"></param>
		protected override void CopySettingsFinal(Dmps300CTransmitterSettings settings)
		{
			base.CopySettingsFinal(settings);

			settings.DmInput = m_DmInput;
		}

		/// <summary>
		/// Override to apply settings to the instance.
		/// </summary>
		/// <param name="settings"></param>
		/// <param name="factory"></param>
		protected override void ApplySettingsFinal(Dmps300CTransmitterSettings settings, IDeviceFactory factory)
		{
			base.ApplySettingsFinal(settings, factory);

			m_DmInput = settings.DmInput;
			Port = (ushort)(START_PORT + PORT_INCREMENT * (m_DmInput - START_DM_INPUT));
		}

		/// <summary>
		/// Override to add controls to the device.
		/// </summary>
		/// <param name="settings"></param>
		/// <param name="factory"></param>
		/// <param name="addControl"></param>
		protected override void AddControls(Dmps300CTransmitterSettings settings, IDeviceFactory factory, Action<IDeviceControl> addControl)
		{
			base.AddControls(settings, factory, addControl);

			addControl(new Dmps300CTransmitterSourceControl(this));
		}

		#endregion
	}
}
