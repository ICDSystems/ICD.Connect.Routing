using System;
using ICD.Connect.Devices.CrestronSPlus.Devices.SPlus;
using ICD.Connect.Routing.SPlus.EventArgs;

namespace ICD.Connect.Routing.SPlus.SPlusVolumeDevice.Shim
{
    public interface ISPlusVolumeDeviceShimmable : ISPlusDevice
    {
        #region Events To Shim

        event EventHandler<SetVolumeLevelEventArgs> OnSetVolumeLevel;

        event EventHandler<SetVolumeMuteStateEventArgs> OnSetVolumeMuteState;

        event EventHandler<VolumeMuteToggleEventArgs> OnVolumeMuteToggle;

        #endregion


        #region Methods From Shim

        void SetVolumeLevelFeedback(ushort volume);

        void SetVolumeMuteStateFeedback(bool state);

        #endregion 
    }
}