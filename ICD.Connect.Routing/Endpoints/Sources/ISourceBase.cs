using System.Collections.Generic;
using ICD.Connect.Routing.Connections;

namespace ICD.Connect.Routing.Endpoints.Sources
{
	/// <summary>
	/// Common interface between sources and source groups.
	/// </summary>
	public interface ISourceBase : ISourceDestinationBaseCommon
	{
		/// <summary>
		/// Gets the sources represented by this instance.
		/// </summary>
		/// <returns></returns>
		IEnumerable<ISource> GetSources();

		/// <summary>
		/// Gets sources supporting the given connection type.
		/// </summary>
		/// <param name="type"></param>
		/// <returns></returns>
		IEnumerable<ISource> GetSources(eConnectionType type);
	}
}