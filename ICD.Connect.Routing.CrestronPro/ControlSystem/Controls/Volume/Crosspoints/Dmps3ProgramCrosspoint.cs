#if SIMPLSHARP
#endif
using Crestron.SimplSharpPro.DM;
using Crestron.SimplSharpPro.DM.Cards;

#if SIMPLSHARP
namespace ICD.Connect.Routing.CrestronPro.ControlSystem.Controls.Volume.Crosspoints
{
	public sealed class Dmps3ProgramCrosspoint : AbstractDmps3Crosspoint
	{

#if SIMPLSHARP
		private Card.Dmps3ProgramOutput ProgramOutputVolumeObject { get { return VolumeObject as Card.Dmps3ProgramOutput; } }
#endif
		public Dmps3ProgramCrosspoint(ControlSystemDevice parent, Card.Dmps3ProgramOutput output, eDmps3InputType inputType, uint inputAddress)
			: base(parent, output, inputType, inputAddress)
		{
		}

		#region Methods

		protected override void SetCodec1Level(short gainLevel)
		{
			ProgramOutputVolumeObject.Codec1Level.ShortValue = gainLevel;
		}

		protected override void SetCodec2Level(short gainLevel)
		{
			ProgramOutputVolumeObject.Codec2Level.ShortValue = gainLevel;
		}

		protected override void SetCodec1Mute(bool mute)
		{
			if (mute)
				ProgramOutputVolumeObject.Codec1MuteOn();
			else
				ProgramOutputVolumeObject.Codec1MuteOff();
		}

		protected override void SetCodec2Mute(bool mute)
		{
			if (mute)
				ProgramOutputVolumeObject.Codec2MuteOn();
			else
				ProgramOutputVolumeObject.Codec2MuteOff();
		}

		#endregion

		protected override void ControlSystemOnDmOutputChange(Switch device, DMOutputEventArgs args)
		{
			base.ControlSystemOnDmOutputChange(device, args);

			switch (InputType)
			{
				case eDmps3InputType.Codec1:
					VolumeLevel = ProgramOutputVolumeObject.Codec1LevelFeedback.ShortValue;
					VolumeIsMuted = ProgramOutputVolumeObject.CodecMute1OnFeedback.BoolValue;
					break;

				case eDmps3InputType.Codec2:
					VolumeLevel = ProgramOutputVolumeObject.Codec1LevelFeedback.ShortValue;
					VolumeIsMuted = ProgramOutputVolumeObject.CodecMute1OnFeedback.BoolValue;
					break;

				case eDmps3InputType.Microphone:
					if (VolumeOutputMixer != null)
					{
						VolumeLevel = VolumeOutputMixer.MicLevel[InputAddress].ShortValue;
						VolumeIsMuted = VolumeOutputMixer.MicMuteOnFeedback[InputAddress].BoolValue;
					}
					break;
			}
		}
/*
		#region Console

		/// <summary>
		/// Gets the child console commands.
		/// </summary>
		/// <returns></returns>
		public override IEnumerable<IConsoleCommand> GetConsoleCommands()
		{
			foreach (IConsoleCommand command in GetBaseConsoleCommands())
				yield return command;

			yield return new GenericConsoleCommand<bool>("SetCodec1Mute", "SetCodec1Mute <BOOL>", r => SetCodec1Mute(r));
			yield return new GenericConsoleCommand<short>("SetCodec1Level", "SetCodec1Level <SHORT>", r => SetCodec1Level(r));

			yield return new GenericConsoleCommand<bool>("SetCodec2Mute", "SetCodec2Mute <BOOL>", r => SetCodec2Mute(r));
			yield return new GenericConsoleCommand<short>("SetCodec2Level", "SetCodec2Level <SHORT>", r => SetCodec2Level(r));
		}

		/// <summary>
		/// Workaround for "unverifiable code" warning.
		/// </summary>
		/// <returns></returns>
		private IEnumerable<IConsoleCommand> GetBaseConsoleCommands()
		{
			return base.GetConsoleCommands();
		}

		#endregion
		*/
	}
}
#endif