using ICD.Connect.Settings.Attributes;

namespace ICD.Connect.Routing.Endpoints.Destinations
{
	[KrangSettings("Destination", typeof(Destination))]
	public sealed class DestinationSettings : AbstractDestinationSettings
	{
	}
}
