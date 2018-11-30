#if SIMPLSHARP
#endif
using System;
using Crestron.SimplSharpPro;
using Crestron.SimplSharpPro.DeviceSupport;
using Crestron.SimplSharpPro.DM;
using Crestron.SimplSharpPro.DM.Cards;
using ICD.Common.Properties;
using ICD.Common.Utils;
using ICD.Common.Utils.EventArguments;
using ICD.Common.Utils.Extensions;

namespace ICD.Connect.Routing.CrestronPro.ControlSystem.Controls.Volume.Crosspoints
{
	public abstract class AbstractDmps3Crosspoint : IDmps3Crosspoint
	{
		public event EventHandler<GenericEventArgs<short>> OnVolumeLevelChanged;
		public event EventHandler<BoolEventArgs> OnMuteStateChanged;

		private bool m_IsMuted;
		private readonly eDmps3InputType m_InputType;
		private readonly uint m_InputAddress;
#if SIMPLSHARP
		private readonly Card.Dmps3OutputBase m_VolumeObject;
		private readonly ControlSystemDevice m_Parent;
		private short m_VolumeLevel;

		[NotNull]
		protected Card.Dmps3OutputBase VolumeObject { get { return m_VolumeObject; } }

		[CanBeNull]
		protected CrestronControlSystem.Dmps3OutputMixer VolumeOutputMixer { get
		{
			return ((IOutputMixer) VolumeObject).OutputMixer as CrestronControlSystem.Dmps3OutputMixer;
		} }
#endif

#if SIMPLSHARP
		protected AbstractDmps3Crosspoint(ControlSystemDevice parent, Card.Dmps3OutputBase output, eDmps3InputType inputType, uint inputAddress)
		{
			if (parent == null)
				throw new ArgumentNullException("parent");

			if (output == null)
				throw new ArgumentNullException("output");

			m_Parent = parent;
			m_VolumeObject = output;
			m_InputType = inputType;
			m_InputAddress = inputAddress;

			Subscribe(parent);
		}
#endif

		public void Dispose()
		{
			OnVolumeLevelChanged = null;
			OnMuteStateChanged = null;

			Unsubscribe(m_Parent);
		}

	#region Properties

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

		public short VolumeLevel
		{
			get { return m_VolumeLevel; }
			protected set
			{
				if (value == m_VolumeLevel)
					return;

				m_VolumeLevel = value;

				OnVolumeLevelChanged.Raise(this, new GenericEventArgs<short>(m_VolumeLevel));
			}
		}

		public virtual short VolumeRawMinAbsolute
		{
			get
			{
#if SIMPLSHARP
				if (VolumeOutputMixer == null)
					throw new ArgumentNullException("VolumeOutputMixer");

				return VolumeOutputMixer.MinVolumeFeedback.ShortValue != 0
					? VolumeOutputMixer.MinVolumeFeedback.ShortValue
					: (short) -800;
#else
				throw new NotSupportedException();
#endif
			}
		}

		public virtual short VolumeRawMaxAbsolute
		{
			get
			{
#if SIMPLSHARP
				if (VolumeOutputMixer == null)
					throw new ArgumentNullException("VolumeOutputMixer");

				return VolumeOutputMixer.MaxVolumeFeedback.ShortValue;
#else
				throw new NotSupportedException();
#endif
			}
		}

		public eDmps3InputType InputType
		{
			get { return m_InputType; }
		}

		public uint InputAddress
		{
			get { return m_InputAddress; }
		}

		#endregion
			
	#region Methods

		public void VolumeMuteToggle()
		{
			SetVolumeMute(!VolumeIsMuted);
		}

		public virtual void SetMicrophoneMute(ushort microphone, bool mute)
		{
			if (VolumeOutputMixer == null)
				throw new ArgumentNullException("VolumeOutputMixer");

			if (!MicrophoneSupported(microphone))
				return;

#if SIMPLSHARP
			if (mute)
			{
				VolumeOutputMixer.MicMuteOn(microphone);
			}
			else
			{
				VolumeOutputMixer.MicMuteOff(microphone);
			}
#endif
		}

		public virtual void SetMicrophoneLevel(ushort microphone, short gainLevel)
		{
			if (VolumeOutputMixer == null)
				throw new ArgumentNullException("VolumeOutputMixer");

			if (!MicrophoneSupported(microphone))
				return;

#if SIMPLSHARP
			VolumeOutputMixer.MicLevel[microphone].ShortValue = gainLevel;
#endif
		}

		public void SetVolumeLevel(short volume)
		{
			switch (InputType)
			{
				case eDmps3InputType.Codec1:
					SetCodec1Level(volume);
					break;
				case eDmps3InputType.Codec2:
					SetCodec2Level(volume);
					break;
				case eDmps3InputType.Source:
					SetSourceLevel(volume);
					break;
				case eDmps3InputType.Master:
					SetMasterVolumeLevel(volume);
					break;
				case eDmps3InputType.Microphone:
					SetMicrophoneLevel((ushort) InputAddress, volume);
					break;
				case eDmps3InputType.MicrophoneMaster:
					SetMicMasterLevel(volume);
					break;

				default:
					string message = string.Format("{0} is not a valid Dmps3 input type", InputType);
					throw new InvalidOperationException(message);
			}
		}

		public virtual void SetMicMasterMute(bool mute)
		{
#if SIMPLSHARP
			if (mute)
				VolumeObject.MicMasterMuteOn();
			else
				VolumeObject.MicMasterMuteOff();
#endif
		}

		public virtual void SetMicMasterLevel(short gainLevel)
		{
#if SIMPLSHARP
			VolumeObject.MicMasterLevel.ShortValue = gainLevel;
#endif
		}

		public virtual void SetSourceLevel(short gainLevel)
		{
#if SIMPLSHARP
			VolumeObject.SourceLevel.ShortValue = gainLevel;
#endif
		}

		public virtual void SetSourceMute(bool mute)
		{
#if SIMPLSHARP
			if (mute)
				VolumeObject.SourceMuteOn();
			else
				VolumeObject.SourceMuteOff();
#endif
		}

		public virtual void SetMasterVolumeLevel(short gainLevel)
		{
#if SIMPLSHARP
			VolumeObject.MasterVolume.ShortValue = gainLevel;
#endif
		}

		public virtual void SetMasterVolumeMute(bool mute)
		{
#if SIMPLSHARP
			if (mute)
				VolumeObject.MasterMuteOn();
			else
				VolumeObject.MasterMuteOff();
#endif
		}

		public void SetVolumeMute(bool mute)
		{
			switch (InputType)
			{
				case eDmps3InputType.Codec1:
					SetCodec1Mute(mute);
					break;
				case eDmps3InputType.Codec2:
					SetCodec2Mute(mute);
					break;
				case eDmps3InputType.Source:
					SetSourceMute(mute);
					break;
				case eDmps3InputType.Master:
#if SIMPLSHARP
					SetMasterVolumeMute(mute);
#endif
					break;
				case eDmps3InputType.Microphone:
					SetMicrophoneMute((ushort)InputAddress, mute);
					break;
				case eDmps3InputType.MicrophoneMaster:
					SetMicMasterMute(mute);
					break;

				default:
					string message = string.Format("{0} is not a valid Dmps3 input type", InputType);
					throw new InvalidOperationException(message);
			}
		}

		protected virtual void SetCodec2Mute(bool parse)
		{
			throw new NotSupportedException();
		}

		protected virtual void SetCodec2Level(short parse)
		{
			throw new NotSupportedException();
		}

		protected virtual void SetCodec1Mute(bool parse)
		{
			throw new NotSupportedException();
		}

		protected virtual void SetCodec1Level(short parse)
		{
			throw new NotSupportedException();
		}

#endregion

#region Private Methods

		private bool MicrophoneSupported(ushort microphone)
		{
#if SIMPLSHARP
			if (VolumeOutputMixer != null && (VolumeOutputMixer.MicLevel.Contains(microphone) && VolumeOutputMixer.MicLevel[microphone].Supported))
				return true;
#endif
			return false;
		}

		#endregion

#region Parent Callbacks

		private void Subscribe(ControlSystemDevice parent)
		{
#if SIMPLSHARP
			parent.ControlSystem.DMOutputChange += ControlSystemOnDmOutputChange;
#endif
		}

		private void Unsubscribe(ControlSystemDevice parent)
		{
#if SIMPLSHARP
			parent.ControlSystem.DMOutputChange -= ControlSystemOnDmOutputChange;
#endif
		}

#if SIMPLSHARP

		protected virtual void ControlSystemOnDmOutputChange(Switch device, DMOutputEventArgs args)
		{
			switch (InputType)
			{
				case eDmps3InputType.Master:
					VolumeLevel = VolumeObject.MasterVolumeFeedBack.ShortValue;
					VolumeIsMuted = VolumeObject.MasterMuteOnFeedBack.BoolValue;
					break;

				case eDmps3InputType.MicrophoneMaster:
					VolumeLevel = VolumeObject.MicMasterLevelFeedBack.ShortValue;
					VolumeIsMuted = VolumeObject.MicMasterMuteOnFeedBack.BoolValue;
					break;

				case eDmps3InputType.Source:
					VolumeLevel = VolumeObject.SourceLevelFeedBack.ShortValue;
					VolumeIsMuted = VolumeObject.SourceMuteOnFeedBack.BoolValue;
					break;
				default:
					IcdConsole.PrintLine(eConsoleColor.Magenta, "DMOutputEventArgs eventId={0}", args.EventId);
					break;
			}
		}
#endif
#endregion
		/*
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
		*/
	}
}
