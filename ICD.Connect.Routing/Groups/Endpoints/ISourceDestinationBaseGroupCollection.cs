using ICD.Connect.Settings.Originators;

namespace ICD.Connect.Routing.Groups.Endpoints
{
	public interface ISourceDestinationBaseGroupCollection<T> : IOriginatorCollection<T>
		where T : class, ISourceDestinationBaseGroup
	{
	}
}
