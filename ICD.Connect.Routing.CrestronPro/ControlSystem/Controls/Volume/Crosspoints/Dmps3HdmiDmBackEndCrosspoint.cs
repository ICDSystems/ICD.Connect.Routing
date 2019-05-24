using System;
#if SIMPLSHARP
using Crestron.SimplSharp.Reflection;
using Crestron.SimplSharpPro;
using Crestron.SimplSharpPro.DM;
using Crestron.SimplSharpPro.DM.Cards;
using ICD.Connect.Misc.CrestronPro.Extensions;
#endif

namespace ICD.Connect.Routing.CrestronPro.ControlSystem.Controls.Volume.Crosspoints
{
	public sealed class Dmps3HdmiDmBackEndCrosspoint : AbstractDmps3Crosspoint
	{
#if SIMPLSHARP
		private CrestronControlSystem.Dmps3AttachableOutputMixer AttachableVolumeOutputMixer
		{
			get { return ((IOutputMixer)VolumeObject).OutputMixer as CrestronControlSystem.Dmps3AttachableOutputMixer; }
		}

		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="parent"></param>
		/// <param name="output"></param>
		/// <param name="inputType"></param>
		/// <param name="inputAddress"></param>
		public Dmps3HdmiDmBackEndCrosspoint(ControlSystemDevice parent, Card.Dmps3OutputBase output,
		                                    eDmps3InputType inputType, uint inputAddress)
			: base(parent, output, inputType, inputAddress)
		{
		}
#endif

		/// <summary>
		/// Gets the minimum crosspoint volume level.
		/// </summary>
		public override short VolumeLevelMin
		{
			get
			{
#if SIMPLSHARP
				return AttachableVolumeOutputMixer.MinVolumeFeedback.GetShortValueOrDefault() != 0
					       ? AttachableVolumeOutputMixer.MinVolumeFeedback.GetShortValueOrDefault()
					       : (short)-800;
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
				return AttachableVolumeOutputMixer.MaxVolumeFeedback.GetShortValueOrDefault();
#else
				throw new NotSupportedException();
#endif
			}
		}

		#region Methods

		public override void SetMicrophoneMute(ushort microphone, bool mute)
		{
#if SIMPLSHARP
			if (!MicrophoneSupported(microphone))
				throw new NotSupportedException();

			if (mute)
				AttachableVolumeOutputMixer.MicMuteOn(microphone);
			else
				AttachableVolumeOutputMixer.MicMuteOff(microphone);
#else
			throw new NotSupportedException();
#endif
		}

		public override void SetMicrophoneLevel(ushort microphone, short gainLevel)
		{
#if SIMPLSHARP
			if (!MicrophoneSupported(microphone))
				throw new NotSupportedException();
			
			AttachableVolumeOutputMixer.MicLevel[microphone].ShortValue = gainLevel;
#else
			throw new NotSupportedException();
#endif
		}

		public override void SetMicMasterMute(bool mute)
		{
#if SIMPLSHARP
			if (mute)
				AttachableVolumeOutputMixer.MicMasterMuteOn();
			else
				AttachableVolumeOutputMixer.MicMasterMuteOff();
#else
			throw new NotSupportedException();
#endif
		}

		public override void SetMicMasterLevel(short gainLevel)
		{
#if SIMPLSHARP
			AttachableVolumeOutputMixer.MicMasterLevel.ShortValue = gainLevel;
#else
			throw new NotSupportedException();
#endif
		}

		public override void SetSourceLevel(short gainLevel)
		{
#if SIMPLSHARP
			AttachableVolumeOutputMixer.SourceLevel.ShortValue = gainLevel;
#else
			throw new NotSupportedException();
#endif
		}

		public override void SetSourceMute(bool mute)
		{
#if SIMPLSHARP
			if (mute)
				AttachableVolumeOutputMixer.SourceMuteOn();
			else
				AttachableVolumeOutputMixer.SourceMuteOff();
#else
			throw new NotSupportedException();
#endif
		}


		public override void SetMasterVolumeLevel(short gainLevel)
		{
#if SIMPLSHARP
			AttachableVolumeOutputMixer.MasterVolume.ShortValue = gainLevel;
#else
			throw new NotSupportedException();
#endif
		}

		public override void SetMasterVolumeMute(bool mute)
		{
#if SIMPLSHARP
			if (mute)
				AttachableVolumeOutputMixer.MasterMuteOn();
			else
				AttachableVolumeOutputMixer.MasterMuteOff();
#else
			throw new NotSupportedException();
#endif
		}

		#endregion

		#region Private Methods

		/// <summary>
		/// Returns true if the output mixer contains a microphone at the given address
		/// and the microphone is supported.
		/// </summary>
		/// <param name="microphone"></param>
		/// <returns></returns>
		private bool MicrophoneSupported(ushort microphone)
		{
#if SIMPLSHARP
			return AttachableVolumeOutputMixer.MicLevel.Contains(microphone) &&
			       AttachableVolumeOutputMixer.MicLevel[microphone].Supported;
#else
			return false;
#endif
		}

		#endregion

		#region ControlSystem Feedback

#if SIMPLSHARP
		/// <summary>
		/// Called when the control system raises a DM output change event.
		/// </summary>
		/// <param name="device"></param>
		/// <param name="args"></param>
		protected override void ControlSystemOnDmOutputChange(Switch device, DMOutputEventArgs args)
		{
			switch (InputType)
			{
				case eDmps3InputType.Master:
					VolumeLevel = AttachableVolumeOutputMixer.MasterVolumeFeedBack.GetShortValueOrDefault();
					VolumeIsMuted = AttachableVolumeOutputMixer.MasterMuteOnFeedBack.GetBoolValueOrDefault();
					break;

				case eDmps3InputType.MicrophoneMaster:
					VolumeLevel = AttachableVolumeOutputMixer.MicMasterLevelFeedBack.GetShortValueOrDefault();
					VolumeIsMuted = GetMicMasterMuteOnFeedBack(AttachableVolumeOutputMixer).GetBoolValueOrDefault();
					break;

				case eDmps3InputType.Source:
					VolumeLevel = AttachableVolumeOutputMixer.SourceLevelFeedBack.GetShortValueOrDefault();
					VolumeIsMuted = AttachableVolumeOutputMixer.SourceMuteOnFeedBack.GetBoolValueOrDefault();
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

			FieldInfo field = mixer.GetType().GetCType().GetField(fieldName, BindingFlags.Instance | BindingFlags.NonPublic);
			return field.GetValue(mixer) as BoolOutputSig;
		}
#endif

		#endregion
	}
}
