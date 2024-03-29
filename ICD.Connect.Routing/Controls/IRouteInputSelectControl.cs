﻿using ICD.Connect.API.Attributes;
using ICD.Connect.Routing.Connections;
using ICD.Connect.Routing.Proxies;

namespace ICD.Connect.Routing.Controls
{
	/// <summary>
	/// Describes a route destination where only one input may be active at a given time.
	/// </summary>
	public interface IRouteInputSelectControl : IRouteDestinationControl
	{
		/// <summary>
		/// Sets the current active input.
		/// </summary>
		/// <param name="input"></param>
		/// <param name="type"></param>
		[ApiMethod(RouteInputSelectControlApi.METHOD_SET_ACTIVE_INPUT,RouteInputSelectControlApi.HELP_METHOD_SET_ACTIVE_INPUT)]
		void SetActiveInput(int? input, eConnectionType type);

		/// <summary>
		/// Gets the current active input.
		/// </summary>
		int? GetActiveInput(eConnectionType flag);
	}
}
