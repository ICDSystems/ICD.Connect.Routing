﻿using System.Linq;
using ICD.Connect.Routing.Endpoints.Destinations;
using ICD.Connect.Routing.Endpoints.Groups;
using ICD.Connect.Routing.Endpoints.Sources;
using ICD.Connect.Settings;
using ICD.Connect.Settings.Core;

namespace ICD.Connect.Routing.Extensions
{
	public sealed class CoreSourceCollection : AbstractOriginatorCollection<ISource>
	{
	}
	public sealed class CoreDestinationCollection : AbstractOriginatorCollection<IDestination>
	{
	}
	public sealed class CoreDestinationGroupCollection : AbstractOriginatorCollection<IDestinationGroup>
	{
	}

	public static class CoreExtensions
	{
		public static IRoutingGraph GetRoutingGraph(this ICore core)
		{
			return core.Originators.OfType<IRoutingGraph>().SingleOrDefault();
		}
	}
}