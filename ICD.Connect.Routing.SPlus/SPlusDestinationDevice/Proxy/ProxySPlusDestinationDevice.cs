using System;
using ICD.Connect.Audio.Proxies.Controls.Volume;
using ICD.Connect.Devices.Controls;
using ICD.Connect.Devices.Proxies.Controls;
using ICD.Connect.Devices.Proxies.Devices;
using ICD.Connect.Settings;

namespace ICD.Connect.Routing.SPlus.SPlusDestinationDevice.Proxy
{
	public sealed class ProxySPlusDestinationDevice : AbstractProxyDevice<ProxySPlusDestinationDeviceSettings>, IProxySPlusDestinationDevice
	{
		private const int ROUTE_CONTROL_ID = 0;
		private const int POWER_CONTROL_ID = 1;
		private const int VOLUME_CONTROL_ID = 2;

		#region Properties

		private ProxySPlusDestinationRouteControl RouteControl
		{
			get { return Controls.GetControl<ProxySPlusDestinationRouteControl>(); }
		}

		private ProxyPowerDeviceControl PowerControl
		{
			get { return Controls.GetControl<ProxyPowerDeviceControl>(); }
		}

		private ProxyVolumeDeviceControl VolumeControl
		{
			get { return Controls.GetControl<ProxyVolumeDeviceControl>(); }
		}

		#endregion

		#region Settings

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
		/// Override to add controls to the device.
		/// </summary>
		/// <param name="settings"></param>
		/// <param name="factory"></param>
		/// <param name="addControl"></param>
		protected override void AddControls(ProxySPlusDestinationDeviceSettings settings, IDeviceFactory factory, Action<IDeviceControl> addControl)
		{
			base.AddControls(settings, factory, addControl);

			//Note: Order is important - power control must be first
			if (settings.PowerControl)
				addControl(new ProxyPowerDeviceControl(this, POWER_CONTROL_ID));

			addControl(new ProxySPlusDestinationRouteControl(this, ROUTE_CONTROL_ID, settings.InputCount));

			if (settings.VolumeControl)
				addControl(new ProxyVolumeDeviceControl(this, VOLUME_CONTROL_ID));
		}

		#endregion

	}
}