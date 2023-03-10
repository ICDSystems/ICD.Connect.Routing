using System.Collections.Generic;
using ICD.Connect.Routing.Connections;

namespace ICD.Connect.Routing.PathFinding
{
	public interface IPathFinder
	{
		/// <summary>
		/// Returns the best paths for the given builder queries.
		/// </summary>
		/// <param name="queries"></param>
		/// <returns></returns>
		IEnumerable<ConnectionPath> FindPaths(IEnumerable<PathBuilderQuery> queries);

		/// <summary>
		/// Returns true if there is a valid path for all of the defined queries.
		/// </summary>
		/// <param name="queries"></param>
		/// <returns></returns>
		bool HasPaths(IEnumerable<PathBuilderQuery> queries);
	}
}
