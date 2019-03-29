using ICD.Connect.Misc.CrestronPro.Extensions;
#if SIMPLSHARP
using Crestron.SimplSharpPro.DM;
using Crestron.SimplSharpPro.DM.Cards;
#else
using System;
#endif

namespace ICD.Connect.Routing.CrestronPro.ControlSystem.Controls.Volume.Crosspoints
{
	public sealed class Dmps3ProgramCrosspoint : AbstractDmps3Crosspoint
	{
#if SIMPLSHARP
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
#if SIMPLSHARP
			ProgramOutputVolumeObject.Codec1Level.ShortValue = gainLevel;
#else
			throw new NotSupportedException();
#endif
		}

		protected override void SetCodec2Level(short gainLevel)
		{
#if SIMPLSHARP
			ProgramOutputVolumeObject.Codec2Level.ShortValue = gainLevel;
#else
			throw new NotSupportedException();
#endif
		}

		protected override void SetCodec1Mute(bool mute)
		{
#if SIMPLSHARP
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
#if SIMPLSHARP
			if (mute)
				ProgramOutputVolumeObject.Codec2MuteOn();
			else
				ProgramOutputVolumeObject.Codec2MuteOff();
#else
			throw new NotSupportedException();
#endif
		}

		#endregion

#if SIMPLSHARP
		/// <summary>
		/// Called when the control system raises a DM output change event.
		/// </summary>
		/// <param name="device"></param>
		/// <param name="args"></param>
		protected override void ControlSystemOnDmOutputChange(Switch device, DMOutputEventArgs args)
		{
			base.ControlSystemOnDmOutputChange(device, args);

			switch (InputType)
			{
				case eDmps3InputType.Codec1:
					VolumeLevel = ProgramOutputVolumeObject.Codec1LevelFeedback.GetShortValueOrDefault();
					VolumeIsMuted = ProgramOutputVolumeObject.CodecMute1OnFeedback.GetBoolValueOrDefault();
					break;

				case eDmps3InputType.Codec2:
					VolumeLevel = ProgramOutputVolumeObject.Codec1LevelFeedback.GetShortValueOrDefault();
					VolumeIsMuted = ProgramOutputVolumeObject.CodecMute1OnFeedback.GetBoolValueOrDefault();
					break;
			}
		}
#endif
	}
}
