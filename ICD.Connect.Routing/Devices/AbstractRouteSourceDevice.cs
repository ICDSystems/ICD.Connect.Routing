using System;
using System.Collections.Generic;
using ICD.Connect.Devices;
using ICD.Connect.Routing.Connections;
using ICD.Connect.Routing.Controls;
using ICD.Connect.Routing.EventArguments;

namespace ICD.Connect.Routing.Devices
{
	public abstract class AbstractRouteSourceDevice<TSettings> : AbstractDevice<TSettings>, IRouteSourceDevice
		where TSettings : IDeviceSettings, new()
	{
		/// <summary>
		/// Raised when the device starts/stops actively transmitting on an output.
		/// </summary>
		public abstract event EventHandler<TransmissionStateEventArgs> OnActiveTransmissionStateChanged;

		/// <summary>
		/// Constructor.
		/// </summary>
		protected AbstractRouteSourceDevice()
		{
			Controls.Add(new RouteSourceControl(this, 0));
		}

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
	}
}