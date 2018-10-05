using ICD.Connect.Settings.Attributes;

namespace ICD.Connect.Routing.CrestronPro.Receivers.DmRmc200C
{
	[KrangSettings("DmRmc200C", typeof(DmRmc200CAdapter))]
	public sealed class DmRmc200CAdapterSettings : AbstractEndpointReceiverBaseAdapterSettings
	{
	}
}
