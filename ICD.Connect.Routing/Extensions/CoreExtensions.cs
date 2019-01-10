using System;
using ICD.Common.Utils.Extensions;
using ICD.Connect.Routing.RoutingGraphs;
using ICD.Connect.Settings.Cores;

namespace ICD.Connect.Routing.Extensions
{
	/// <summary>
	/// Gets the routing graph instance from the core.
	/// </summary>
	public static class CoreExtensions
	{
		/// <summary>
		/// Gets the first routing graph instance from the core.
		/// </summary>
		/// <param name="core"></param>
		/// <returns></returns>
		public static IRoutingGraph GetRoutingGraph(this ICore core)
		{
			if (core == null)
				throw new ArgumentNullException("core");

			return core.Originators.GetChild<IRoutingGraph>();
		}

		/// <summary>
		/// Tries to get the first routing graph instance from the core.
		/// </summary>
		/// <param name="core"></param>
		/// <param name="output"></param>
		/// <returns></returns>
		public static bool TryGetRoutingGraph(this ICore core, out IRoutingGraph output)
		{
			if (core == null)
				throw new ArgumentNullException("core");

			return core.Originators.GetChildren<IRoutingGraph>().TryFirst(out output);
		}
	}
}
