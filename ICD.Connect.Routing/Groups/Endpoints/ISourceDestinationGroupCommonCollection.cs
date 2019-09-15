using ICD.Connect.Settings.Originators;

namespace ICD.Connect.Routing.Groups.Endpoints
{
	public interface ISourceDestinationGroupCommonCollection<T> : IOriginatorCollection<T>
		where T : class, ISourceDestinationGroupCommon
	{
	}
}
