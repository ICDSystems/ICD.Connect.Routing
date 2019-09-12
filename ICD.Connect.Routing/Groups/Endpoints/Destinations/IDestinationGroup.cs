using ICD.Connect.Routing.Endpoints.Destinations;

namespace ICD.Connect.Routing.Groups.Endpoints.Destinations
{
	public interface IDestinationGroup : ISourceDestinationBaseGroup
	{
	}

	public interface IDestinationGroup<T> : ISourceDestinationBaseGroup<T>, ISourceDestinationBaseGroup
		where T : IDestination
	{
	}
}
