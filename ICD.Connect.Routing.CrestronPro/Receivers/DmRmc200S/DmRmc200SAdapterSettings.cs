using ICD.Connect.Settings.Attributes;

namespace ICD.Connect.Routing.CrestronPro.Receivers.DmRmc200S
{
	[KrangSettings("DmRmc200S", typeof(DmRmc200SAdapter))]
	public sealed class DmRmc200SAdapterSettings : AbstractEndpointReceiverBaseAdapterSettings
	{
	}
}
