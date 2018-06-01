using System;
using System.Collections.Generic;
using ICD.Connect.Devices.Proxies.Devices;
using ICD.Connect.Routing.Connections;
using ICD.Connect.Routing.EventArguments;

namespace ICD.Connect.Routing.Proxies
{
	public abstract class AbstractProxyRouteSourceControl : AbstractProxyRouteControl, IProxyRouteSourceControl
	{
		/// <summary>
		/// Raised when the device starts/stops actively transmitting on an output.
		/// </summary>
		public event EventHandler<TransmissionStateEventArgs> OnActiveTransmissionStateChanged;

		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="parent"></param>
		/// <param name="id"></param>
		protected AbstractProxyRouteSourceControl(IProxyDeviceBase parent, int id)
			: base(parent, id)
		{
		}

		/// <summary>
		/// Override to release resources.
		/// </summary>
		/// <param name="disposing"></param>
		protected override void DisposeFinal(bool disposing)
		{
			OnActiveTransmissionStateChanged = null;

			base.DisposeFinal(disposing);
		}

		/// <summary>
		/// Returns true if the device is actively transmitting on the given output.
		/// This is NOT the same as sending video, since some devices may send an
		/// idle signal by default.
		/// </summary>
		/// <param name="output"></param>
		/// <param name="type"></param>
		/// <returns></returns>
		public bool GetActiveTransmissionState(int output, eConnectionType type)
		{
			// TODO
			return false;
		}

		/// <summary>
		/// Returns the outputs.
		/// </summary>
		/// <returns></returns>
		public IEnumerable<ConnectorInfo> GetOutputs()
		{
			// TODO
			yield break;
		}
	}
}
