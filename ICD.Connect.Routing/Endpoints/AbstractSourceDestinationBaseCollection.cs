using ICD.Connect.Settings;

namespace ICD.Connect.Routing.Endpoints
{
	public abstract class AbstractSourceDestinationBaseCollection<T> : AbstractOriginatorCollection<T>,
	                                                                   ISourceDestinationBaseCollection<T>
		where T : ISourceDestinationBase
	{
	}
}
