using ICD.Connect.Settings;

namespace ICD.Connect.Routing.Endpoints
{
	public interface ISourceDestinationBaseCollection<T> : IOriginatorCollection<T>
		where T : ISourceDestinationBase
	{
	}
}
