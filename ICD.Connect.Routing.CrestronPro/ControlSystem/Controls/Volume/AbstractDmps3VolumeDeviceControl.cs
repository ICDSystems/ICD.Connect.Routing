using System;
using System.Collections.Generic;
using ICD.Common.Utils;
using ICD.Common.Utils.Services.Logging;
using ICD.Connect.API.Commands;
using ICD.Connect.Audio.Console;
#if SIMPLSHARP
using Crestron.SimplSharpPro;
using Crestron.SimplSharpPro.DM;
using Crestron.SimplSharpPro.DM.Cards;
#endif
using ICD.Common.Utils.EventArguments;
using ICD.Common.Utils.Extensions;
using ICD.Connect.Audio.Controls;
using ICD.Connect.Devices.EventArguments;

namespace ICD.Connect.Routing.CrestronPro.ControlSystem.Controls.Volume
{
	public abstract class AbstractDmps3VolumeDeviceControl : AbstractVolumeLevelDeviceControl<ControlSystemDevice>, IVolumeMuteFeedbackDeviceControl
	{
		public event EventHandler<BoolEventArgs> OnMuteStateChanged;

		private readonly string m_Name;

		private float m_VolumeLevel;
		private bool m_IsMuted;
#if SIMPLSHARP
		protected readonly Card.Dmps3OutputBase VolumeObject;
		protected readonly CrestronControlSystem.Dmps3OutputMixer VolumeOutputMixer;
#endif
		protected AbstractDmps3VolumeDeviceControl(ControlSystemDevice parent, int id, string name, uint outputAddress)
			: base(parent, id)
		{
			m_Name = name;

#if SIMPLSHARP
			VolumeObject = Parent.ControlSystem.SwitcherOutputs[outputAddress] as Card.Dmps3OutputBase;
			VolumeOutputMixer = ((IOutputMixer)VolumeObject).OutputMixer as CrestronControlSystem.Dmps3OutputMixer;
#endif

			Subscribe(parent);
		}

		protected override void DisposeFinal(bool disposing)
		{
			base.DisposeFinal(disposing);

			if (Parent != null)
				Unsubscribe(Parent);
		}

	#region Properties

		public override string Name
		{
			get { return string.IsNullOrEmpty(m_Name) ? base.Name : m_Name; }
		}

		public bool VolumeIsMuted
		{ 
			get { return m_IsMuted; }
			protected set
			{
				if (value == m_IsMuted)
					return;
				
				m_IsMuted = value; 

				OnMuteStateChanged.Raise(this, new BoolEventArgs(m_IsMuted));
			}
		}

		public override float VolumeLevel
		{
			get { return m_VolumeLevel; }
		}
#if SIMPLSHARP
		protected override float VolumeRawMinAbsolute
		{
			get
			{
				return VolumeOutputMixer.MinVolumeFeedback.ShortValue != 0 ?
					VolumeOutputMixer.MinVolumeFeedback.ShortValue / 10.0f :
					-80;
			}
		}

		protected override float VolumeRawMaxAbsolute
		{
			get { return VolumeOutputMixer.MaxVolumeFeedback.ShortValue / 10.0f; }
		}
#else 
		protected override float VolumeRawMinAbsolute
		{
			get { throw new NotImplementedException(); }
		}

		protected override float VolumeRawMaxAbsolute
		{
			get { throw new NotImplementedException(); }
		}
#endif
		protected virtual void OnlineStateChanged()
		{
		}

	#endregion
			
	#region Methods

		public void VolumeMuteToggle()
		{
			SetVolumeMute(!VolumeIsMuted);
		}

		/// <summary>
		/// Sets the VolumeRaw property and calls VolumeFeedback with that value.
		/// Only need this since C# won't let me add a setter to an abstract property
		/// </summary>
		/// <param name="value"></param>
		protected void SetVolumeRawProperty(float value)
		{
			m_VolumeLevel = value;
			VolumeFeedback(m_VolumeLevel);
		}


		public virtual void SetMicrophoneMute(ushort microphone, bool mute)
		{
			if (!MicrophoneSupported(microphone)) return;
#if SIMPLSHARP
			if (VolumeOutputMixer.MicMuteOnFeedback[microphone].BoolValue & !mute)
			{
				VolumeOutputMixer.MicMuteOff(microphone);
			}
			else if (VolumeOutputMixer.MicMuteOffFeedback[microphone].BoolValue & mute)
			{
				VolumeOutputMixer.MicMuteOn(microphone);
			}
#endif
		}

		public virtual void SetMicrophoneLevel(ushort microphone, short gainLevel)
		{
			if (!MicrophoneSupported(microphone)) return;
#if SIMPLSHARP
				VolumeOutputMixer.MicLevel[microphone].ShortValue = (short)(gainLevel * 10);
#endif
		}

		public override void SetVolumeLevel(float volume)
		{
#if SIMPLSHARP
			VolumeObject.MasterVolume.ShortValue = (short)(volume * 10);
#endif
		}

		public virtual void SetMicMasterMute(bool mute)
		{
#if SIMPLSHARP
			if (mute && VolumeObject.MicMasterMuteOffFeedBack.BoolValue)
			{
				VolumeObject.MicMasterMuteOn();
			}
			else if (!mute && VolumeObject.MicMasterMuteOnFeedBack.BoolValue)
			{
				VolumeObject.MicMasterMuteOff();
			}
#endif
		}

		public virtual void SetMicMasterLevel(short gainLevel)
		{
#if SIMPLSHARP
			VolumeObject.MicMasterLevel.ShortValue = (short)(gainLevel * 10);
#endif
		}

		public virtual void SetSourceLevel(short gainLevel)
		{
#if SIMPLSHARP
			VolumeObject.SourceLevel.ShortValue = (short)(gainLevel * 10);
#endif
		}

		public virtual void SetSourceMute(bool mute)
		{
#if SIMPLSHARP
			if (mute && VolumeObject.SourceMuteOffFeedBack.BoolValue)
			{
				VolumeObject.SourceMuteOn();
			}
			else if (!mute && VolumeObject.SourceMuteOnFeedBack.BoolValue)
			{
				VolumeObject.SourceMuteOff();
			}
#endif
		}

		public virtual void SetVolumeMute(bool mute)
		{
#if SIMPLSHARP
			if (mute && !VolumeIsMuted)
			{
				VolumeObject.MasterMuteOn();
			}
			else if (!mute && VolumeIsMuted)
			{
				VolumeObject.MasterMuteOff();
			}
#endif
		}

#endregion

#region Private Methods

		private bool MicrophoneSupported(ushort microphone)
		{
#if SIMPLSHARP
			if (VolumeOutputMixer.MicPan.Contains(microphone) && VolumeOutputMixer.MicPan[microphone].Supported)
				return true;
#endif
			Log(eSeverity.Warning, "Microphone {0} is not supported", microphone);
			return false;
		}

#endregion

#region Parent Callbacks

		private void Subscribe(ControlSystemDevice parent)
		{
			parent.OnIsOnlineStateChanged += ParentOnIsOnlineStateChanged;
#if SIMPLSHARP
			parent.ControlSystem.DMOutputChange += ControlSystemOnDmOutputChange;
#endif
		}

		private void Unsubscribe(ControlSystemDevice parent)
		{
			parent.OnIsOnlineStateChanged -= ParentOnIsOnlineStateChanged;
#if SIMPLSHARP
			parent.ControlSystem.DMOutputChange -= ControlSystemOnDmOutputChange;
#endif
		}

		private void ParentOnIsOnlineStateChanged(object sender, DeviceBaseOnlineStateApiEventArgs e)
		{
			if(e.Data)
				OnlineStateChanged();
#if SIMPLSHARP
			VolumeIsMuted = VolumeObject.MasterMuteOnFeedBack.BoolValue;
#endif
		}
#if SIMPLSHARP
		protected virtual void ControlSystemOnDmOutputChange(Switch device, DMOutputEventArgs args)
		{
			if (args.Number != VolumeObject.Number)
				return;

			switch (args.EventId)
			{
				default:
					IcdConsole.PrintLine(eConsoleColor.Magenta, "DMOutputEventArgs output={0} eventId={1}", args.Number, args.EventId);
					break;
			}
		}
#endif
#endregion

#region Console

		/// <summary>
		/// Gets the child console commands.
		/// </summary>
		/// <returns></returns>
		public override IEnumerable<IConsoleCommand> GetConsoleCommands()
		{
			foreach (IConsoleCommand command in GetBaseConsoleCommands())
				yield return command;

			foreach (IConsoleCommand command in VolumeMuteDeviceControlConsole.GetConsoleCommands(this))
				yield return command;

			yield return new GenericConsoleCommand<ushort, short>("SetMicrophoneLevel", "SetMicrophoneLevel <MICROPHONE> <SHORT>", (r, s) => SetMicrophoneLevel(r, s));
			yield return new GenericConsoleCommand<ushort, bool>("SetMicrophoneMute", "SetMicrophoneMute <MICROPHONE> <BOOL>", (r, s) => SetMicrophoneMute(r, s));

			yield return new GenericConsoleCommand<bool>("SetMicMasterMute", "SetMicMasterMute <BOOL>", r => SetMicMasterMute(r));
			yield return new GenericConsoleCommand<short>("SetMicMasterLevel", "SetMicMasterLevel <SHORT>", r => SetMicMasterLevel(r));

			yield return new GenericConsoleCommand<bool>("SetSourceMute", "SetSourceMute <BOOL>", r => SetSourceMute(r));
			yield return new GenericConsoleCommand<short>("SetSourceLevel", "SetSourceLevel <SHORT>", r => SetSourceLevel(r));
		}

		/// <summary>
		/// Workaround for "unverifiable code" warning.
		/// </summary>
		/// <returns></returns>
		private IEnumerable<IConsoleCommand> GetBaseConsoleCommands()
		{
			return base.GetConsoleCommands();
		}

#endregion

	}
}
