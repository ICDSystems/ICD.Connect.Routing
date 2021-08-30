using System;
using ICD.Common.Utils.EventArguments;
using ICD.Common.Utils.Extensions;
#if !NETSTANDARD
using Crestron.SimplSharpPro.DM;
#endif

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

#if !NETSTANDARD
		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="parent"></param>
		/// <param name="inputType"></param>
		/// <param name="inputAddress"></param>
		protected AbstractDmps3Crosspoint(ControlSystemDevice parent, eDmps3InputType inputType,
		                                  uint inputAddress)
		{
			if (parent == null)
				throw new ArgumentNullException("parent");

			m_Parent = parent;
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
		/// Gets the maximum crosspoint volume level.
		/// </summary>
		public abstract short VolumeLevelMax { get; }

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
		public abstract short VolumeLevelMin { get; }

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

		public void UpdateState()
		{
			switch (InputType)
			{
				case eDmps3InputType.Master:
					UpdateMasterVolume();
					break;

				case eDmps3InputType.Microphone:
					UpdateMicVolume(InputAddress);
					break;

				case eDmps3InputType.MicrophoneMaster:
					UpdateMicMasterVolume();
					break;

				case eDmps3InputType.Source:
					UpdateSourceVolume();
					break;

				case eDmps3InputType.Codec1:
					UpdateCodec1Volume();
					break;
				case eDmps3InputType.Codec2:
					UpdateCodec2Volume();
					break;
			}
		}

		#endregion

		#region Protected Methods

		protected abstract bool MicrophoneSupported(ushort microphone);

		protected virtual void SetMicMasterLevel(short volume)
		{
			throw new NotSupportedException();
		}

		protected virtual void SetMasterVolumeLevel(short volume)
		{
			throw new NotSupportedException();
		}

		protected virtual void SetSourceLevel(short volume)
		{
			throw new NotSupportedException();
		}

		protected virtual void SetMicrophoneLevel(ushort inputAddress, short volume)
		{
			throw new NotSupportedException();
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

		protected virtual void SetMicMasterMute(bool mute)
		{
			throw new NotSupportedException();
		}

		protected virtual void SetMicrophoneMute(ushort inputAddress, bool mute)
		{
			throw new NotSupportedException();
		}

		protected virtual void SetMasterVolumeMute(bool mute)
		{
			throw new NotSupportedException();
		}

		protected virtual void SetSourceMute(bool mute)
		{
			throw new NotSupportedException();
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

		/// <summary>
		/// Updates the volume/mute state with the master volume values
		/// </summary>
		protected virtual void UpdateMasterVolume()
		{
			throw new NotSupportedException();
		}

		/// <summary>
		/// Updates the volume/mute states with the source volume values
		/// </summary>
		protected virtual void UpdateSourceVolume()
		{
			throw new NotSupportedException();
		}

		/// <summary>
		/// Updates the volume/mute states with the Codec 1 volume values
		/// </summary>
		protected virtual void UpdateCodec1Volume()
		{
			throw new NotSupportedException();
		}

		/// <summary>
		/// Updates the volume/mute states with the Codec 2 volume values
		/// </summary>
		protected virtual void UpdateCodec2Volume()
		{
			throw new NotSupportedException();
		}

		/// <summary>
		/// Updates the volume/mute states with the Mic Master volume values
		/// </summary>
		protected virtual void UpdateMicMasterVolume()
		{
			throw new NotSupportedException();
		}

		/// <summary>
		/// Updates the volume/mute states with the values for the given microphone
		/// </summary>
		/// <param name="microphone"></param>
		protected virtual void UpdateMicVolume(uint microphone)
		{
			throw new NotSupportedException();
		}

		#endregion

		#region Parent Callbacks

		/// <summary>
		/// Subscribe to the parent events.
		/// </summary>
		/// <param name="parent"></param>
		private void Subscribe(ControlSystemDevice parent)
		{
#if !NETSTANDARD
			parent.ControlSystem.DMOutputChange += ControlSystemOnDmOutputChange;
#endif
		}

		/// <summary>
		/// Unsubscribe from the parent events.
		/// </summary>
		/// <param name="parent"></param>
		private void Unsubscribe(ControlSystemDevice parent)
		{
#if !NETSTANDARD
			parent.ControlSystem.DMOutputChange -= ControlSystemOnDmOutputChange;
#endif
		}

#if !NETSTANDARD

		/// <summary>
		/// Called when the control system raises a DM output change event.
		/// </summary>
		/// <param name="device"></param>
		/// <param name="args"></param>
		private void ControlSystemOnDmOutputChange(Switch device, DMOutputEventArgs args)
		{
			UpdateState();
		}
#endif

		#endregion
	}
}