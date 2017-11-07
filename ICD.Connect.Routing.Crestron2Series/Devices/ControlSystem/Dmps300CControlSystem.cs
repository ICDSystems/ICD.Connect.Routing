using ICD.Connect.Settings.Core;

namespace ICD.Connect.Routing.Crestron2Series.Devices.ControlSystem
{
	public sealed class Dmps300CControlSystem : AbstractDmps300CDevice<Dmps300CControlSystemSettings>
	{
		private const ushort PORT = 8700;

		/// <summary>
		/// Constructor.
		/// </summary>
		public Dmps300CControlSystem()
		{
			Controls.Add(new Dmps300CControlSystemSwitcherControl(this));
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
		}

		#endregion
	}
}
