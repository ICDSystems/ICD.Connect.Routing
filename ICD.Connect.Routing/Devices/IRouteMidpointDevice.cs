using System;
using System.Collections.Generic;
using ICD.Connect.Routing.Connections;

namespace ICD.Connect.Routing.Devices
{
	public interface IRouteMidpointDevice : IRouteSourceDevice, IRouteDestinationDevice
	{
		/// <summary>
		/// Gets the input routed to the given output matching the given type.
		/// </summary>
		/// <param name="output"></param>
		/// <param name="type"></param>
		/// <returns></returns>
		/// <exception cref="InvalidOperationException">Type has multiple flags.</exception>
		ConnectorInfo? GetInput(int output, eConnectionType type);

		/// <summary>
		/// Gets the outputs for the given input.
		/// </summary>
		/// <param name="input"></param>
		/// <param name="type"></param>
		/// <returns></returns>
		IEnumerable<ConnectorInfo> GetOutputs(int input, eConnectionType type);
	}
}