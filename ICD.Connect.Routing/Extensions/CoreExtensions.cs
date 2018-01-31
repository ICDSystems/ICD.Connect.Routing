using System;
using ICD.Connect.Routing.RoutingGraphs;
using ICD.Connect.Settings.Core;

namespace ICD.Connect.Routing.Extensions
{
	/// <summary>
	/// Gets the routing graph instance from the core.
	/// </summary>
	public static class CoreExtensions
	{
		public static IRoutingGraph GetRoutingGraph(this ICore core)
		{
			if (core == null)
				throw new ArgumentNullException("core");

			return core.Originators.GetChild<IRoutingGraph>();
		}
	}
}