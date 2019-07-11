using System;
using ICD.Common.Utils.Extensions;
using ICD.Connect.Audio.Controls.Mute;
using ICD.Connect.Audio.Controls.Volume;
using ICD.Connect.Audio.EventArguments;

namespace ICD.Connect.Routing.SPlus.SPlusDestinationDevice.Controls
{
	public sealed class SPlusDestinationVolumeControl : AbstractVolumeLevelDeviceControl<Device.SPlusDestinationDevice>, IVolumeMuteFeedbackDeviceControl
	{

		private ushort m_VolumeLevel;
		private bool m_VolumeIsMuted;

		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="parent">Device this control belongs to</param>
		/// <param name="id">Id of this control in the device</param>
		public SPlusDestinationVolumeControl(Device.SPlusDestinationDevice parent, int id) : base(parent, id)
		{
		}

		#region Volume

		/// <summary>
		/// Gets the current volume, in the parent device's format
		/// </summary>
		public override float VolumeLevel
		{
			get { return m_VolumeLevel; }
		}

		/// <summary>
		/// Absolute Minimum the raw volume can be
		/// Used as a last resort for position caculation
		/// </summary>
		protected override float VolumeRawMinAbsolute { get { return ushort.MinValue; } }

		/// <summary>
		/// Absolute Maximum the raw volume can be
		/// Used as a last resport for position caculation
		/// </summary>
		protected override float VolumeRawMaxAbsolute { get { return ushort.MaxValue; } }

		/// <summary>
		/// Sets the raw volume. This will be clamped to the min/max and safety min/max.
		/// </summary>
		/// <param name="volume"></param>
		public override void SetVolumeLevel(float volume)
		{
			Parent.SetVolumeLevel((ushort)volume);
		}

		/// <summary>
		/// Sets the volume feedback, from the shim
		/// </summary>
		/// <param name="volume"></param>
		internal void SetVolumeFeedback(ushort volume)
		{
			m_VolumeLevel = volume;
			VolumeFeedback(volume);
		}

		#endregion

		#region Mute

		/// <summary>
		/// Toggles the current mute state.
		/// </summary>
		public void VolumeMuteToggle()
		{
			Parent.VolumeMuteToggle();
		}

		/// <summary>
		/// Sets the mute state.
		/// </summary>
		/// <param name="mute"></param>
		public void SetVolumeMute(bool mute)
		{
			Parent.SetVolumeMuteState(mute);
		}

		public event EventHandler<MuteDeviceMuteStateChangedApiEventArgs> OnMuteStateChanged;

		/// <summary>
		/// Gets the muted state.
		/// </summary>
		public bool VolumeIsMuted
		{
			get { return m_VolumeIsMuted; }
			private set
			{
				if (m_VolumeIsMuted == value)
					return;
				m_VolumeIsMuted = value;

				OnMuteStateChanged.Raise(this, new MuteDeviceMuteStateChangedApiEventArgs(value));
			}
		}

		/// <summary>
		/// Sets the mute state feedback, called from the shim through the device
		/// </summary>
		/// <param name="state"></param>
		internal void SetVolumeMuteStateFeedback(bool state)
		{
			VolumeIsMuted = state;
		}

		#endregion
	}
}