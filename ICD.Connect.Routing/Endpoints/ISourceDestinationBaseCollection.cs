﻿using System;
using System.Collections.Generic;
using ICD.Connect.Routing.Connections;
using ICD.Connect.Routing.RoutingGraphs;
using ICD.Connect.Settings;

namespace ICD.Connect.Routing.Endpoints
{
	public interface ISourceDestinationBaseCollection<T> : IOriginatorCollection<T>
		where T : class, ISourceDestinationBase
	{
		/// <summary>
		/// Raised when the disabled state of a source destination base changes.
		/// </summary>
		event EventHandler<SourceDestinationBaseDisabledStateChangedEventArgs> OnSourceDestinationBaseDisabledStateChanged;

		/// <summary>
		/// Gets the child with the given endpoint info.
		/// </summary>
		/// <param name="endpoint"></param>
		/// <returns></returns>
		IEnumerable<T> GetChildren(EndpointInfo endpoint);

		/// <summary>
		/// Gets the child with the given endpoint info.
		/// </summary>
		/// <param name="endpoint"></param>
		/// <param name="type"></param>
		/// <returns></returns>
		IEnumerable<T> GetChildren(EndpointInfo endpoint, eConnectionType type);
	}
}
