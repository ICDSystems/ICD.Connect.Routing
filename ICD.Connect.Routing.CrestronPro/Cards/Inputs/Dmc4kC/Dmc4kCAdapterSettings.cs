using ICD.Connect.Routing.CrestronPro.Cards.Inputs.Dmc4kCBase;
using ICD.Connect.Settings.Attributes;

namespace ICD.Connect.Routing.CrestronPro.Cards.Inputs.Dmc4kC
{
	[KrangSettings("Dmc4kC", typeof(Dmc4kCAdapter))]
	public sealed class Dmc4kCAdapterSettings : AbstractDmc4kCBaseAdapterSettings
	{
	}
}
