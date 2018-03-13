﻿using System.Collections.Generic;
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
		/// <param name="flag"></param>
		/// <returns></returns>
		IEnumerable<T> GetChildren(EndpointInfo endpoint, eConnectionType flag);
	}
}
