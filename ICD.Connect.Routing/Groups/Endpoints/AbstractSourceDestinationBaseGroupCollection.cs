using ICD.Connect.Settings.Originators;

namespace ICD.Connect.Routing.Groups.Endpoints
{
	public abstract class AbstractSourceDestinationBaseGroupCollection<T> : AbstractOriginatorCollection<T>,
	                                                                        ISourceDestinationBaseGroupCollection<T>
		where T : class, ISourceDestinationBaseGroup
	{
	}
}
