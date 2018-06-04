using ICD.Connect.Settings.Core;

namespace ICD.Connect.Routing.Crestron2Series.Devices.Endpoints.Transmitter
{
	public sealed class Dmps300CTransmitter : AbstractDmps300CEndpointDevice<Dmps300CTransmitterSettings>
	{
		private const ushort START_PORT = 8710;
		private const ushort PORT_INCREMENT = 10;
		private const ushort START_DM_INPUT = 6;

		private int m_DmInput;

		/// <summary>
		/// Constructor.
		/// </summary>
		public Dmps300CTransmitter()
		{
			Controls.Add(new Dmps300CTransmitterSourceControl(this));
		}

		protected override void ClearSettingsFinal()
		{
			base.ClearSettingsFinal();

			m_DmInput = 0;
		}

		protected override void CopySettingsFinal(Dmps300CTransmitterSettings settings)
		{
			base.CopySettingsFinal(settings);

			settings.DmInput = m_DmInput;
		}

		protected override void ApplySettingsFinal(Dmps300CTransmitterSettings settings, IDeviceFactory factory)
		{
			base.ApplySettingsFinal(settings, factory);

			m_DmInput = settings.DmInput;
			Port = (ushort)(START_PORT + PORT_INCREMENT * (m_DmInput - START_DM_INPUT));
		}
	}
}
