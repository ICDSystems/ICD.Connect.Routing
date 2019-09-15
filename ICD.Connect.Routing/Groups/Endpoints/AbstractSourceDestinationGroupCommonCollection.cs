using ICD.Connect.Settings.Originators;

namespace ICD.Connect.Routing.Groups.Endpoints
{
	public abstract class AbstractSourceDestinationGroupCommonCollection<T> : AbstractOriginatorCollection<T>,
	                                                                        ISourceDestinationGroupCommonCollection<T>
		where T : class, ISourceDestinationGroupCommon
	{
	}
}
