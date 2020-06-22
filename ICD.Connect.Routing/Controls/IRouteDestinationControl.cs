using System;
using System.Collections.Generic;
using System.Linq;
using ICD.Common.Properties;
using ICD.Common.Utils.Extensions;
using ICD.Connect.API.Attributes;
using ICD.Connect.Routing.Connections;
using ICD.Connect.Routing.Endpoints;
using ICD.Connect.Routing.EventArguments;
using ICD.Connect.Routing.Proxies;

namespace ICD.Connect.Routing.Controls
{
	/// <summary>
	/// A destination control is a control with one or more input connectors.
	/// </summary>
	[ApiClass(typeof(ProxyRouteDestinationControl), typeof(IRouteControl))]
	public interface IRouteDestinationControl : IRouteControl
	{
		/// <summary>
		/// Raised when an input source status changes.
		/// </summary>
		[ApiEvent(RouteDestinationControlApi.EVENT_SOURCE_DETECTION_STATE_CHANGE, RouteDestinationControlApi.HELP_EVENT_SOURCE_DETECTION_STATE_CHANGE)]
		event EventHandler<SourceDetectionStateChangeEventArgs> OnSourceDetectionStateChange;

		/// <summary>
		/// Raised when the device starts/stops actively using an input, e.g. unroutes an input.
		/// </summary>
		[ApiEvent(RouteDestinationControlApi.EVENT_ACTIVE_INPUTS_CHANGED, RouteDestinationControlApi.HELP_EVENT_ACTIVE_INPUTS_CHANGED)]
		event EventHandler<ActiveInputStateChangeEventArgs> OnActiveInputsChanged;

		/// <summary>
		/// Returns true if a signal is detected at the given input.
		/// </summary>
		/// <param name="input"></param>
		/// <param name="type"></param>
		/// <returns></returns>
		bool GetSignalDetectedState(int input, eConnectionType type);

		/// <summary>
		/// Returns the true if the input is actively being used by the source device.
		/// For example, a display might true if the input is currently on screen,
		/// while a switcher may return true if the input is currently routed.
		/// </summary>
		bool GetInputActiveState(int input, eConnectionType type);

		/// <summary>
		/// Gets the input at the given address.
		/// </summary>
		/// <param name="input"></param>
		/// <returns></returns>
		ConnectorInfo GetInput(int input);

		/// <summary>
		/// Returns true if the destination contains an input at the given address.
		/// </summary>
		/// <param name="input"></param>
		/// <returns></returns>
		bool ContainsInput(int input);

		/// <summary>
		/// Returns the inputs.
		/// </summary>
		/// <returns></returns>
		IEnumerable<ConnectorInfo> GetInputs();
	}

	/// <summary>
	/// Extension methods for IRouteDestinationDevice
	/// </summary>
	public static class RouteDestinationControlExtensions
	{
		/// <summary>
		/// Gets the endpoint info for the given input.
		/// </summary>
		/// <param name="extends"></param>
		/// <param name="input"></param>
		/// <returns></returns>
		public static EndpointInfo GetInputEndpointInfo([NotNull] this IRouteDestinationControl extends, int input)
		{
			if (extends == null)
				throw new ArgumentNullException("extends");

			return new EndpointInfo(extends.Parent.Id, extends.Id, input);
		}

		/// <summary>
		/// Returns the inputs that are actively being used by the source device.
		/// For example, a display might return the input that is currently on screen,
		/// while a switcher may return the inputs that are currently routed.
		/// </summary>
		public static IEnumerable<ConnectorInfo> GetActiveInputs([NotNull] this IRouteDestinationControl extends, eConnectionType type)
		{
			if (extends == null)
				throw new ArgumentNullException("extends");

			return extends.GetInputs(type).Where(c => extends.GetInputActiveState(c.Address, type));
		}

		/// <summary>
		/// Returns the inputs that have the given type flags.
		/// </summary>
		/// <param name="extends"></param>
		/// <param name="type"></param>
		/// <returns></returns>
		public static IEnumerable<ConnectorInfo> GetInputs([NotNull] this IRouteDestinationControl extends, eConnectionType type)
		{
			if (extends == null)
				throw new ArgumentNullException("extends");

			return extends.GetInputs().Where(o => o.ConnectionType.HasFlags(type));
		}
	}
}
