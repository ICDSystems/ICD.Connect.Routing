using ICD.Connect.Settings.Attributes;

namespace ICD.Connect.Routing.CrestronPro.Receivers.DmRmc150S
{
	[KrangSettings("DmRmc150S", typeof(DmRmc150SAdapter))]
	public sealed class DmRmc150SAdapterSettings : AbstractEndpointReceiverBaseAdapterSettings
	{
	}
}
