using System;
using ICD.Common.Utils.Extensions;
using ICD.Connect.Devices.Controls;
using ICD.Connect.Devices.CrestronSPlus.Devices.SPlus;
using ICD.Connect.Routing.SPlus.Controls.Volume;
using ICD.Connect.Routing.SPlus.EventArgs;
using ICD.Connect.Routing.SPlus.SPlusVolumeDevice.Shim;
using ICD.Connect.Settings;

namespace ICD.Connect.Routing.SPlus.SPlusVolumeDevice.Device
{
    public sealed class SPlusVolumeDevice : AbstractSPlusDevice<SPlusVolumeDeviceSettings>, ISPlusVolumeDeviceControlParent, ISPlusVolumeDeviceShimmable
    {

        private const int VOLUME_CONTROL_ID = 2;

        private SPlusVolumeDeviceControl VolumeControl
        {
            get { return Controls.GetControl<SPlusVolumeDeviceControl>(); }
        }

        #region Volume Control
        #region Events to Shim

        public event EventHandler<SetVolumeLevelEventArgs> OnSetVolumeLevel;

        public event EventHandler<SetVolumeMuteStateEventArgs> OnSetVolumeMuteState;

        public event EventHandler<VolumeMuteToggleEventArgs> OnVolumeMuteToggle;

        #endregion

        #region Methods From Shim

        public void SetVolumeLevelFeedback(ushort volume)
        {
            if (VolumeControl != null)
                VolumeControl.SetVolumeFeedback(volume);
        }

        public void SetVolumeMuteStateFeedback(bool state)
        {
            if (VolumeControl != null)
                VolumeControl.SetVolumeIsMutedFeedback(state);
        }

        #endregion

        #region Methods From Control

        public void SetVolumeLevel(ushort volume)
        {
            OnSetVolumeLevel.Raise(this, new SetVolumeLevelEventArgs(volume));
        }

        public void SetVolumeMuteState(bool state)
        {
            OnSetVolumeMuteState.Raise(this, new SetVolumeMuteStateEventArgs(state));
        }

        public void VolumeMuteToggle()
        {
            OnVolumeMuteToggle.Raise(this, new VolumeMuteToggleEventArgs());
        }

        #endregion

        #endregion

        #region Settings

        /// <summary>
        /// Override to add controls to the device.
        /// </summary>
        /// <param name="settings"></param>
        /// <param name="factory"></param>
        /// <param name="addControl"></param>
        protected override void AddControls(SPlusVolumeDeviceSettings settings, IDeviceFactory factory, Action<IDeviceControl> addControl)
        {
            base.AddControls(settings, factory, addControl);

            addControl(new SPlusVolumeDeviceControl(this, VOLUME_CONTROL_ID));
        }

        #endregion
    }
}