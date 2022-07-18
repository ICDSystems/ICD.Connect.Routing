using System;
using ICD.Connect.Audio.Proxies.Controls.Volume;
using ICD.Connect.Devices.Controls;
using ICD.Connect.Devices.Proxies.Devices;
using ICD.Connect.Settings;

namespace ICD.Connect.Routing.SPlus.SPlusVolumeDevice.Proxy
{
    public sealed class ProxySPlusVolumeDevice : AbstractProxyDevice<ProxySPlusVolumeDeviceSettings>
    {
        private const int VOLUME_CONTROL_ID = 2;

        /// <summary>
        /// Override to add controls to the device.
        /// </summary>
        /// <param name="settings"></param>
        /// <param name="factory"></param>
        /// <param name="addControl"></param>
        protected override void AddControls(ProxySPlusVolumeDeviceSettings settings, IDeviceFactory factory, Action<IDeviceControl> addControl)
        {
            base.AddControls(settings, factory, addControl);

            addControl(new ProxyVolumeDeviceControl(this, VOLUME_CONTROL_ID));
        }
    }
}