#if SIMPLSHARP
#endif
using Crestron.SimplSharpPro.DM;
using Crestron.SimplSharpPro.DM.Cards;
using ICD.Common.Utils;

#if SIMPLSHARP
namespace ICD.Connect.Routing.CrestronPro.ControlSystem.Controls.Volume.Crosspoints
{
	public sealed class Dmps3AuxOut1Crosspoint : AbstractDmps3Crosspoint
	{

#if SIMPLSHARP
		private Card.Dmps3Aux1Output Aux1OutputVolumeObject { get { return VolumeObject as Card.Dmps3Aux1Output; } }
#endif
		public Dmps3AuxOut1Crosspoint(ControlSystemDevice parent, Card.Dmps3Aux1Output output, eDmps3InputType inputType, uint inputAddress)
			: base(parent, output, inputType, inputAddress)
		{
		}

		#region Methods

		protected override void SetCodec2Level(short gainLevel)
		{
			Aux1OutputVolumeObject.Codec2Level.ShortValue = gainLevel;
		}

		protected override void SetCodec2Mute(bool mute)
		{
			if (mute)
				Aux1OutputVolumeObject.Codec2MuteOn();
			else
				Aux1OutputVolumeObject.Codec2MuteOff();
		}

		#endregion

		protected override void ControlSystemOnDmOutputChange(Switch device, DMOutputEventArgs args)
		{
			base.ControlSystemOnDmOutputChange(device, args);

			switch (InputType)
			{
				case eDmps3InputType.Codec2:
					VolumeLevel = Aux1OutputVolumeObject.Codec2LevelFeedback.ShortValue;
					VolumeIsMuted = Aux1OutputVolumeObject.CodecMute2OnFeedback.BoolValue;
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