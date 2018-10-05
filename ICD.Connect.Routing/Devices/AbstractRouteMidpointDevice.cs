using System;
using System.Collections.Generic;
using System.Linq;
using ICD.Connect.Devices;
using ICD.Connect.Routing.Connections;
using ICD.Connect.Routing.EventArguments;

namespace ICD.Connect.Routing.Devices
{
	public abstract class AbstractRouteMidpointDevice<TSettings> : AbstractRouteDestinationDevice<TSettings>, IRouteMidpointDevice
		where TSettings : IDeviceSettings, new()
	{
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
		public abstract bool GetActiveTransmissionState(int output, eConnectionType type);

		/// <summary>
		/// Gets the output at the given address.
		/// </summary>
		/// <param name="output"></param>
		/// <returns></returns>
		public ConnectorInfo GetOutput(int output)
		{
			if (ContainsOutput(output))
				return GetOutputs().First(c => c.Address == output);

			string message = string.Format("{0} has no output at address {1}", this, output);
			throw new ArgumentOutOfRangeException("output", message);
		}

		/// <summary>
		/// Returns true if the source contains an output at the given address.
		/// </summary>
		/// <param name="output"></param>
		/// <returns></returns>
		public bool ContainsOutput(int output)
		{
			return GetOutputs().Any(o => o.Address == output);
		}

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
	}
}
