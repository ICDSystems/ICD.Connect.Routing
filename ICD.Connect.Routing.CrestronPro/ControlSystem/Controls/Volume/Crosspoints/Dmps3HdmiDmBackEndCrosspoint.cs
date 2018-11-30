#if SIMPLSHARP
#endif
using System.Reflection;
using Crestron.SimplSharpPro;
using Crestron.SimplSharpPro.DM;
using Crestron.SimplSharpPro.DM.Cards;
using ICD.Common.Utils;

#if SIMPLSHARP
namespace ICD.Connect.Routing.CrestronPro.ControlSystem.Controls.Volume.Crosspoints
{
	public sealed class Dmps3HdmiDmBackEndCrosspoint : AbstractDmps3Crosspoint
	{
		private CrestronControlSystem.Dmps3AttachableOutputMixer m_AttachableVolumeOutputMixer;

		public Dmps3HdmiDmBackEndCrosspoint(ControlSystemDevice parent, Card.Dmps3OutputBase output,
			eDmps3InputType inputType, uint inputAddress)
			: base(parent, output, inputType, inputAddress)
		{
			m_AttachableVolumeOutputMixer =
				((IOutputMixer) VolumeObject).OutputMixer as CrestronControlSystem.Dmps3AttachableOutputMixer;
		}

		public override short VolumeRawMinAbsolute
		{
			get
			{
#if SIMPLSHARP
				return m_AttachableVolumeOutputMixer.MinVolumeFeedback.ShortValue != 0
					? m_AttachableVolumeOutputMixer.MinVolumeFeedback.ShortValue
					: (short)-800;
#else
				throw new NotSupportedException();
#endif
			}
		}

		public override short VolumeRawMaxAbsolute
		{
			get
			{
#if SIMPLSHARP
				return m_AttachableVolumeOutputMixer.MaxVolumeFeedback.ShortValue;
#else
				throw new NotSupportedException();
#endif
			}
		}

		#region Methods

		public override void SetMicrophoneMute(ushort microphone, bool mute)
		{
			if (!MicrophoneSupported(microphone)) return;

			if (mute)
				m_AttachableVolumeOutputMixer.MicMuteOn(microphone);
			else
				m_AttachableVolumeOutputMixer.MicMuteOff(microphone);
		}

		public override void SetMicrophoneLevel(ushort microphone, short gainLevel)
		{
			if (MicrophoneSupported(microphone))
				m_AttachableVolumeOutputMixer.MicLevel[microphone].ShortValue = gainLevel;
		}

		public override void SetMicMasterMute(bool mute)
		{
			if (mute)
				m_AttachableVolumeOutputMixer.MicMasterMuteOn();
			else
				m_AttachableVolumeOutputMixer.MicMasterMuteOff();
		}

		public override void SetMicMasterLevel(short gainLevel)
		{
			m_AttachableVolumeOutputMixer.MicMasterLevel.ShortValue = gainLevel;
		}

		public override void SetSourceLevel(short gainLevel)
		{
			m_AttachableVolumeOutputMixer.SourceLevel.ShortValue = gainLevel;
		}

		public override void SetSourceMute(bool mute)
		{
			if (mute)
				m_AttachableVolumeOutputMixer.SourceMuteOn();
			else
				m_AttachableVolumeOutputMixer.SourceMuteOff();
		}


		public override void SetMasterVolumeLevel(short gainLevel)
		{
#if SIMPLSHARP
			m_AttachableVolumeOutputMixer.MasterVolume.ShortValue = gainLevel;
#endif
		}

		public override void SetMasterVolumeMute(bool mute)
		{
#if SIMPLSHARP
			if (mute)
				m_AttachableVolumeOutputMixer.MasterMuteOn();
			else
				m_AttachableVolumeOutputMixer.MasterMuteOff();
#endif
		}

		#endregion

		#region Private Methods

		private bool MicrophoneSupported(ushort microphone)
		{

			if (m_AttachableVolumeOutputMixer.MicLevel.Contains(microphone) && m_AttachableVolumeOutputMixer.MicLevel[microphone].Supported)
				return true;

			return false;
		}

		#endregion

		protected override void ControlSystemOnDmOutputChange(Switch device, DMOutputEventArgs args)
		{
			switch (InputType)
			{
				case eDmps3InputType.Microphone:
					VolumeLevel = m_AttachableVolumeOutputMixer.MicLevel[InputAddress].ShortValue;
					VolumeIsMuted = m_AttachableVolumeOutputMixer.MicMuteOnFeedback[InputAddress].BoolValue;
					break;
				case eDmps3InputType.Master:
					VolumeLevel = m_AttachableVolumeOutputMixer.MasterVolumeFeedBack.ShortValue;
					VolumeIsMuted = m_AttachableVolumeOutputMixer.MasterMuteOnFeedBack.BoolValue;
					break;

				case eDmps3InputType.MicrophoneMaster:
					VolumeLevel = m_AttachableVolumeOutputMixer.MicMasterLevelFeedBack.ShortValue;
					VolumeIsMuted = GetMicMasterMuteOnFeedBack(m_AttachableVolumeOutputMixer).BoolValue;
					break;

				case eDmps3InputType.Source:
					VolumeLevel = m_AttachableVolumeOutputMixer.SourceLevelFeedBack.ShortValue;
					VolumeIsMuted = m_AttachableVolumeOutputMixer.SourceMuteOnFeedBack.BoolValue;
					break;

				default:
					IcdConsole.PrintLine(eConsoleColor.Magenta, "DMOutputEventArgs eventId={0}", args.EventId);
					break;
			}
		}

		/// <summary>
		/// Hack - Crestron neglected to expose the output sigs for mic master mute.
		/// </summary>
		/// <param name="mixer"></param>
		/// <returns></returns>
		private static BoolOutputSig GetMicMasterMuteOnFeedBack(CrestronControlSystem.Dmps3AttachableOutputMixer mixer)
		{
			// Warning - this could very easily change in future
			const string fieldName = "_MicMasterMuteOnFeedBack";

			FieldInfo field = mixer.GetType().GetField(fieldName, BindingFlags.Instance | BindingFlags.NonPublic);
			return field.GetValue(mixer) as BoolOutputSig;
		}
	}
}
#endif