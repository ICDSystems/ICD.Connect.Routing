using ICD.Connect.Settings.Attributes;

namespace ICD.Connect.Routing.Endpoints.Groups
{
	[KrangSettings("DestinationGroup", typeof(DestinationGroup))]
	public sealed class DestinationGroupSettings : AbstractDestinationGroupSettings
	{
	}
}
