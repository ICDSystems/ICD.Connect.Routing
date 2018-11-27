using ICD.Connect.Routing.CrestronPro.Receivers.DmRmc100CBase;
using ICD.Connect.Settings.Attributes;

namespace ICD.Connect.Routing.CrestronPro.Receivers.DmRmc4k100C
{
	[KrangSettings("DmRmc4k100C", typeof(DmRmc4k100CAdapter))]
	public sealed class DmRmc4k100CAdapterSettings : AbstractDmRmc100CBaseAdapterSettings
	{
	}
}
