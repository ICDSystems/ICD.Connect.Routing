using ICD.Connect.Routing.Endpoints.Sources;

namespace ICD.Connect.Routing.Groups.Endpoints.Sources
{
	public abstract class AbstractSourceGroup<TOriginator, TSettings> :
		AbstractSourceDestinationBaseGroup<TOriginator, TSettings>, ISourceGroup
		where TOriginator : class, ISource
		where TSettings : ISourceGroupSettings, new()
	{
	}
}
