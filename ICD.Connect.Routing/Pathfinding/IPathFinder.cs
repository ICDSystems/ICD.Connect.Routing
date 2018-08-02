using System.Collections.Generic;
using ICD.Connect.Routing.Connections;

namespace ICD.Connect.Routing.Pathfinding
{
	public interface IPathFinder
	{
		/// <summary>
		/// Returns the best paths for the given builder queries.
		/// </summary>
		/// <param name="queries"></param>
		/// <returns></returns>
		IEnumerable<ConnectionPath> FindPaths(IEnumerable<PathBuilderQuery> queries);
	}
}
