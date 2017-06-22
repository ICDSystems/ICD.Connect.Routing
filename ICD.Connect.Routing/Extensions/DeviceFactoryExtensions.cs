using System;
using System.Collections.Generic;
using System.Linq;
using ICD.Common.Properties;
using ICD.Connect.Routing.Connections;
using ICD.Connect.Routing.Endpoints.Destinations;
using ICD.Connect.Routing.Endpoints.Groups;
using ICD.Connect.Routing.Endpoints.Sources;
using ICD.Connect.Routing.StaticRoutes;
using ICD.Connect.Settings.Core;

namespace ICD.Connect.Routing.Extensions
{
	public static class DeviceFactoryExtensions
	{
		public static ISource GetSourceById(this IDeviceFactory factory, int id)
		{
			return factory.GetOriginatorById<ISource>(id);
		}

		public static IDestination GetDestinationById(this IDeviceFactory factory, int id)
		{
			return factory.GetOriginatorById<IDestination>(id);
		}

		public static IDestinationGroup GetDestinationGroupById(this IDeviceFactory factory, int id)
		{
			return factory.GetOriginatorById<IDestinationGroup>(id);
		}

		/// <summary>
		/// Lazy-loads the Connection with the given id.
		/// </summary>
		/// <param name="id"></param>
		/// <returns></returns>
		[PublicAPI]
		public static Connection GetConnectionById(this IDeviceFactory factory, int id)
		{
			return factory.GetOriginatorById<Connection>(id);
		}

		/// <summary>
		/// Lazy-loads the StaticRoute with the given id.
		/// </summary>
		/// <param name="id"></param>
		/// <returns></returns>
		[PublicAPI]
		public static StaticRoute GetStaticRouteById(this IDeviceFactory factory, int id)
		{
			return factory.GetOriginatorById<StaticRoute>(id);
		}

	}
}