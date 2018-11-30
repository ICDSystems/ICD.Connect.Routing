using System;
using ICD.Common.Utils.EventArguments;

namespace ICD.Connect.Routing.CrestronPro.ControlSystem.Controls.Volume.Crosspoints
{
	public interface IDmps3Crosspoint : IDisposable
	{
		event EventHandler<GenericEventArgs<short>> OnVolumeLevelChanged;
		event EventHandler<BoolEventArgs> OnMuteStateChanged;

		short VolumeLevel { get; }
		short VolumeRawMinAbsolute { get; }
		short VolumeRawMaxAbsolute { get; }
		bool VolumeIsMuted { get; }

		void SetVolumeLevel(short volume);
		void VolumeMuteToggle();
		void SetVolumeMute(bool mute);
	}
}
