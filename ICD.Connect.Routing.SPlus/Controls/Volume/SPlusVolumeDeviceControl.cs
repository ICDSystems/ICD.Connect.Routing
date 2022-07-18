using System;
using ICD.Connect.Audio.Controls.Volume;

namespace ICD.Connect.Routing.SPlus.Controls.Volume
{
	public sealed class SPlusVolumeDeviceControl : AbstractVolumeDeviceControl<ISPlusVolumeDeviceControlParent>
	{
		#region Properties

		/// <summary>
		/// Gets the minimum supported volume level.
		/// </summary>
		public override float VolumeLevelMin { get { return ushort.MinValue; } }

		/// <summary>
		/// Gets the maximum supported volume level.
		/// </summary>
		public override float VolumeLevelMax { get { return ushort.MaxValue; } }

		#endregion

		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="parent">Device this control belongs to</param>
		/// <param name="id">Id of this control in the device</param>
		public SPlusVolumeDeviceControl(ISPlusVolumeDeviceControlParent parent, int id)
			: base(parent, id)
		{
			SupportedVolumeFeatures = eVolumeFeatures.Mute |
			                          eVolumeFeatures.MuteAssignment |
			                          eVolumeFeatures.MuteFeedback |
			                          eVolumeFeatures.Volume |
			                          eVolumeFeatures.VolumeAssignment |
			                          eVolumeFeatures.VolumeFeedback;
		}

		#region Methods

		/// <summary>
		/// Sets the raw volume level in the device volume representation.
		/// </summary>
		/// <param name="level"></param>
		public override void SetVolumeLevel(float level)
		{
			Parent.SetVolumeLevel((ushort)level);
		}

		/// <summary>
		/// Raises the volume one time
		/// Amount of the change varies between implementations - typically "1" raw unit
		/// </summary>
		public override void VolumeIncrement()
		{
			this.SetVolumePercent(this.GetVolumePercent() + 0.01f);
		}

		/// <summary>
		/// Lowers the volume one time
		/// Amount of the change varies between implementations - typically "1" raw unit
		/// </summary>
		public override void VolumeDecrement()
		{
			this.SetVolumePercent(this.GetVolumePercent() - 0.01f);
		}

		/// <summary>
		/// Starts ramping the volume, and continues until stop is called or the timeout is reached.
		/// If already ramping the current timeout is updated to the new timeout duration.
		/// </summary>
		/// <param name="increment">Increments the volume if true, otherwise decrements.</param>
		/// <param name="timeout"></param>
		public override void VolumeRamp(bool increment, long timeout)
		{
			throw new NotSupportedException();
		}

		/// <summary>
		/// Stops any current ramp up/down in progress.
		/// </summary>
		public override void VolumeRampStop()
		{
			throw new NotSupportedException();
		}

		/// <summary>
		/// Toggles the current mute state.
		/// </summary>
		public override void ToggleIsMuted()
		{
			Parent.VolumeMuteToggle();
		}

		/// <summary>
		/// Sets the mute state.
		/// </summary>
		/// <param name="mute"></param>
		public override void SetIsMuted(bool mute)
		{
			Parent.SetVolumeMuteState(mute);
		}

		#endregion

		#region Internal Methods

		/// <summary>
		/// Sets the volume feedback, from the shim
		/// </summary>
		/// <param name="level"></param>
		internal void SetVolumeFeedback(ushort level)
		{
			VolumeLevel = level;
		}

		/// <summary>
		/// Sets the mute state feedback, called from the shim through the device
		/// </summary>
		/// <param name="isMuted"></param>
		internal void SetVolumeIsMutedFeedback(bool isMuted)
		{
			IsMuted = isMuted;
		}

		#endregion
	}
}