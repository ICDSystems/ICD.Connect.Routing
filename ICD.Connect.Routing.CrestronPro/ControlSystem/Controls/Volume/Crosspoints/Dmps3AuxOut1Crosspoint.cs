#if SIMPLSHARP
using Crestron.SimplSharpPro.DM;
using Crestron.SimplSharpPro.DM.Cards;
using ICD.Connect.Misc.CrestronPro.Extensions;
#else
using System;
#endif

namespace ICD.Connect.Routing.CrestronPro.ControlSystem.Controls.Volume.Crosspoints
{
	public sealed class Dmps3AuxOut1Crosspoint : AbstractDmps3Crosspoint
	{
#if SIMPLSHARP
		private Card.Dmps3Aux1Output Aux1OutputVolumeObject { get { return VolumeObject as Card.Dmps3Aux1Output; } }

		public Dmps3AuxOut1Crosspoint(ControlSystemDevice parent, Card.Dmps3Aux1Output output, eDmps3InputType inputType, uint inputAddress)
			: base(parent, output, inputType, inputAddress)
		{
		}
#endif

		#region Methods

		protected override void SetCodec2Level(short gainLevel)
		{
#if SIMPLSHARP
			Aux1OutputVolumeObject.Codec2Level.ShortValue = gainLevel;
#else
			throw new NotSupportedException();
#endif
		}

		protected override void SetCodec2Mute(bool mute)
		{
#if SIMPLSHARP
			if (mute)
				Aux1OutputVolumeObject.Codec2MuteOn();
			else
				Aux1OutputVolumeObject.Codec2MuteOff();
#else
			throw new NotSupportedException();
#endif
		}

		#endregion

#if SIMPLSHARP
		protected override void ControlSystemOnDmOutputChange(Switch device, DMOutputEventArgs args)
		{
			base.ControlSystemOnDmOutputChange(device, args);

			switch (InputType)
			{
				case eDmps3InputType.Codec2:
					VolumeLevel = Aux1OutputVolumeObject.Codec2LevelFeedback.GetShortValueOrDefault();
					VolumeIsMuted = Aux1OutputVolumeObject.CodecMute2OnFeedback.GetBoolValueOrDefault();
					break;
			}
		}
#endif
	}
}
