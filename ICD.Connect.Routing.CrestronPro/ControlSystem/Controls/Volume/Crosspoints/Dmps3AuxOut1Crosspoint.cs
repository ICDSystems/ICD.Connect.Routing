#if !NETSTANDARD
using Crestron.SimplSharpPro.DM.Cards;
using ICD.Connect.Misc.CrestronPro.Extensions;
#else
using System;
#endif

namespace ICD.Connect.Routing.CrestronPro.ControlSystem.Controls.Volume.Crosspoints
{
	public sealed class Dmps3AuxOut1Crosspoint : AbstractDmps3OutputBaseCrosspoint
	{
#if !NETSTANDARD
		private Card.Dmps3Aux1Output Aux1OutputVolumeObject { get { return VolumeObject as Card.Dmps3Aux1Output; } }

		public Dmps3AuxOut1Crosspoint(ControlSystemDevice parent, Card.Dmps3Aux1Output output, eDmps3InputType inputType, uint inputAddress)
			: base(parent, output, inputType, inputAddress)
		{
		}
#endif

		#region Methods

		protected override void SetCodec2Level(short gainLevel)
		{
#if !NETSTANDARD
			Aux1OutputVolumeObject.Codec2Level.ShortValue = gainLevel;
#else
			throw new NotSupportedException();
#endif
		}

		protected override void SetCodec2Mute(bool mute)
		{
#if !NETSTANDARD
			if (mute)
				Aux1OutputVolumeObject.Codec2MuteOn();
			else
				Aux1OutputVolumeObject.Codec2MuteOff();
#else
			throw new NotSupportedException();
#endif
		}

		#endregion

#if !NETSTANDARD

		/// <summary>
		/// Updates the volume/mute states with the Codec 2 volume values
		/// </summary>
		protected override void UpdateCodec2Volume()
		{
			VolumeLevel = Aux1OutputVolumeObject.Codec2LevelFeedback.GetShortValueOrDefault();
			VolumeIsMuted = Aux1OutputVolumeObject.CodecMute2OnFeedback.GetBoolValueOrDefault();
		}
#endif
	}
}
