using ICD.Connect.Routing.Endpoints.Sources;

namespace ICD.Connect.Routing.Groups.Endpoints.Sources
{
	public interface ISourceGroup : ISourceDestinationBaseGroup
	{
	}

	public interface ISourceGroup<T> : ISourceDestinationBaseGroup<T>, ISourceGroup
		where T : ISource
	{
	}
}
