using ICD.Connect.Devices;

namespace ICD.Connect.Routing.SPlus.Controls.Volume
{
    public interface ISPlusVolumeDeviceControlParent : IDevice
    {
        void SetVolumeLevel(ushort level);

        void VolumeMuteToggle();
        void SetVolumeMuteState(bool mute);
    }
}