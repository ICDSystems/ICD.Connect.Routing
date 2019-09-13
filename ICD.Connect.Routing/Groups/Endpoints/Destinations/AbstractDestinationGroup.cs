using ICD.Connect.Routing.Endpoints.Destinations;

namespace ICD.Connect.Routing.Groups.Endpoints.Destinations
{
	public abstract class AbstractDestinationGroup<TOriginator, TSettings> :
		AbstractSourceDestinationBaseGroup<TOriginator, TSettings>, IDestinationGroup
		where TOriginator : class, IDestination
		where TSettings : IDestinationGroupSettings, new()
	{
	}
}
