using System;
using System.Collections.Generic;
using ICD.Connect.Devices;
using ICD.Connect.Routing.Connections;
using ICD.Connect.Routing.EventArguments;

namespace ICD.Connect.Routing.Devices
{
	public interface IRouteSourceDevice : IDevice
	{
		event EventHandler<TransmissionStateEventArgs> OnActiveTransmissionStateChanged;

		IEnumerable<ConnectorInfo> GetOutputs();
		bool GetActiveTransmissionState(int output, eConnectionType type);
	}
}