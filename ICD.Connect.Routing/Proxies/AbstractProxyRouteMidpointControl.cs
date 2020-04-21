using System;
using System.Collections.Generic;
using ICD.Common.Utils.EventArguments;
using ICD.Connect.Devices.Proxies.Devices;
using ICD.Connect.Routing.Connections;
using ICD.Connect.Routing.EventArguments;

namespace ICD.Connect.Routing.Proxies
{
	public abstract class AbstractProxyRouteMidpointControl : AbstractProxyRouteDestinationControl,
	                                                          IProxyRouteMidpointControl
	{
		public event EventHandler<BoolEventArgs> OnAudioBreakawayEnabledChanged;
		public event EventHandler<BoolEventArgs> OnUsbBreakawayEnabledChanged;

		/// <summary>
		/// Raised when a route changes.
		/// </summary>
		public event EventHandler<RouteChangeEventArgs> OnRouteChange;

		public event EventHandler<TransmissionStateEventArgs> OnActiveTransmissionStateChanged;

		/// <summary>
		/// Describes whether a switcher is breaking away audio.
		/// </summary>
		public bool AudioBreakawayEnabled { get { return false; } }

		/// <summary>
		/// Describes whether a switcher is breaking away USB.
		/// </summary>
		public bool UsbBreakawayEnabled { get { return false; } }

		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="parent"></param>
		/// <param name="id"></param>
		protected AbstractProxyRouteMidpointControl(IProxyDevice parent, int id)
			: base(parent, id)
		{
		}

		/// <summary>
		/// Override to release resources.
		/// </summary>
		/// <param name="disposing"></param>
		protected override void DisposeFinal(bool disposing)
		{
			OnRouteChange = null;
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
		/// Gets the output at the given address.
		/// </summary>
		/// <param name="output"></param>
		/// <returns></returns>
		public ConnectorInfo GetOutput(int output)
		{
			// TODO
			throw new ArgumentOutOfRangeException("output");
		}

		/// <summary>
		/// Returns true if the source contains an output at the given address.
		/// </summary>
		/// <param name="output"></param>
		/// <returns></returns>
		public bool ContainsOutput(int output)
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

		/// <summary>
		/// Gets the input routed to the given output matching the given type.
		/// </summary>
		/// <param name="output"></param>
		/// <param name="type"></param>
		/// <returns></returns>
		/// <exception cref="InvalidOperationException">Type has multiple flags.</exception>
		public ConnectorInfo? GetInput(int output, eConnectionType type)
		{
			// TODO
			return null;
		}

		/// <summary>
		/// Gets the outputs for the given input.
		/// </summary>
		/// <param name="input"></param>
		/// <param name="type"></param>
		/// <returns></returns>
		public IEnumerable<ConnectorInfo> GetOutputs(int input, eConnectionType type)
		{
			// TODO
			yield break;
		}
	}
}
