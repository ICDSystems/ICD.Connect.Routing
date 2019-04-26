using System;
using System.Collections.Generic;
using System.Linq;
using ICD.Common.Utils;
using ICD.Connect.Devices;
using ICD.Connect.Routing.Connections;
using ICD.Connect.Routing.EventArguments;

namespace ICD.Connect.Routing.Devices
{
	public abstract class AbstractRouteMidpointDevice<TSettings> : AbstractRouteDestinationDevice<TSettings>, IRouteMidpointDevice
		where TSettings : IDeviceSettings, new()
	{
		/// <summary>
		/// Called when a route changes.
		/// </summary>
		public abstract event EventHandler<RouteChangeEventArgs> OnRouteChange;

		/// <summary>
		/// Raised when the device starts/stops actively transmitting on an output.
		/// </summary>
		public abstract event EventHandler<TransmissionStateEventArgs> OnActiveTransmissionStateChanged;

		/// <summary>
		/// Returns true if the device is actively transmitting on the given output.
		/// This is NOT the same as sending video, since some devices may send an
		/// idle signal by default.
		/// </summary>
		/// <param name="output"></param>
		/// <param name="type"></param>
		/// <returns></returns>
		public virtual bool GetActiveTransmissionState(int output, eConnectionType type)
		{
			// Returns true if the output is transmitting an input on all flags
			return EnumUtils.GetFlagsExceptNone(type).All(flag => this.GetInputs(output, flag).Any());
		}

		/// <summary>
		/// Gets the output at the given address.
		/// </summary>
		/// <param name="output"></param>
		/// <returns></returns>
		public abstract ConnectorInfo GetOutput(int output);

		/// <summary>
		/// Returns true if the source contains an output at the given address.
		/// </summary>
		/// <param name="output"></param>
		/// <returns></returns>
		public abstract bool ContainsOutput(int output);

		/// <summary>
		/// Returns the outputs.
		/// </summary>
		/// <returns></returns>
		public abstract IEnumerable<ConnectorInfo> GetOutputs();

		/// <summary>
		/// Gets the input routed to the given output matching the given type.
		/// </summary>
		/// <param name="output"></param>
		/// <param name="type"></param>
		/// <returns></returns>
		/// <exception cref="InvalidOperationException">Type has multiple flags.</exception>
		public abstract ConnectorInfo? GetInput(int output, eConnectionType type);

		/// <summary>
		/// Gets the outputs for the given input.
		/// </summary>
		/// <param name="input"></param>
		/// <param name="type"></param>
		/// <returns></returns>
		public abstract IEnumerable<ConnectorInfo> GetOutputs(int input, eConnectionType type);

		/// <summary>
		/// Returns the true if the input is actively being used by the source device.
		/// For example, a display might true if the input is currently on screen,
		/// while a switcher may return true if the input is currently routed.
		/// </summary>
		public override bool GetInputActiveState(int input, eConnectionType type)
		{
			return GetOutputs(input, type).Any();
		}
	}
}
