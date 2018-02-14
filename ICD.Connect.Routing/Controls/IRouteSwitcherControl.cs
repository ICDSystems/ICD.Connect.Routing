using System;
using ICD.Common.Properties;
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
		public static bool ClearInput(this IRouteSwitcherControl extends, int input, eConnectionType type)
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
		public static bool Route(this IRouteSwitcherControl extends, int input, int output, eConnectionType type)
		{
			if (extends == null)
				throw new ArgumentNullException("extends");

			return extends.Route(new RouteOperation {ConnectionType = type, LocalInput = input, LocalOutput = output});
		}
	}
}
