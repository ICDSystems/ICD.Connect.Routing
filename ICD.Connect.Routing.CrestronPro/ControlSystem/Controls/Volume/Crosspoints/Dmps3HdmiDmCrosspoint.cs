#if SIMPLSHARP
using Crestron.SimplSharpPro.DM.Cards;
#endif

namespace ICD.Connect.Routing.CrestronPro.ControlSystem.Controls.Volume.Crosspoints
{
	public sealed class Dmps3HdmiDmCrosspoint : AbstractDmps3OutputBaseCrosspoint
	{
#if SIMPLSHARP
		public Dmps3HdmiDmCrosspoint(ControlSystemDevice parent, Card.Dmps3OutputBase output, eDmps3InputType inputType, uint inputAddress)
			: base(parent, output, inputType, inputAddress)
		{
		}
#endif
	}
}
