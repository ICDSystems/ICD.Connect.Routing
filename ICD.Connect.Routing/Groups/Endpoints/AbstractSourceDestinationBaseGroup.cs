using ICD.Connect.Routing.Endpoints;
using ICD.Connect.Settings.Groups;

namespace ICD.Connect.Routing.Groups.Endpoints
{
	public abstract class AbstractSourceDestinationBaseGroup<TOriginator, TSettings> :
		AbstractGroup<TOriginator, TSettings>, ISourceDestinationBaseGroup
		where TOriginator : class, ISourceDestinationBase
		where TSettings : ISourceDestinationBaseGroupSettings, new()
	{
	}
}
