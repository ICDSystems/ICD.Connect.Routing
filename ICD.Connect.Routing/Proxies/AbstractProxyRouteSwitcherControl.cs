using System.Collections.Generic;
using ICD.Connect.Devices.Proxies.Devices;
using ICD.Connect.Routing.Connections;
using ICD.Connect.Routing.Controls;

namespace ICD.Connect.Routing.Proxies
{
	public abstract class AbstractProxyRouteSwitcherControl : AbstractProxyRouteMidpointControl, IProxyRouteSwitcherControl
	{
		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="parent"></param>
		/// <param name="id"></param>
		protected AbstractProxyRouteSwitcherControl(IProxyDevice parent, int id)
			: base(parent, id)
		{
		}

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
