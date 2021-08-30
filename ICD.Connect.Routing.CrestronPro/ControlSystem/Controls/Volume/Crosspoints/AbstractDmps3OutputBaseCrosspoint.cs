using System;
using ICD.Common.Properties;
using ICD.Connect.Misc.CrestronPro.Extensions;
#if !NETSTANDARD
using Crestron.SimplSharpPro;
using Crestron.SimplSharpPro.DM;
using Crestron.SimplSharpPro.DM.Cards;
#endif

namespace ICD.Connect.Routing.CrestronPro.ControlSystem.Controls.Volume.Crosspoints
{
	public abstract class AbstractDmps3OutputBaseCrosspoint : AbstractDmps3Crosspoint
	{

#if !NETSTANDARD
		private readonly Card.Dmps3OutputBase m_VolumeObject;

		[NotNull]
		protected Card.Dmps3OutputBase VolumeObject { get { return m_VolumeObject; } }

		[CanBeNull]
		protected CrestronControlSystem.Dmps3OutputMixer VolumeOutputMixer
		{
			get { return ((IOutputMixer)VolumeObject).OutputMixer as CrestronControlSystem.Dmps3OutputMixer; }
		}
#endif

#if !NETSTANDARD
		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="parent"></param>
		/// <param name="output"></param>
		/// <param name="inputType"></param>
		/// <param name="inputAddress"></param>
		protected AbstractDmps3OutputBaseCrosspoint(ControlSystemDevice parent, Card.Dmps3OutputBase output, eDmps3InputType inputType,
		                                  uint inputAddress) : base(parent, inputType, inputAddress)
		{

			if (output == null)
				throw new ArgumentNullException("output");

			m_VolumeObject = output;
		}
#endif

		#region Properties

		/// <summary>
		/// Gets the minimum crosspoint volume level.
		/// </summary>
		public override short VolumeLevelMin
		{
			get
			{
#if !NETSTANDARD
				if (VolumeOutputMixer == null)
					throw new ArgumentNullException("VolumeOutputMixer");

				return VolumeOutputMixer.MinVolumeFeedback.GetShortValueOrDefault() != 0
					       ? VolumeOutputMixer.MinVolumeFeedback.GetShortValueOrDefault()
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
				if (VolumeOutputMixer == null)
					throw new ArgumentNullException("VolumeOutputMixer");

				return VolumeOutputMixer.MaxVolumeFeedback.GetShortValueOrDefault();
#else
				throw new NotSupportedException();
#endif
			}
		}

		#endregion

		#region Methods

		protected override void SetMicrophoneMute(ushort microphone, bool mute)
		{
#if !NETSTANDARD
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

		protected override void SetMicrophoneLevel(ushort microphone, short gainLevel)
		{
#if !NETSTANDARD
			if (VolumeOutputMixer == null)
				throw new ArgumentNullException("VolumeOutputMixer");

			if (!MicrophoneSupported(microphone))
				throw new NotSupportedException(string.Format("Microphone {0} is not supported", microphone));

			VolumeOutputMixer.MicLevel[microphone].ShortValue = gainLevel;
#else
			throw new NotSupportedException();
#endif
		}

		protected override void SetMicMasterMute(bool mute)
		{
#if !NETSTANDARD
			if (mute)
				VolumeObject.MicMasterMuteOn();
			else
				VolumeObject.MicMasterMuteOff();
#else
			throw new NotSupportedException();
#endif
		}

		protected override void SetMicMasterLevel(short gainLevel)
		{
#if !NETSTANDARD
			VolumeObject.MicMasterLevel.ShortValue = gainLevel;
#else
			throw new NotSupportedException();
#endif
		}

		protected override void SetSourceLevel(short gainLevel)
		{
#if !NETSTANDARD
			VolumeObject.SourceLevel.ShortValue = gainLevel;
#else
			throw new NotSupportedException();
#endif
		}

		protected override void SetSourceMute(bool mute)
		{
#if !NETSTANDARD
			if (mute)
				VolumeObject.SourceMuteOn();
			else
				VolumeObject.SourceMuteOff();
#else
			throw new NotSupportedException();
#endif
		}

		protected override void SetMasterVolumeLevel(short gainLevel)
		{
#if !NETSTANDARD
			VolumeObject.MasterVolume.ShortValue = gainLevel;
#else
			throw new NotSupportedException();
#endif
		}

		protected override void SetMasterVolumeMute(bool mute)
		{
#if !NETSTANDARD
			if (mute)
				VolumeObject.MasterMuteOn();
			else
				VolumeObject.MasterMuteOff();
#else
			throw new NotSupportedException();
#endif
		}

		#endregion

		#region Private Methods

		/// <summary>
		/// Returns true if there is a microphone at the given address and the microphone is supported.
		/// </summary>
		/// <param name="microphone"></param>
		/// <returns></returns>
		protected override bool MicrophoneSupported(ushort microphone)
		{
#if !NETSTANDARD
			return VolumeOutputMixer != null &&
			       VolumeOutputMixer.MicLevel.Contains(microphone) &&
				   VolumeOutputMixer.MicLevel[microphone].Supported;
#else
			return false;
#endif
		}

		#endregion

		#region Parent Callbacks

#if !NETSTANDARD

		/// <summary>
		/// Updates the volume/mute state with the master volume values
		/// </summary>
		protected override void UpdateMasterVolume()
		{
			VolumeLevel = VolumeObject.MasterVolumeFeedBack.GetShortValueOrDefault();
			VolumeIsMuted = VolumeObject.MasterMuteOnFeedBack.GetBoolValueOrDefault();
		}

		/// <summary>
		/// Updates the volume/mute states with the values for the given microphone
		/// </summary>
		/// <param name="microphone"></param>
		protected override void UpdateMicVolume(uint microphone)
		{
			if (VolumeOutputMixer != null)
			{
				VolumeLevel = VolumeOutputMixer.MicLevelFeedback[microphone].ShortValue;
				VolumeIsMuted = VolumeOutputMixer.MicMuteOnFeedback[microphone].GetBoolValueOrDefault();
			}
		}

		/// <summary>
		/// Updates the volume/mute states with the Mic Master volume values
		/// </summary>
		protected override void UpdateMicMasterVolume()
		{
			VolumeLevel = VolumeObject.MicMasterLevelFeedBack.GetShortValueOrDefault();
			VolumeIsMuted = VolumeObject.MicMasterMuteOnFeedBack.GetBoolValueOrDefault();
		}

		/// <summary>
		/// Updates the volume/mute states with the source volume values
		/// </summary>
		protected override void UpdateSourceVolume()
		{
			VolumeLevel = VolumeObject.SourceLevelFeedBack.GetShortValueOrDefault();
			VolumeIsMuted = VolumeObject.SourceMuteOnFeedBack.GetBoolValueOrDefault();
		}
#endif

		#endregion
	}
}
