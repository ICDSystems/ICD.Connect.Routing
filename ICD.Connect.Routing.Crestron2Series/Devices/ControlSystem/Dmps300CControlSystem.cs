using ICD.Connect.Settings.Core;

namespace ICD.Connect.Routing.Crestron2Series.Devices.ControlSystem
{
	public sealed class Dmps300CControlSystem : AbstractDmps300CDevice<Dmps300CControlSystemSettings>, IDmps300CComPortDevice
	{
		private const ushort PORT = 8700;
		private const ushort SERIAL_COMSPEC_JOIN = 317;

		/// <summary>
		/// Constructor.
		/// </summary>
		public Dmps300CControlSystem()
		{
			Controls.Add(new Dmps300CControlSystemSwitcherControl(this));
		}

		/// <summary>
		/// Gets the com spec join for the device.
		/// </summary>
		public ushort ComSpecJoin
		{
			get { return SERIAL_COMSPEC_JOIN; }
		}

		#region Settings

		/// <summary>
		/// Override to clear the instance settings.
		/// </summary>
		protected override void ClearSettingsFinal()
		{
			base.ClearSettingsFinal();

			Address = null;
			Port = PORT;
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
			Port = PORT;

			Connect();
		}

		#endregion
	}
}
