using ICD.Connect.Routing.Endpoints;
using ICD.Connect.Settings.Groups;

namespace ICD.Connect.Routing.Groups.Endpoints
{
	public interface ISourceDestinationBaseGroup : IGroup<ISourceDestinationBase>
	{
	}

	public interface ISourceDestinationBaseGroup<T> : IGroup<T>, ISourceDestinationBaseGroup
		where T : ISourceDestinationBase
	{
	}
}
