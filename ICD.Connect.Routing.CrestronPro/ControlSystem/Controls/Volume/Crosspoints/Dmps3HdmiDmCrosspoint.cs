#if SIMPLSHARP
using Crestron.SimplSharpPro.DM;
using Crestron.SimplSharpPro.DM.Cards;
#endif

namespace ICD.Connect.Routing.CrestronPro.ControlSystem.Controls.Volume.Crosspoints
{
	public sealed class Dmps3HdmiDmCrosspoint : AbstractDmps3Crosspoint
	{
#if SIMPLSHARP
		public Dmps3HdmiDmCrosspoint(ControlSystemDevice parent, Card.Dmps3OutputBase output, eDmps3InputType inputType, uint inputAddress)
			: base(parent, output, inputType, inputAddress)
		{
		}

		protected override void ControlSystemOnDmOutputChange(Switch device, DMOutputEventArgs args)
		{
			base.ControlSystemOnDmOutputChange(device, args);

			switch (InputType)
			{
				case eDmps3InputType.Microphone:
					if (VolumeOutputMixer != null)
					{
						VolumeLevel = VolumeOutputMixer.MicLevel[InputAddress].ShortValue;
						VolumeIsMuted = VolumeOutputMixer.MicMuteOnFeedback[InputAddress].BoolValue;
					}
					break;
			}
		}
#endif
	}
}
