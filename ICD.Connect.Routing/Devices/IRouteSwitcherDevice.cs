using System;
using ICD.Common.Properties;
using ICD.Connect.Routing.Connections;
using ICD.Connect.Routing.EventArguments;

namespace ICD.Connect.Routing.Devices
{
	public interface IRouteSwitcherDevice : IRouteMidpointDevice
	{
		/// <summary>
		/// Called when a route changes.
		/// </summary>
		event EventHandler<RouteChangeEventArgs> OnRouteChange;

		/// <summary>
		/// Performs the given route operation.
		/// </summary>
		/// <param name="info"></param>
		/// <returns></returns>
		bool Route(RouteOperation info);

		/// <summary>
		/// Stops routing to the given output.
		/// </summary>
		/// <param name="output"></param>
		/// <param name="type"></param>
		/// <returns>True if successfully cleared.</returns>
		bool ClearOutput(int output, eConnectionType type);
	}

	public static class RouteSwitcherDeviceExtensions
	{
		/// <summary>
		/// Unroutes all.
		/// </summary>
		[PublicAPI]
		public static void Clear(this IRouteSwitcherDevice extends)
		{
			if (extends == null)
				throw new ArgumentNullException("extends");

			foreach (ConnectorInfo output in extends.GetOutputs())
				extends.ClearOutput(output.Address, output.ConnectionType);
		}

		/// <summary>
		/// Stops routing from the given input.
		/// </summary>
		/// <param name="extends"></param>
		/// <param name="input"></param>
		/// <param name="type"></param>
		/// <returns>True if the route changed.</returns>
		[PublicAPI]
		public static bool ClearInput(this IRouteSwitcherDevice extends, int input, eConnectionType type)
		{
			if (extends == null)
				throw new ArgumentNullException("extends");

			bool result = false;
			foreach (ConnectorInfo item in extends.GetOutputs(input, type))
				result |= extends.ClearOutput(item.Address, type);

			return result;
		}

		/// <summary>
		/// Routes the input to the given output.
		/// </summary>
		/// <param name="extends"></param>
		/// <param name="input"></param>
		/// <param name="output"></param>
		/// <param name="type"></param>
		/// <returns>True if the route changed.</returns>
		[PublicAPI]
		public static bool Route(this IRouteSwitcherDevice extends, int input, int output, eConnectionType type)
		{
			if (extends == null)
				throw new ArgumentNullException("extends");

			return extends.Route(new RouteOperation { ConnectionType = type, LocalInput = input, LocalOutput = output });
		}
	}
}
