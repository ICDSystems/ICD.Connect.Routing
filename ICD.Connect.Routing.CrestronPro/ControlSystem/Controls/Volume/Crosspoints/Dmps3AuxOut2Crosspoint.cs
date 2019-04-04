#if SIMPLSHARP
using Crestron.SimplSharpPro.DM;
using Crestron.SimplSharpPro.DM.Cards;
using ICD.Connect.Misc.CrestronPro.Extensions;
#else
using System;
#endif

namespace ICD.Connect.Routing.CrestronPro.ControlSystem.Controls.Volume.Crosspoints
{
	public sealed class Dmps3AuxOut2Crosspoint : AbstractDmps3Crosspoint
	{
#if SIMPLSHARP
		private Card.Dmps3Aux2Output Aux2OutputVolumeObject { get { return VolumeObject as Card.Dmps3Aux2Output; } }
#endif

#if SIMPLSHARP
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
#if SIMPLSHARP
			Aux2OutputVolumeObject.Codec1Level.ShortValue = gainLevel;
#else
			throw new NotSupportedException();
#endif
		}

		protected override void SetCodec1Mute(bool mute)
		{
#if SIMPLSHARP
			if (mute)
				Aux2OutputVolumeObject.Codec1MuteOn();
			else
				Aux2OutputVolumeObject.Codec1MuteOff();
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
				case eDmps3InputType.Codec1:
					VolumeLevel = Aux2OutputVolumeObject.Codec1LevelFeedback.GetShortValueOrDefault();
					VolumeIsMuted = Aux2OutputVolumeObject.CodecMute1OnFeedback.GetBoolValueOrDefault();
					break;
			}
		}
#endif
	}
}