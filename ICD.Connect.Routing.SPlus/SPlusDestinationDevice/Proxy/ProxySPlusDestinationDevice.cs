using ICD.Connect.Audio.Proxies.Controls.Volume;
using ICD.Connect.Devices.Proxies.Controls;
using ICD.Connect.Devices.Proxies.Devices;
using ICD.Connect.Settings;

namespace ICD.Connect.Routing.SPlus.SPlusDestinationDevice.Proxy
{
	public sealed class ProxySPlusDestinationDevice : AbstractProxyDevice<ProxySPlusDestinationDeviceSettings>, IProxySPlusDestinationDevice
	{
		#region Consts

		private const int ROUTE_CONTROL_ID = 0;
		private const int POWER_CONTROL_ID = 1;
		private const int VOLUME_CONTROL_ID = 2;

		#endregion

		#region Fields

		#endregion

		#region Properties

		internal ProxySPlusDestinationRouteControl RouteControl { get; private set; }
		internal ProxyPowerDeviceControl PowerControl { get; private set; }
		internal ProxyVolumeDeviceControl VolumeControl { get; private set; }

		#endregion

		#region Settings

		/// <summary>
		/// Override to apply settings to the instance.
		/// </summary>
		/// <param name="settings"></param>
		/// <param name="factory"></param>
		protected override void ApplySettingsFinal(ProxySPlusDestinationDeviceSettings settings, IDeviceFactory factory)
		{
			base.ApplySettingsFinal(settings, factory);

			//Note: Order is important - power control must be first
			if (settings.PowerControl)
			{
				PowerControl = new ProxyPowerDeviceControl(this, POWER_CONTROL_ID);
				Controls.Add(PowerControl);
			}

			RouteControl = new ProxySPlusDestinationRouteControl(this, ROUTE_CONTROL_ID, settings.InputCount);
			Controls.Add(RouteControl);


			if (settings.VolumeControl)
			{
				VolumeControl = new ProxyVolumeDeviceControl(this, VOLUME_CONTROL_ID);
				Controls.Add(VolumeControl);
			}
		}

		/// <summary>
		/// Override to apply properties to the settings instance.
		/// </summary>
		/// <param name="settings"></param>
		protected override void CopySettingsFinal(ProxySPlusDestinationDeviceSettings settings)
		{
			base.CopySettingsFinal(settings);

			if (RouteControl != null)
				settings.InputCount = RouteControl.InputCount;

			if (PowerControl != null)
				settings.PowerControl = true;
			if (VolumeControl != null)
				settings.VolumeControl = true;
		}

		/// <summary>
		/// Override to clear the instance settings.
		/// </summary>
		protected override void ClearSettingsFinal()
		{
			base.ClearSettingsFinal();

			if (RouteControl != null)
			{
				Controls.Remove(ROUTE_CONTROL_ID);
				RouteControl.Dispose();
				RouteControl = null;
			}

			if (PowerControl != null)
			{
				Controls.Remove(POWER_CONTROL_ID);
				PowerControl.Dispose();
				PowerControl = null;
			}
			if (VolumeControl != null)
			{
				Controls.Remove(VOLUME_CONTROL_ID);
				VolumeControl.Dispose();
				VolumeControl = null;
			}
		}

		#endregion

	}
}