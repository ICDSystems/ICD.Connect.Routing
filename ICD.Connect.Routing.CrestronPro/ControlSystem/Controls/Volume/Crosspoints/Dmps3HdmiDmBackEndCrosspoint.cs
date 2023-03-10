using System;
using ICD.Common.Properties;
#if !NETSTANDARD
using Crestron.SimplSharp.Reflection;
using Crestron.SimplSharpPro;
using Crestron.SimplSharpPro.DM;
using Crestron.SimplSharpPro.DM.Cards;
using ICD.Connect.Misc.CrestronPro.Extensions;
#endif

namespace ICD.Connect.Routing.CrestronPro.ControlSystem.Controls.Volume.Crosspoints
{
	public sealed class Dmps3HdmiDmBackEndCrosspoint : AbstractDmps3OutputBaseCrosspoint
	{
#if !NETSTANDARD
		[CanBeNull]
		private CrestronControlSystem.Dmps3AttachableOutputMixer AttachableVolumeOutputMixer
		{
			get
			{
				return ((IOutputMixer)VolumeObject).OutputMixer as CrestronControlSystem.Dmps3AttachableOutputMixer;
			}
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
#if !NETSTANDARD
				if (AttachableVolumeOutputMixer == null)
					return 0;

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
#if !NETSTANDARD
				if (AttachableVolumeOutputMixer == null)
					return 0;

				return AttachableVolumeOutputMixer.MaxVolumeFeedback.GetShortValueOrDefault();
#else
				throw new NotSupportedException();
#endif
			}
		}

		#region Methods

		protected override void SetMicrophoneMute(ushort microphone, bool mute)
		{
#if !NETSTANDARD
			if (!MicrophoneSupported(microphone))
				throw new NotSupportedException();

			if (AttachableVolumeOutputMixer == null)
				return;

			if (mute)
				AttachableVolumeOutputMixer.MicMuteOn(microphone);
			else
				AttachableVolumeOutputMixer.MicMuteOff(microphone);
#else
			throw new NotSupportedException();
#endif
		}

		protected override void SetMicrophoneLevel(ushort microphone, short gainLevel)
		{
#if !NETSTANDARD
			if (!MicrophoneSupported(microphone))
				throw new NotSupportedException();

			if (AttachableVolumeOutputMixer == null)
				return;
			
			AttachableVolumeOutputMixer.MicLevel[microphone].ShortValue = gainLevel;
#else
			throw new NotSupportedException();
#endif
		}

		protected override void SetMicMasterMute(bool mute)
		{
#if !NETSTANDARD
			if (AttachableVolumeOutputMixer == null)
				return;

			if (mute)
				AttachableVolumeOutputMixer.MicMasterMuteOn();
			else
				AttachableVolumeOutputMixer.MicMasterMuteOff();
#else
			throw new NotSupportedException();
#endif
		}

		protected override void SetMicMasterLevel(short gainLevel)
		{
#if !NETSTANDARD

			if (AttachableVolumeOutputMixer == null)
				return;

			AttachableVolumeOutputMixer.MicMasterLevel.ShortValue = gainLevel;
#else
			throw new NotSupportedException();
#endif
		}

		protected override void SetSourceLevel(short gainLevel)
		{
#if !NETSTANDARD
			if (AttachableVolumeOutputMixer == null)
				return;

			AttachableVolumeOutputMixer.SourceLevel.ShortValue = gainLevel;
#else
			throw new NotSupportedException();
#endif
		}

		protected override void SetSourceMute(bool mute)
		{
#if !NETSTANDARD
			if (AttachableVolumeOutputMixer == null)
				return;

			if (mute)
				AttachableVolumeOutputMixer.SourceMuteOn();
			else
				AttachableVolumeOutputMixer.SourceMuteOff();
#else
			throw new NotSupportedException();
#endif
		}

		protected override void SetMasterVolumeLevel(short gainLevel)
		{
#if !NETSTANDARD
			if (AttachableVolumeOutputMixer == null)
				return;

			AttachableVolumeOutputMixer.MasterVolume.ShortValue = gainLevel;
#else
			throw new NotSupportedException();
#endif
		}

		protected override void SetMasterVolumeMute(bool mute)
		{
#if !NETSTANDARD
			if (AttachableVolumeOutputMixer == null)
				return;

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
		protected override bool MicrophoneSupported(ushort microphone)
		{
#if !NETSTANDARD
			if (AttachableVolumeOutputMixer == null)
				return false;

			return AttachableVolumeOutputMixer.MicLevel.Contains(microphone) &&
			       AttachableVolumeOutputMixer.MicLevel[microphone].Supported;
#else
			return false;
#endif
		}

		#endregion

		#region ControlSystem Feedback

#if !NETSTANDARD

		/// <summary>
		/// Updates the volume/mute state with the master volume values
		/// </summary>
		protected override void UpdateMasterVolume()
		{
			if (AttachableVolumeOutputMixer == null)
				return;

			VolumeLevel = AttachableVolumeOutputMixer.MasterVolumeFeedBack.GetShortValueOrDefault();
			VolumeIsMuted = AttachableVolumeOutputMixer.MasterMuteOnFeedBack.GetBoolValueOrDefault();
		}

		/// <summary>
		/// Updates the volume/mute states with the Mic Master volume values
		/// </summary>
		protected override void UpdateMicMasterVolume()
		{
			if (AttachableVolumeOutputMixer == null)
				return;

			VolumeLevel = AttachableVolumeOutputMixer.MicMasterLevelFeedBack.GetShortValueOrDefault();
			VolumeIsMuted = GetMicMasterMuteOnFeedBack(AttachableVolumeOutputMixer).GetBoolValueOrDefault();
		}

		/// <summary>
		/// Updates the volume/mute states with the source volume values
		/// </summary>
		protected override void UpdateSourceVolume()
		{
			if (AttachableVolumeOutputMixer == null)
				return;
			
			VolumeLevel = AttachableVolumeOutputMixer.SourceLevelFeedBack.GetShortValueOrDefault();
			VolumeIsMuted = AttachableVolumeOutputMixer.SourceMuteOnFeedBack.GetBoolValueOrDefault();
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
