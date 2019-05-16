using System;
using System.Collections.Generic;
using ICD.Common.Utils.EventArguments;
using ICD.Connect.Devices.Proxies.Devices;
using ICD.Connect.Routing.Connections;
using ICD.Connect.Routing.Controls;
using ICD.Connect.Routing.EventArguments;

namespace ICD.Connect.Routing.Proxies
{
	public abstract class AbstractProxyRouteSwitcherControl : AbstractProxyRouteMidpointControl, IProxyRouteSwitcherControl
	{
		public event EventHandler<BoolEventArgs> OnAudioBreakawayEnabledChanged;
		public event EventHandler<BoolEventArgs> OnUsbBreakawayEnabledChanged;

		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="parent"></param>
		/// <param name="id"></param>
		protected AbstractProxyRouteSwitcherControl(IProxyDeviceBase parent, int id)
			: base(parent, id)
		{
		}

		/// <summary>
		/// Describes whether a switcher is breaking away audio.
		/// </summary>
		public bool AudioBreakawayEnabled { get { return false; } }

		/// <summary>
		/// Describes whether a switcher is breaking away USB.
		/// </summary>
		public bool UsbBreakawayEnabled { get { return false; } }

		/// <summary>
		/// Returns switcher port objects to get details about the input ports on this switcher.
		/// </summary>
		/// <returns></returns>
		public IEnumerable<InputPort> GetInputPorts()
		{
			yield break;
		}

		/// <summary>
		/// Returns switcher port objects to get details about the output ports on this switcher.
		/// </summary>
		/// <returns></returns>
		public IEnumerable<OutputPort> GetOutputPorts()
		{
			yield break;
		}

		/// <summary>
		/// Performs the given route operation.
		/// </summary>
		/// <param name="info"></param>
		/// <returns></returns>
		public bool Route(RouteOperation info)
		{
			// TODO
			return true;
		}

		/// <summary>
		/// Stops routing to the given output.
		/// </summary>
		/// <param name="output"></param>
		/// <param name="type"></param>
		/// <returns>True if successfully cleared.</returns>
		public bool ClearOutput(int output, eConnectionType type)
		{
			// TODO
			return true;
		}
	}
}
