using System;
#if SIMPLSHARP
using Crestron.SimplSharpPro;
using Crestron.SimplSharpPro.DM;
using Crestron.SimplSharpPro.DM.Cards;
#endif
using ICD.Common.Properties;
using ICD.Common.Utils.EventArguments;
using ICD.Common.Utils.Extensions;

namespace ICD.Connect.Routing.CrestronPro.ControlSystem.Controls.Volume.Crosspoints
{
	public abstract class AbstractDmps3Crosspoint : IDmps3Crosspoint
	{
		/// <summary>
		/// Raised when the crosspoint volume level changes.
		/// </summary>
		public event EventHandler<GenericEventArgs<short>> OnVolumeLevelChanged;

		/// <summary>
		/// Raised when the crosspoint mute state changes.
		/// </summary>
		public event EventHandler<BoolEventArgs> OnMuteStateChanged;

		private readonly ControlSystemDevice m_Parent;
		private readonly eDmps3InputType m_InputType;
		private readonly uint m_InputAddress;

		private bool m_IsMuted;
		private short m_VolumeLevel;

#if SIMPLSHARP
		private readonly Card.Dmps3OutputBase m_VolumeObject;

		[NotNull]
		protected Card.Dmps3OutputBase VolumeObject { get { return m_VolumeObject; } }

		[CanBeNull]
		protected CrestronControlSystem.Dmps3OutputMixer VolumeOutputMixer
		{
			get { return ((IOutputMixer)VolumeObject).OutputMixer as CrestronControlSystem.Dmps3OutputMixer; }
		}
#endif

#if SIMPLSHARP
		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="parent"></param>
		/// <param name="output"></param>
		/// <param name="inputType"></param>
		/// <param name="inputAddress"></param>
		protected AbstractDmps3Crosspoint(ControlSystemDevice parent, Card.Dmps3OutputBase output, eDmps3InputType inputType,
		                                  uint inputAddress)
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

		/// <summary>
		/// Release resources.
		/// </summary>
		public void Dispose()
		{
			OnVolumeLevelChanged = null;
			OnMuteStateChanged = null;

			Unsubscribe(m_Parent);
		}

		#region Properties

		/// <summary>
		/// Gets the crosspoint mute state.
		/// </summary>
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

		/// <summary>
		/// Gets the crosspoint volume level.
		/// </summary>
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

		/// <summary>
		/// Gets the minimum crosspoint volume level.
		/// </summary>
		public virtual short VolumeLevelMin
		{
			get
			{
#if SIMPLSHARP
				if (VolumeOutputMixer == null)
					throw new ArgumentNullException("VolumeOutputMixer");

				return VolumeOutputMixer.MinVolumeFeedback.ShortValue != 0
					       ? VolumeOutputMixer.MinVolumeFeedback.ShortValue
					       : (short)-800;
#else
				throw new NotSupportedException();
#endif
			}
		}

		/// <summary>
		/// Gets the maximum crosspoint volume level.
		/// </summary>
		public virtual short VolumeLevelMax
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

		public eDmps3InputType InputType { get { return m_InputType; } }

		public uint InputAddress { get { return m_InputAddress; } }

		#endregion

		#region Methods

		/// <summary>
		/// Toggles the crosspoint mute state.
		/// </summary>
		public void VolumeMuteToggle()
		{
			SetVolumeMute(!VolumeIsMuted);
		}

		public virtual void SetMicrophoneMute(ushort microphone, bool mute)
		{
#if SIMPLSHARP
			if (VolumeOutputMixer == null)
				throw new ArgumentNullException("VolumeOutputMixer");

			if (!MicrophoneSupported(microphone))
				throw new NotSupportedException(string.Format("Microphone {0} is not supported", microphone));

			if (mute)
				VolumeOutputMixer.MicMuteOn(microphone);
			else
				VolumeOutputMixer.MicMuteOff(microphone);
#else
			throw new NotSupportedException();
#endif
		}

		public virtual void SetMicrophoneLevel(ushort microphone, short gainLevel)
		{
#if SIMPLSHARP
			if (VolumeOutputMixer == null)
				throw new ArgumentNullException("VolumeOutputMixer");

			if (!MicrophoneSupported(microphone))
				throw new NotSupportedException(string.Format("Microphone {0} is not supported", microphone));

			VolumeOutputMixer.MicLevel[microphone].ShortValue = gainLevel;
#else
			throw new NotSupportedException();
#endif
		}

		/// <summary>
		/// Sets the crosspoint volume level.
		/// </summary>
		/// <param name="volume"></param>
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
					SetMicrophoneLevel((ushort)InputAddress, volume);
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
#else
			throw new NotSupportedException();
#endif
		}

		public virtual void SetMicMasterLevel(short gainLevel)
		{
#if SIMPLSHARP
			VolumeObject.MicMasterLevel.ShortValue = gainLevel;
#else
			throw new NotSupportedException();
#endif
		}

		public virtual void SetSourceLevel(short gainLevel)
		{
#if SIMPLSHARP
			VolumeObject.SourceLevel.ShortValue = gainLevel;
#else
			throw new NotSupportedException();
#endif
		}

		public virtual void SetSourceMute(bool mute)
		{
#if SIMPLSHARP
			if (mute)
				VolumeObject.SourceMuteOn();
			else
				VolumeObject.SourceMuteOff();
#else
			throw new NotSupportedException();
#endif
		}

		public virtual void SetMasterVolumeLevel(short gainLevel)
		{
#if SIMPLSHARP
			VolumeObject.MasterVolume.ShortValue = gainLevel;
#else
			throw new NotSupportedException();
#endif
		}

		public virtual void SetMasterVolumeMute(bool mute)
		{
#if SIMPLSHARP
			if (mute)
				VolumeObject.MasterMuteOn();
			else
				VolumeObject.MasterMuteOff();
#else
			throw new NotSupportedException();
#endif
		}

		/// <summary>
		/// Sets the crosspoint mute state.
		/// </summary>
		/// <param name="mute"></param>
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
					SetMasterVolumeMute(mute);
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

		/// <summary>
		/// Returns true if there is a microphone at the given address and the microphone is supported.
		/// </summary>
		/// <param name="microphone"></param>
		/// <returns></returns>
		private bool MicrophoneSupported(ushort microphone)
		{
#if SIMPLSHARP
			return VolumeOutputMixer != null &&
			       VolumeOutputMixer.MicLevel.Contains(microphone) &&
				   VolumeOutputMixer.MicLevel[microphone].Supported;
#else
			return false;
#endif
		}

		#endregion

		#region Parent Callbacks

		/// <summary>
		/// Subscribe to the parent events.
		/// </summary>
		/// <param name="parent"></param>
		private void Subscribe(ControlSystemDevice parent)
		{
#if SIMPLSHARP
			parent.ControlSystem.DMOutputChange += ControlSystemOnDmOutputChange;
#endif
		}

		/// <summary>
		/// Unsubscribe from the parent events.
		/// </summary>
		/// <param name="parent"></param>
		private void Unsubscribe(ControlSystemDevice parent)
		{
#if SIMPLSHARP
			parent.ControlSystem.DMOutputChange -= ControlSystemOnDmOutputChange;
#endif
		}

#if SIMPLSHARP

		/// <summary>
		/// Called when the control system raises a DM output change event.
		/// </summary>
		/// <param name="device"></param>
		/// <param name="args"></param>
		protected virtual void ControlSystemOnDmOutputChange(Switch device, DMOutputEventArgs args)
		{
			switch (InputType)
			{
				case eDmps3InputType.Master:
					VolumeLevel = VolumeObject.MasterVolumeFeedBack.ShortValue;
					VolumeIsMuted = VolumeObject.MasterMuteOnFeedBack.BoolValue;
					break;

				case eDmps3InputType.Microphone:
					if (VolumeOutputMixer != null)
					{
						VolumeLevel = VolumeOutputMixer.MicLevel[InputAddress].ShortValue;
						VolumeIsMuted = VolumeOutputMixer.MicMuteOnFeedback[InputAddress].BoolValue;
					}
					break;

				case eDmps3InputType.MicrophoneMaster:
					VolumeLevel = VolumeObject.MicMasterLevelFeedBack.ShortValue;
					VolumeIsMuted = VolumeObject.MicMasterMuteOnFeedBack.BoolValue;
					break;

				case eDmps3InputType.Source:
					VolumeLevel = VolumeObject.SourceLevelFeedBack.ShortValue;
					VolumeIsMuted = VolumeObject.SourceMuteOnFeedBack.BoolValue;
					break;
			}
		}
#endif

		#endregion
	}
}
