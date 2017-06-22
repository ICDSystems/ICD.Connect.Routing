using System;
using System.Collections.Generic;
using System.Linq;
using ICD.Common.Properties;
using ICD.Common.Utils;
using ICD.Common.Utils.Extensions;
using ICD.Connect.Routing.Connections;

namespace ICD.Connect.Routing.Controls
{
	public interface IRouteMidpointControl : IRouteSourceControl, IRouteDestinationControl
	{
		/// <summary>
		/// Gets the inputs routed to the given output matching the given type.
		/// </summary>
		/// <param name="output"></param>
		/// <param name="type"></param>
		/// <returns></returns>
		/// <exception cref="InvalidOperationException">Type has multiple flags and the result is not unanimous.</exception>
		IEnumerable<ConnectorInfo> GetInputs(int output, eConnectionType type);
	}

	public static class RouteMidpointControlExtensions
	{
		/// <summary>
		/// Gets the input for the given output.
		/// </summary>
		/// <param name="extends"></param>
		/// <param name="output"></param>
		/// <returns></returns>
		[PublicAPI]
		public static IEnumerable<ConnectorInfo> GetInputs(this IRouteMidpointControl extends, int output)
		{
			if (extends == null)
				throw new ArgumentNullException("extends");

			return extends.GetInputs(output, EnumUtils.GetFlagsAllValue<eConnectionType>());
		}

		/// <summary>
		/// Gets the input for the given output and type.
		/// </summary>
		/// <param name="extends"></param>
		/// <param name="output"></param>
		/// <param name="type"></param>
		/// <returns></returns>
		/// <exception cref="InvalidOperationException">Type has multiple flags and the result is not unanimous.</exception>
		public static ConnectorInfo? GetInput(this IRouteMidpointControl extends, int output, eConnectionType type)
		{
			if (extends == null)
				throw new ArgumentNullException("extends");

			ConnectorInfo[] inputs = extends.GetInputs(output, type).ToArray();

			if (inputs.Length == 0)
				return null;
			if (inputs.Unanimous())
				return inputs.First();

			string message = string.Format("Unable to get single Input for {0} Output {1} with connection types {2}",
			                               extends, output, type);
			throw new InvalidOperationException(message);
		}

		/// <summary>
		/// Gets the outputs for the given input.
		/// </summary>
		/// <param name="extends"></param>
		/// <param name="input"></param>
		/// <param name="type"></param>
		/// <returns></returns>
		[PublicAPI]
		public static IEnumerable<ConnectorInfo> GetOutputs(this IRouteMidpointControl extends, int input,
		                                                    eConnectionType type)
		{
			if (extends == null)
				throw new ArgumentNullException("extends");

			return extends.GetOutputs(type).Where(o => extends.GetInputs(o.Address)
			                                                  .Any(i => i.Address == input));
		}
	}
}
