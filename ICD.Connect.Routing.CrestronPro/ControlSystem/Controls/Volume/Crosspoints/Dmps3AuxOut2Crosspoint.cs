#if !NETSTANDARD
using Crestron.SimplSharpPro.DM.Cards;
using ICD.Connect.Misc.CrestronPro.Extensions;
#else
using System;
#endif

namespace ICD.Connect.Routing.CrestronPro.ControlSystem.Controls.Volume.Crosspoints
{
	public sealed class Dmps3AuxOut2Crosspoint : AbstractDmps3OutputBaseCrosspoint
	{
#if !NETSTANDARD
		private Card.Dmps3Aux2Output Aux2OutputVolumeObject { get { return VolumeObject as Card.Dmps3Aux2Output; } }
#endif

#if !NETSTANDARD
		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="parent"></param>
		/// <param name="output"></param>
		/// <param name="inputType"></param>
		/// <param name="inputAddress"></param>
		public Dmps3AuxOut2Crosspoint(ControlSystemDevice parent, Card.Dmps3Aux2Output output, eDmps3InputType inputType,
		                              uint inputAddress)
			: base(parent, output, inputType, inputAddress)
		{
		}
#endif

		#region Methods

		protected override void SetCodec1Level(short gainLevel)
		{
#if !NETSTANDARD
			Aux2OutputVolumeObject.Codec1Level.ShortValue = gainLevel;
#else
			throw new NotSupportedException();
#endif
		}

		protected override void SetCodec1Mute(bool mute)
		{
#if !NETSTANDARD
			if (mute)
				Aux2OutputVolumeObject.Codec1MuteOn();
			else
				Aux2OutputVolumeObject.Codec1MuteOff();
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
			VolumeLevel = Aux2OutputVolumeObject.Codec1LevelFeedback.GetShortValueOrDefault();
			VolumeIsMuted = Aux2OutputVolumeObject.CodecMute1OnFeedback.GetBoolValueOrDefault();
		}

#endif
	}
}