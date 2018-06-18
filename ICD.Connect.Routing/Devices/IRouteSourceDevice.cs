using System;
using System.Collections.Generic;
using ICD.Connect.Devices;
using ICD.Connect.Routing.Connections;
using ICD.Connect.Routing.EventArguments;

namespace ICD.Connect.Routing.Devices
{
	public interface IRouteSourceDevice : IDevice
	{
		/// <summary>
		/// Raised when the device starts/stops actively transmitting on an output.
		/// </summary>
		event EventHandler<TransmissionStateEventArgs> OnActiveTransmissionStateChanged;

		/// <summary>
		/// Returns true if the device is actively transmitting on the given output.
		/// This is NOT the same as sending video, since some devices may send an
		/// idle signal by default.
		/// </summary>
		/// <param name="output"></param>
		/// <param name="type"></param>
		/// <returns></returns>
		bool GetActiveTransmissionState(int output, eConnectionType type);

		/// <summary>
		/// Gets the output at the given address.
		/// </summary>
		/// <param name="output"></param>
		/// <returns></returns>
		ConnectorInfo GetOutput(int output);

		/// <summary>
		/// Returns true if the source contains an output at the given address.
		/// </summary>
		/// <param name="output"></param>
		/// <returns></returns>
		bool ContainsOutput(int output);

		/// <summary>
		/// Returns the outputs.
		/// </summary>
		/// <returns></returns>
		IEnumerable<ConnectorInfo> GetOutputs();
	}
}