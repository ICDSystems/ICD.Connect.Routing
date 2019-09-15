using System.Collections.Generic;
using ICD.Connect.Routing.Connections;

namespace ICD.Connect.Routing.Endpoints.Destinations
{
	/// <summary>
	/// Common interface between destinations and destination groups.
	/// </summary>
	public interface IDestinationBase : ISourceDestinationBaseCommon
	{
		/// <summary>
		/// Gets the destinations represented by this instance.
		/// </summary>
		/// <returns></returns>
		IEnumerable<IDestination> GetDestinations();

		/// <summary>
		/// Gets destinations supporting the given connection type.
		/// </summary>
		/// <param name="type"></param>
		/// <returns></returns>
		IEnumerable<IDestination> GetDestinations(eConnectionType type);
	}
}
