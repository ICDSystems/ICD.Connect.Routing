using ICD.Connect.Audio.Controls.Volume;
#if SIMPLSHARP
using System;
using Crestron.SimplSharpPro;
using Crestron.SimplSharpPro.DM;
using Crestron.SimplSharpPro.DM.Streaming;
using ICD.Connect.Misc.CrestronPro.Extensions;
using ICD.Connect.Routing.CrestronPro.DigitalMedia.DmNvx.Dm100xStrBase;

namespace ICD.Connect.Routing.CrestronPro.DigitalMedia.DmNvx.DmNvxBaseClass
{
	public sealed class DmNvxBaseClassVolumeControl : AbstractVolumeDeviceControl<IDmNvxBaseClassAdapter>
	{
		private Crestron.SimplSharpPro.DM.Streaming.DmNvxBaseClass m_Streamer;
		private DmNvxControl m_NvxControl;

		#region Properties

		/// <summary>
		/// Returns the features that are supported by this volume control.
		/// </summary>
		public override eVolumeFeatures SupportedVolumeFeatures
		{
			get
			{
				return eVolumeFeatures.Mute |
					   eVolumeFeatures.MuteAssignment |
					   eVolumeFeatures.MuteFeedback |
					   eVolumeFeatures.Volume |
					   eVolumeFeatures.VolumeAssignment |
					   eVolumeFeatures.VolumeFeedback;
			}
		}

		/// <summary>
		/// Gets the minimum supported volume level.
		/// </summary>
		public override float VolumeLevelMin { get { return -80.0f; } }

		/// <summary>
		/// Gets the maximum supported volume level.
		/// </summary>
		public override float VolumeLevelMax { get { return 24.0f; } }

		#endregion

		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="parent">Device this control belongs to</param>
		/// <param name="id">Id of this control in the device</param>
		public DmNvxBaseClassVolumeControl(IDmNvxBaseClassAdapter parent, int id)
			: base(parent, id)
		{
			SetStreamer(parent.Streamer as Crestron.SimplSharpPro.DM.Streaming.DmNvxBaseClass);
		}

		/// <summary>
		/// Override to release resources.
		/// </summary>
		/// <param name="disposing"></param>
		protected override void DisposeFinal(bool disposing)
		{
			base.DisposeFinal(disposing);

			SetStreamer(null);
		}

		#region Methods

		/// <summary>
		/// Sets the mute state.
		/// </summary>
		/// <param name="mute"></param>
		public override void SetIsMuted(bool mute)
		{
			if (m_NvxControl == null)
				throw new NotSupportedException("Wrapped control is null");

			if (mute)
				m_NvxControl.AudioMute();
			else
				m_NvxControl.AudioUnmute();
		}

		/// <summary>
		/// Toggles the current mute state.
		/// </summary>
		public override void ToggleIsMuted()
		{
			SetIsMuted(!IsMuted);
		}

		/// <summary>
		/// Sets the raw volume. This will be clamped to the min/max and safety min/max.
		/// </summary>
		/// <param name="level"></param>
		public override void SetVolumeLevel(float level)
		{
			if (m_NvxControl == null)
				throw new InvalidOperationException("Wrapped control is null");

			m_NvxControl.AnalogAudioOutputVolume.ShortValue = (short)(level * 10.0f);
		}

		/// <summary>
		/// Raises the volume one time
		/// Amount of the change varies between implementations - typically "1" raw unit
		/// </summary>
		public override void VolumeIncrement()
		{
			SetVolumeLevel(VolumeLevel + 1);
		}

		/// <summary>
		/// Lowers the volume one time
		/// Amount of the change varies between implementations - typically "1" raw unit
		/// </summary>
		public override void VolumeDecrement()
		{
			SetVolumeLevel(VolumeLevel - 1);
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

		#endregion

		#region Parent Callbacks

        protected override void Subscribe(IDmNvxBaseClassAdapter parent)
		{
			base.Subscribe(parent);

			parent.OnStreamerChanged += ParentOnStreamerChanged;
		}

		protected override void Unsubscribe(IDmNvxBaseClassAdapter parent)
		{
			base.Unsubscribe(parent);

			Parent.OnStreamerChanged -= ParentOnStreamerChanged;
		}

		/// <summary>
		/// Called when the parent wrapped streamer instance changes.
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="streamer"></param>
		private void ParentOnStreamerChanged(IDm100XStrBaseAdapter sender,
		                                     Crestron.SimplSharpPro.DM.Streaming.Dm100xStrBase streamer)
		{
			SetStreamer(streamer as Crestron.SimplSharpPro.DM.Streaming.DmNvxBaseClass);
		}

		#endregion

		#region Streamer Callbacks

		/// <summary>
		/// Sets the wrapped streamer instance.
		/// </summary>
		/// <param name="streamer"></param>
		private void SetStreamer(Crestron.SimplSharpPro.DM.Streaming.DmNvxBaseClass streamer)
		{
			if (streamer == m_Streamer)
				return;

			Unsubscribe(m_Streamer);

			m_Streamer = streamer;
			m_NvxControl = m_Streamer == null ? null : m_Streamer.Control;

			Subscribe(m_Streamer);
		}

		/// <summary>
		/// Subscribe to the streamer events.
		/// </summary>
		/// <param name="streamer"></param>
		private void Subscribe(Crestron.SimplSharpPro.DM.Streaming.DmNvxBaseClass streamer)
		{
			if (streamer == null)
				return;

			streamer.BaseEvent += StreamerOnBaseEvent;
		}

		/// <summary>
		/// Unsubscribe from the streamer events.
		/// </summary>
		/// <param name="streamer"></param>
		private void Unsubscribe(Crestron.SimplSharpPro.DM.Streaming.DmNvxBaseClass streamer)
		{
			if (streamer == null)
				return;

			streamer.BaseEvent -= StreamerOnBaseEvent;
		}

		/// <summary>
		/// Called when the streamer raises a base event.
		/// </summary>
		/// <param name="device"></param>
		/// <param name="args"></param>
		private void StreamerOnBaseEvent(GenericBase device, BaseEventArgs args)
		{
			switch (args.EventId)
			{
				case DMInputEventIds.VolumeEventId:
					VolumeLevel =
						m_NvxControl == null
							? 0.0f
							: m_NvxControl.AnalogAudioOutputVolumeFeedback.GetShortValueOrDefault() / 10.0f;
					break;

				case DMInputEventIds.AudioMuteEventId:
					IsMuted = m_NvxControl != null && m_NvxControl.AudioMutedFeedback.GetBoolValueOrDefault();
					break;
			}
		}

		#endregion
	}
}

#endif
