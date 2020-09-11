using System;
#if SIMPLSHARP
using Crestron.SimplSharpPro.DM.Cards;
#endif

namespace ICD.Connect.Routing.CrestronPro.ControlSystem.Controls.Volume.Crosspoints
{
	public sealed class Dmps3DmHdmiOutputStreamCrosspoint : AbstractDmps3Crosspoint
	{
#if SIMPLSHARP
		private readonly Card.Dmps3HdmiAudioOutput.Dmps3DmHdmiOutputStream m_Output;
#endif

		/// <summary>
		/// Gets the minimum crosspoint volume level.
		/// </summary>
		public override short VolumeLevelMin
		{
			get
			{
#if SIMPLSHARP
				return m_Output.OutputMixer.MinVolumeFeedback.ShortValue;
#else
				throw new NotSupportedException();
#endif
			}
		}

		/// <summary>
		/// Gets the maximum crosspoint volume level.
		/// </summary>
		public override short VolumeLevelMax
		{
			get
			{
#if SIMPLSHARP
		return m_Output.OutputMixer.MaxVolumeFeedback.ShortValue;
#else
				throw new NotSupportedException();
#endif
			}
		}

#if SIMPLSHARP
		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="parent"></param>
		/// <param name="output"></param>
		/// <param name="inputType"></param>
		/// <param name="inputAddress"></param>
		public Dmps3DmHdmiOutputStreamCrosspoint(ControlSystemDevice parent, Card.Dmps3HdmiAudioOutput.Dmps3DmHdmiOutputStream output, eDmps3InputType inputType, uint inputAddress)
			: base(parent, inputType, inputAddress)
		{
			m_Output = output;
		}

		protected override void SetMasterVolumeLevel(short volume)
		{
			m_Output.MasterVolume.ShortValue = volume;
		}

		protected override void SetSourceLevel(short volume)
		{
			m_Output.SourceLevel.ShortValue = volume;
		}

		protected override void SetMicrophoneLevel(ushort inputAddress, short volume)
		{
			if (!MicrophoneSupported(inputAddress))
				throw new InvalidOperationException(String.Format("No microphone with address {0}", inputAddress));

			m_Output.OutputMixer.MicLevel[inputAddress].ShortValue = volume;
		}

		protected override void SetMicrophoneMute(ushort inputAddress, bool mute)
		{
			if (!MicrophoneSupported(inputAddress))
				throw new InvalidOperationException(String.Format("No microphone with address {0}", inputAddress));

			if (mute)
				m_Output.OutputMixer.MicMuteOn(inputAddress);
			else
				m_Output.OutputMixer.MicMuteOff(inputAddress);
		}

		protected override void SetMasterVolumeMute(bool mute)
		{
			if (mute)
				m_Output.MasterMuteOn();
			else
				m_Output.MasterMuteOff();
		}

		protected override void SetSourceMute(bool mute)
		{
			if (mute)
				m_Output.SourceMuteOn();
			else
				m_Output.SourceMuteOff();
		}

		protected override bool MicrophoneSupported(ushort microphone)
		{
			return m_Output.OutputMixer != null && m_Output.OutputMixer.MicLevel.Contains(microphone) && m_Output.OutputMixer.MicLevel[microphone].Supported;
		}

		/// <summary>
		/// Updates the volume/mute state with the master volume values
		/// </summary>
		protected override void UpdateMasterVolume()
		{
			VolumeLevel = m_Output.MasterVolumeFeedBack.ShortValue;
			VolumeIsMuted = m_Output.MasterMuteOnFeedBack.BoolValue;
		}

		/// <summary>
		/// Updates the volume/mute states with the source volume values
		/// </summary>
		protected override void UpdateSourceVolume()
		{
			VolumeLevel = m_Output.SourceLevelFeedBack.ShortValue;
			VolumeIsMuted = m_Output.SourceMuteOnFeedBack.BoolValue;
		}

		/// <summary>
		/// Updates the volume/mute states with the values for the given microphone
		/// </summary>
		/// <param name="microphone"></param>
		protected override void UpdateMicVolume(uint microphone)
		{
			if (!MicrophoneSupported((ushort)microphone))
				throw new InvalidOperationException(String.Format("No microphone at address {0}", microphone));

			VolumeLevel = m_Output.OutputMixer.MicLevelFeedback[microphone].ShortValue;
			VolumeIsMuted = m_Output.OutputMixer.MicMuteOnFeedback[microphone].BoolValue;
		}
#endif
		protected override bool MicrophoneSupported(ushort microphone)
		{
#if SIMPLSHARP
			return m_Output.OutputMixer != null && m_Output.OutputMixer.MicLevel.Contains(microphone) && m_Output.OutputMixer.MicLevel[microphone].Supported;
#else
			return false;
#endif
		}

	}
}