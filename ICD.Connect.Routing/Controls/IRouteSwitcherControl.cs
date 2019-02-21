using System;
using System.Collections;
using System.Collections.Generic;
using ICD.Common.Properties;
using ICD.Common.Utils.EventArguments;
using ICD.Connect.API.Attributes;
using ICD.Connect.Routing.Connections;
using ICD.Connect.Routing.Proxies;

namespace ICD.Connect.Routing.Controls
{
	/// <summary>
	/// An IRouteSwitcherControl has inputs and outputs, and provides methods
	/// for routing specific inputs to specific outputs.
	/// </summary>
	[ApiClass(typeof(ProxyRouteSwitcherControl), typeof(IRouteMidpointControl))]
	public interface IRouteSwitcherControl : IRouteMidpointControl
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

		/// <summary>
		/// Gets the Input Id of the switcher's inputs (ie HDMI1, VGA2)
		/// </summary>
		/// <returns></returns>
		[PublicAPI]
		IEnumerable<string> GetSwitcherVideoInputIds();

		/// <summary>
		/// Gets the Input Name of the switcher's inputs (ie Content, Display In)
		/// </summary>
		/// <returns></returns>
		[PublicAPI]
		IEnumerable<string> GetSwitcherVideoInputNames();

		/// <summary>
		/// Gets the Input Sync Type of the switcher's inputs (ie HDMI when HDMI Sync is detected, empty when not detected)
		/// </summary>
		/// <returns></returns>
		[PublicAPI]
		IEnumerable<string> GetSwitcherVideoInputSyncType();

		/// <summary>
		/// Gets the Input Resolution for the switcher's inputs (ie 1920x1080, or empty for no sync)
		/// </summary>
		/// <returns></returns>
		[PublicAPI]
		IEnumerable<string> GetSwitcherVideoInputResolutions();

		/// <summary>
		/// Gets the Output Ids of the switcher's outputs (ie HDMI1, VGA2)
		/// </summary>
		/// <returns></returns>
		[PublicAPI]
		IEnumerable<string> GetSwitcherVideoOutputIds();

		/// <summary>
		/// Gets the Output Name of the switcher's outputs (ie Content, Display In)
		/// </summary>
		/// <returns></returns>
		[PublicAPI]
		IEnumerable<string> GetSwitcherVideoOutputNames();
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
