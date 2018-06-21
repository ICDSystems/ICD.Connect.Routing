using ICD.Connect.Settings.Attributes;

namespace ICD.Connect.Routing.CrestronPro.Receivers.DmRmc100S
{
	[KrangSettings("DmRmc100S", typeof(DmRmc100SAdapter))]
	public sealed class DmRmc100SAdapterSettings : AbstractEndpointReceiverBaseAdapterSettings
	{
	}
}
