using System;
using System.Collections.Generic;
using System.Linq;
using ICD.Common.Properties;
using ICD.Common.Utils;
using ICD.Common.Utils.EventArguments;
using ICD.Common.Utils.Extensions;
using ICD.Connect.API.Attributes;
using ICD.Connect.Routing.Connections;
using ICD.Connect.Routing.EventArguments;
using ICD.Connect.Routing.Proxies;

namespace ICD.Connect.Routing.Controls
{
	[ApiClass(typeof(ProxyRouteMidpointControl), typeof(IRouteSourceControl))]
	public interface IRouteMidpointControl : IRouteSourceControl, IRouteDestinationControl
	{
		/// <summary>
		/// Raised when the switcher enables or disables audio breakaway.
		/// </summary>
		event EventHandler<BoolEventArgs> OnAudioBreakawayEnabledChanged;

		/// <summary>
		/// Raised when the switcher enables or disables USB breakaway.
		/// </summary>
		event EventHandler<BoolEventArgs> OnUsbBreakawayEnabledChanged;

		/// <summary>
		/// Raised when a route changes.
		/// </summary>
		event EventHandler<RouteChangeEventArgs> OnRouteChange;

		/// <summary>
		/// Describes whether a switcher is breaking away audio.
		/// </summary>
		[PublicAPI]
		bool AudioBreakawayEnabled { get; }

		/// <summary>
		/// Describes whether a switcher is breaking away USB.
		/// </summary>
		[PublicAPI]
		bool UsbBreakawayEnabled { get; }

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

	public static class RouteMidpointControlExtensions
	{
		/// <summary>
		/// Gets the inputs for the given output.
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
		/// Gets the input for the given output.
		/// </summary>
		/// <param name="extends"></param>
		/// <param name="output"></param>
		/// <param name="type"></param>
		/// <returns></returns>
		public static IEnumerable<ConnectorInfo> GetInputs(this IRouteMidpointControl extends, int output,
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
