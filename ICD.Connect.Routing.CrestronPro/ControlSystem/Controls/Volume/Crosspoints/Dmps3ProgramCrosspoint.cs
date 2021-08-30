#if !NETSTANDARD
using Crestron.SimplSharpPro.DM.Cards;
using ICD.Connect.Misc.CrestronPro.Extensions;
#else
using System;
#endif

namespace ICD.Connect.Routing.CrestronPro.ControlSystem.Controls.Volume.Crosspoints
{
	public sealed class Dmps3ProgramCrosspoint : AbstractDmps3OutputBaseCrosspoint
	{
#if !NETSTANDARD
		private Card.Dmps3ProgramOutput ProgramOutputVolumeObject { get { return VolumeObject as Card.Dmps3ProgramOutput; } }

		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="parent"></param>
		/// <param name="output"></param>
		/// <param name="inputType"></param>
		/// <param name="inputAddress"></param>
		public Dmps3ProgramCrosspoint(ControlSystemDevice parent, Card.Dmps3ProgramOutput output, eDmps3InputType inputType, uint inputAddress)
			: base(parent, output, inputType, inputAddress)
		{
		}
#endif

#region Methods

		protected override void SetCodec1Level(short gainLevel)
		{
#if !NETSTANDARD
			ProgramOutputVolumeObject.Codec1Level.ShortValue = gainLevel;
#else
			throw new NotSupportedException();
#endif
		}

		protected override void SetCodec2Level(short gainLevel)
		{
#if !NETSTANDARD
			ProgramOutputVolumeObject.Codec2Level.ShortValue = gainLevel;
#else
			throw new NotSupportedException();
#endif
		}

		protected override void SetCodec1Mute(bool mute)
		{
#if !NETSTANDARD
			if (mute)
				ProgramOutputVolumeObject.Codec1MuteOn();
			else
				ProgramOutputVolumeObject.Codec1MuteOff();
#else
			throw new NotSupportedException();
#endif
		}

		protected override void SetCodec2Mute(bool mute)
		{
#if !NETSTANDARD
			if (mute)
				ProgramOutputVolumeObject.Codec2MuteOn();
			else
				ProgramOutputVolumeObject.Codec2MuteOff();
#else
			throw new NotSupportedException();
#endif
		}

		#endregion

#if !NETSTANDARD

		/// <summary>
		/// Updates the volume/mute states with the Codec 1 volume values
		/// </summary>
		protected override void UpdateCodec1Volume()
		{
			VolumeLevel = ProgramOutputVolumeObject.Codec1LevelFeedback.GetShortValueOrDefault();
			VolumeIsMuted = ProgramOutputVolumeObject.CodecMute1OnFeedback.GetBoolValueOrDefault();
		}

		/// <summary>
		/// Updates the volume/mute states with the Codec 2 volume values
		/// </summary>
		protected override void UpdateCodec2Volume()
		{
			VolumeLevel = ProgramOutputVolumeObject.Codec1LevelFeedback.GetShortValueOrDefault();
			VolumeIsMuted = ProgramOutputVolumeObject.CodecMute1OnFeedback.GetBoolValueOrDefault();
		}
#endif
	}
}
