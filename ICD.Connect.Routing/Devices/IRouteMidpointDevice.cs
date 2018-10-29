using System;
using System.Collections.Generic;
using System.Linq;
using ICD.Common.Properties;
using ICD.Common.Utils;
using ICD.Common.Utils.Extensions;
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

	public static class RouteMidpointDeviceExtensions
	{
		/// <summary>
		/// Gets the inputs for the given output.
		/// </summary>
		/// <param name="extends"></param>
		/// <param name="output"></param>
		/// <returns></returns>
		[PublicAPI]
		public static IEnumerable<ConnectorInfo> GetInputs(this IRouteMidpointDevice extends, int output)
		{
			if (extends == null)
				throw new ArgumentNullException("extends");

			return extends.GetInputs(output, EnumUtils.GetFlagsAllValue<eConnectionType>());
		}

		/// <summary>
		/// Gets the input for the given output.
		/// </summary>
		/// <param name="extends"></param>
		/// <param name="output"></param>
		/// <param name="type"></param>
		/// <returns></returns>
		public static IEnumerable<ConnectorInfo> GetInputs(this IRouteMidpointDevice extends, int output,
		                                                   eConnectionType type)
		{
			if (extends == null)
				throw new ArgumentNullException("extends");

			return EnumUtils.GetFlagsExceptNone(type)
			                .Select(t =>
			                        {
				                        ConnectorInfo? input = extends.GetInput(output, t);
				                        return input == null
					                               ? (ConnectorInfo?)null
					                               : new ConnectorInfo(((ConnectorInfo)input).Address, t);
			                        })
			                .ExceptNulls();
		}
	}
}
