using System;
using System.Linq;
using ICD.Common.Properties;
using ICD.Common.Utils;
using ICD.Connect.Routing.Connections;
using ICD.Connect.Routing.EventArguments;

namespace ICD.Connect.Routing.Controls
{
	/// <summary>
	/// An IRouteSwitcherControl has inputs and outputs, and provides methods
	/// for routing specific inputs to specific outputs.
	/// </summary>
	[PublicAPI]
	public interface IRouteSwitcherControl : IRouteMidpointControl
	{
		/// <summary>
		/// Called when a route changes.
		/// </summary>
		event EventHandler<RouteChangeEventArgs> OnRouteChange;

		bool Route(RouteOperation info);

		/// <summary>
		/// Stops routing to the given output.
		/// </summary>
		/// <param name="output"></param>
		/// <param name="type"></param>
		/// <returns>True if successfully cleared.</returns>
		[PublicAPI]
		bool ClearOutput(int output, eConnectionType type);
	}

	/// <summary>
	/// Extension methods for IRouteSwitcherControls.
	/// </summary>
	[PublicAPI]
	public static class RouteSwitcherControlExtensions
	{
		/// <summary>
		/// Unroutes all.
		/// </summary>
		[PublicAPI]
		public static void Clear(this IRouteSwitcherControl extends)
		{
			foreach (ConnectorInfo input in extends.GetInputs())
			{
				foreach (eConnectionType type in EnumUtils.GetFlagsExceptNone(input.ConnectionType))
					extends.ClearInput(input.Address, type);
			}
		}

		/// <summary>
		/// Stops routing from the given input.
		/// </summary>
		/// <param name="extends"></param>
		/// <param name="input"></param>
		/// <param name="type"></param>
		/// <returns>True if the route changed.</returns>
		[PublicAPI]
		public static bool ClearInput(this IRouteSwitcherControl extends, int input, eConnectionType type)
		{
			return extends.GetOutputs(input, type)
			              .Select(o => extends.ClearOutput(o.Address, type))
			              .ToArray()
			              .Any();
		}

		/// <summary>
		/// Routes the input to the given output.
		/// </summary>
		/// <param name="device"></param>
		/// <param name="input"></param>
		/// <param name="output"></param>
		/// <param name="type"></param>
		/// <returns>True if the route changed.</returns>
		[PublicAPI]
		public static bool Route(this IRouteSwitcherControl device, int input, int output, eConnectionType type)
		{
			return device.Route(new RouteOperation {ConnectionType = type, LocalInput = input, LocalOutput = output});
		}
	}
}
