using System;
using ICD.Common.Utils.EventArguments;

namespace ICD.Connect.Routing.CrestronPro.ControlSystem.Controls.Volume.Crosspoints
{
	public interface IDmps3Crosspoint : IDisposable
	{
		/// <summary>
		/// Raised when the crosspoint volume level changes.
		/// </summary>
		event EventHandler<GenericEventArgs<short>> OnVolumeLevelChanged;

		/// <summary>
		/// Raised when the crosspoint mute state changes.
		/// </summary>
		event EventHandler<BoolEventArgs> OnMuteStateChanged;

		/// <summary>
		/// Gets the crosspoint volume level.
		/// </summary>
		short VolumeLevel { get; }

		/// <summary>
		/// Gets the minimum crosspoint volume level.
		/// </summary>
		short VolumeLevelMin { get; }

		/// <summary>
		/// Gets the maximum crosspoint volume level.
		/// </summary>
		short VolumeLevelMax { get; }

		/// <summary>
		/// Gets the crosspoint mute state.
		/// </summary>
		bool VolumeIsMuted { get; }

		/// <summary>
		/// Sets the crosspoint volume level.
		/// </summary>
		/// <param name="volume"></param>
		void SetVolumeLevel(short volume);

		/// <summary>
		/// Toggles the crosspoint mute state.
		/// </summary>
		void VolumeMuteToggle();

		/// <summary>
		/// Sets the crosspoint mute state.
		/// </summary>
		/// <param name="mute"></param>
		void SetVolumeMute(bool mute);
	}
}
