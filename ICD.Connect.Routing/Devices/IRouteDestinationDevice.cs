using System;
using System.Collections.Generic;
using ICD.Connect.Devices;
using ICD.Connect.Routing.Connections;
using ICD.Connect.Routing.EventArguments;

namespace ICD.Connect.Routing.Devices
{
	public interface IRouteDestinationDevice : IDevice
	{
		event EventHandler<SourceDetectionStateChangeEventArgs> OnSourceDetectionStateChange;
		event EventHandler<ActiveInputStateChangeEventArgs> OnActiveInputsChanged;

		bool GetSignalDetectedState(int input, eConnectionType type);
		bool GetInputActiveState(int input, eConnectionType type);

		ConnectorInfo GetInput(int input);
		IEnumerable<ConnectorInfo> GetInputs();

		bool GetActiveTransmissionState(int input, eConnectionType type);
	}
}