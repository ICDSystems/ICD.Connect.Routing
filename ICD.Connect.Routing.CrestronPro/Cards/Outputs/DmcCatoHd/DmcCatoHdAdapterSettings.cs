using ICD.Connect.Routing.CrestronPro.Cards.Outputs.Dmc4kCoHdBase;
using ICD.Connect.Settings.Attributes;

namespace ICD.Connect.Routing.CrestronPro.Cards.Outputs.DmcCatoHd
{
	[KrangSettings("DmcCatoHd", typeof(DmcCatoHdAdapter))]
	public sealed class DmcCatoHdAdapterSettings : AbstractDmc4kCoHdBaseAdapterSettings
	{
	}
}
