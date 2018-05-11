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

		/// <summary>
		/// Constructor.
		/// </summary>
		public Dmps300CReceiver()
		{
			Controls.Add(new Dmps300CReceiverDestinationControl(this));
		}

		/// <summary>
		/// Gets the com spec join for the device.
		/// </summary>
		public ushort ComSpecJoin
		{
			get { return SERIAL_COMSPEC_JOIN; }
		}

		protected override void ClearSettingsFinal()
		{
			base.ClearSettingsFinal();

			m_DmOutput = 0;
		}

		protected override void CopySettingsFinal(Dmps300CReceiverSettings settings)
		{
			base.CopySettingsFinal(settings);

			settings.DmOutput = m_DmOutput;
		}

		protected override void ApplySettingsFinal(Dmps300CReceiverSettings settings, IDeviceFactory factory)
		{
			base.ApplySettingsFinal(settings, factory);

			m_DmOutput = settings.DmOutput;
			Port = (ushort)(START_PORT + PORT_INCREMENT * (m_DmOutput - START_DM_OUTPUT));

			Connect();
		}
	}
}
