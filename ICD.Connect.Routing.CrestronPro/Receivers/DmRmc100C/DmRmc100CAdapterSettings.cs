using ICD.Connect.Routing.CrestronPro.Receivers.DmRmc100CBase;
using ICD.Connect.Settings.Attributes;

namespace ICD.Connect.Routing.CrestronPro.Receivers.DmRmc100C
{
	[KrangSettings("DmRmc100C", typeof(DmRmc100CAdapter))]
	public sealed class DmRmc100CAdapterSettings : AbstractDmRmc100CBaseAdapterSettings
	{
	}
}
