using ICD.Connect.Audio.Console.Mute;
using ICD.Connect.Audio.Controls.Mute;
using ICD.Connect.Audio.Controls.Volume;
using ICD.Connect.Misc.CrestronPro.Extensions;
using ICD.Connect.Routing.CrestronPro.DigitalMedia.DmNvx.Dm100xStrBase;
#if SIMPLSHARP
using System;
using System.Collections.Generic;
using Crestron.SimplSharpPro;
using Crestron.SimplSharpPro.DM;
using Crestron.SimplSharpPro.DM.Streaming;
using ICD.Common.Utils.EventArguments;
using ICD.Common.Utils.Extensions;
using ICD.Connect.API.Commands;
using ICD.Connect.API.Nodes;

namespace ICD.Connect.Routing.CrestronPro.DigitalMedia.DmNvx.DmNvxBaseClass
{
	public sealed class DmNvxBaseClassVolumeControl : AbstractVolumeLevelDeviceControl<IDmNvxBaseClassAdapter>, IVolumeMuteFeedbackDeviceControl
	{
		private Crestron.SimplSharpPro.DM.Streaming.DmNvxBaseClass m_Streamer;
		private DmNvxControl m_NvxControl;

		/// <summary>
		/// Raised when the mute state changes.
		/// </summary>
		public event EventHandler<BoolEventArgs> OnMuteStateChanged;

#region Properties

		/// <summary>
		/// Gets the current volume, in the parent device's format
		/// </summary>
		public override float VolumeLevel
		{
			get
			{
				return m_NvxControl == null
					       ? 0.0f
					       : m_NvxControl.AnalogAudioOutputVolumeFeedback.GetShortValueOrDefault() / 10.0f;
			}
		}

		/// <summary>
		/// Absolute Minimum the raw volume can be
		/// Used as a last resort for position caculation
		/// </summary>
		protected override float VolumeRawMinAbsolute { get { return -80.0f; } }

		/// <summary>
		/// Absolute Maximum the raw volume can be
		/// Used as a last resport for position caculation
		/// </summary>
		protected override float VolumeRawMaxAbsolute { get { return 24.0f; } }

		/// <summary>
		/// Gets the muted state.
		/// </summary>
		public bool VolumeIsMuted { get { return m_NvxControl != null && m_NvxControl.AudioMutedFeedback.GetBoolValueOrDefault(); } }

#endregion

		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="parent">Device this control belongs to</param>
		/// <param name="id">Id of this control in the device</param>
		public DmNvxBaseClassVolumeControl(IDmNvxBaseClassAdapter parent, int id)
			: base(parent, id)
		{
			parent.OnStreamerChanged += ParentOnStreamerChanged;

			SetStreamer(parent.Streamer as Crestron.SimplSharpPro.DM.Streaming.DmNvxBaseClass);
		}

		/// <summary>
		/// Override to release resources.
		/// </summary>
		/// <param name="disposing"></param>
		protected override void DisposeFinal(bool disposing)
		{
			OnMuteStateChanged = null;

			base.DisposeFinal(disposing);

			Parent.OnStreamerChanged -= ParentOnStreamerChanged;

			SetStreamer(null);
		}

#region Methods

		/// <summary>
		/// Sets the raw volume. This will be clamped to the min/max and safety min/max.
		/// </summary>
		/// <param name="volume"></param>
		public override void SetVolumeLevel(float volume)
		{
			if (m_NvxControl == null)
				throw new InvalidOperationException("Wrapped control is null");

			m_NvxControl.AnalogAudioOutputVolume.ShortValue = (short)(volume * 10.0f);
		}

		/// <summary>
		/// Toggles the current mute state.
		/// </summary>
		public void VolumeMuteToggle()
		{
			bool mute = !VolumeIsMuted;
			SetVolumeMute(mute);
		}

		/// <summary>
		/// Sets the mute state.
		/// </summary>
		/// <param name="mute"></param>
		public void SetVolumeMute(bool mute)
		{
			if (m_NvxControl == null)
				throw new InvalidOperationException("Wrapped control is null");

			if (mute)
				m_NvxControl.AudioMute();
			else
				m_NvxControl.AudioUnmute();
		}

#endregion

#region Streamer Callbacks

		/// <summary>
		/// Called when the parent wrapped streamer instance changes.
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="streamer"></param>
		private void ParentOnStreamerChanged(IDm100XStrBaseAdapter sender, Crestron.SimplSharpPro.DM.Streaming.Dm100xStrBase streamer)
		{
			SetStreamer(streamer as Crestron.SimplSharpPro.DM.Streaming.DmNvxBaseClass);
		}

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
					VolumeFeedback(VolumeLevel);
					break;

				case DMInputEventIds.AudioMuteEventId:
					OnMuteStateChanged.Raise(this, new BoolEventArgs(VolumeIsMuted));
					break;
			}
		}

#endregion

#region Console

		/// <summary>
		/// Calls the delegate for each console status item.
		/// </summary>
		/// <param name="addRow"></param>
		public override void BuildConsoleStatus(AddStatusRowDelegate addRow)
		{
			base.BuildConsoleStatus(addRow);

			VolumeMuteFeedbackDeviceControlConsole.BuildConsoleStatus(this, addRow);
			VolumeMuteDeviceControlConsole.BuildConsoleStatus(this, addRow);
			VolumeMuteBasicDeviceControlConsole.BuildConsoleStatus(this, addRow);
		}

		/// <summary>
		/// Gets the child console commands.
		/// </summary>
		/// <returns></returns>
		public override IEnumerable<IConsoleCommand> GetConsoleCommands()
		{
			foreach (IConsoleCommand command in GetBaseConsoleCommands())
				yield return command;

			foreach (IConsoleCommand command in VolumeMuteFeedbackDeviceControlConsole.GetConsoleCommands(this))
				yield return command;

			foreach (IConsoleCommand command in VolumeMuteDeviceControlConsole.GetConsoleCommands(this))
				yield return command;

			foreach (IConsoleCommand command in VolumeMuteBasicDeviceControlConsole.GetConsoleCommands(this))
				yield return command;
		}

		/// <summary>
		/// Workaround for "unverifiable code" warning.
		/// </summary>
		/// <returns></returns>
		private IEnumerable<IConsoleCommand> GetBaseConsoleCommands()
		{
			return base.GetConsoleCommands();
		}

		/// <summary>
		/// Gets the child console nodes.
		/// </summary>
		/// <returns></returns>
		public override IEnumerable<IConsoleNodeBase> GetConsoleNodes()
		{
			foreach (IConsoleNodeBase command in GetBaseConsoleNodes())
				yield return command;

			foreach (IConsoleNodeBase command in VolumeMuteFeedbackDeviceControlConsole.GetConsoleNodes(this))
				yield return command;

			foreach (IConsoleNodeBase command in VolumeMuteDeviceControlConsole.GetConsoleNodes(this))
				yield return command;

			foreach (IConsoleNodeBase command in VolumeMuteBasicDeviceControlConsole.GetConsoleNodes(this))
				yield return command;
		}

		/// <summary>
		/// Workaround for "unverifiable code" warning.
		/// </summary>
		/// <returns></returns>
		private IEnumerable<IConsoleNodeBase> GetBaseConsoleNodes()
		{
			return base.GetConsoleNodes();
		}

#endregion
	}
}
#endif
