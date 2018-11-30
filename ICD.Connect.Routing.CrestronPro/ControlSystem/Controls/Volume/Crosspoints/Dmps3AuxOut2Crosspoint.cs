#if SIMPLSHARP
#endif
using Crestron.SimplSharpPro.DM;
using Crestron.SimplSharpPro.DM.Cards;
using ICD.Common.Utils;

#if SIMPLSHARP
namespace ICD.Connect.Routing.CrestronPro.ControlSystem.Controls.Volume.Crosspoints
{
	public sealed class Dmps3AuxOut2Crosspoint : AbstractDmps3Crosspoint
	{

#if SIMPLSHARP
		private Card.Dmps3Aux2Output Aux2OutputVolumeObject { get { return VolumeObject as Card.Dmps3Aux2Output; } }
#endif
		public Dmps3AuxOut2Crosspoint(ControlSystemDevice parent, Card.Dmps3Aux2Output output, eDmps3InputType inputType, uint inputAddress)
			: base(parent, output, inputType, inputAddress)
		{
		}

		#region Methods

		protected override void SetCodec1Level(short gainLevel)
		{
			Aux2OutputVolumeObject.Codec1Level.ShortValue = gainLevel;
		}

		protected override void SetCodec1Mute(bool mute)
		{
			if (mute)
				Aux2OutputVolumeObject.Codec1MuteOn();
			else
				Aux2OutputVolumeObject.Codec1MuteOff();
		}

		#endregion

		protected override void ControlSystemOnDmOutputChange(Switch device, DMOutputEventArgs args)
		{
			base.ControlSystemOnDmOutputChange(device, args);

			switch (InputType)
			{
				case eDmps3InputType.Codec1:
					VolumeLevel = Aux2OutputVolumeObject.Codec1LevelFeedback.ShortValue;
					VolumeIsMuted = Aux2OutputVolumeObject.CodecMute1OnFeedback.BoolValue;
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