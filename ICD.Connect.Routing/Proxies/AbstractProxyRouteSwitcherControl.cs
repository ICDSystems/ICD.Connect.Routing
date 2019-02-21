using System;
using System.Collections.Generic;
using System.Linq;
using ICD.Common.Utils.EventArguments;
using ICD.Connect.Devices.Proxies.Devices;
using ICD.Connect.Routing.Connections;

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

		public IEnumerable<string> GetSwitcherVideoInputIds()
		{
			return Enumerable.Empty<string>();
		}

		/// <summary>
		/// Gets the Input Name of the switcher (ie Content, Display In)
		/// </summary>
		/// <returns></returns>
		public IEnumerable<string> GetSwitcherVideoInputNames()
		{
			return Enumerable.Empty<string>();
		}

		/// <summary>
		/// Gets the Input Sync Type of the switcher's inputs (ie HDMI when HDMI Sync is detected, empty when not detected)
		/// </summary>
		/// <returns></returns>
		public IEnumerable<string> GetSwitcherVideoInputSyncType()
		{
			return Enumerable.Empty<string>();
		}

		/// <summary>
		/// Gets the Input Resolution for the switcher's inputs (ie 1920x1080, or empty for no sync)
		/// </summary>
		/// <returns></returns>
		public IEnumerable<string> GetSwitcherVideoInputResolutions()
		{
			return Enumerable.Empty<string>();
		}

		/// <summary>
		/// Gets the Output Ids of the switcher's outputs (ie HDMI1, VGA2)
		/// </summary>
		/// <returns></returns>
		public IEnumerable<string> GetSwitcherVideoOutputIds()
		{
			return Enumerable.Empty<string>();
		}

		/// <summary>
		/// Gets the Output Name of the switcher's outputs (ie Content, Display In)
		/// </summary>
		/// <returns></returns>
		public IEnumerable<string> GetSwitcherVideoOutputNames()
		{
			return Enumerable.Empty<string>();
		}
	}
}
