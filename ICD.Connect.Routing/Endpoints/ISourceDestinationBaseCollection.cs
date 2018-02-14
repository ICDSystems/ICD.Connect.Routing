using ICD.Connect.Routing.Connections;
using ICD.Connect.Settings;

namespace ICD.Connect.Routing.Endpoints
{
	public interface ISourceDestinationBaseCollection<T> : IOriginatorCollection<T>
		where T : ISourceDestinationBase
	{
		/// <summary>
		/// Gets the child with the given endpoint info.
		/// </summary>
		/// <param name="endpoint"></param>
		/// <param name="type"></param>
		/// <returns></returns>
		T GetChild(EndpointInfo endpoint, eConnectionType type);

		/// <summary>
		/// Gets the child with the given endpoint info.
		/// </summary>
		/// <param name="endpoint"></param>
		/// <param name="type"></param>
		/// <param name="output"></param>
		/// <returns></returns>
		bool TryGetChild(EndpointInfo endpoint, eConnectionType type, out T output);
	}
}
