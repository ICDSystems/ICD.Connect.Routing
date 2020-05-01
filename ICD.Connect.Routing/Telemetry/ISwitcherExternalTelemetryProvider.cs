using System;
using System.Collections.Generic;
using ICD.Common.Utils.EventArguments;
using ICD.Connect.Devices;
using ICD.Connect.Routing.Controls;
using ICD.Connect.Telemetry;
using ICD.Connect.Telemetry.Attributes;

namespace ICD.Connect.Routing.Telemetry
{
	public interface ISwitcherExternalTelemetryProvider : IExternalTelemetryProvider
	{
		[CollectionTelemetry(SwitcherTelemetryNames.INPUT_PORTS)]
		IEnumerable<InputPort> SwitcherInputPorts { get; }

		[CollectionTelemetry(SwitcherTelemetryNames.OUTPUT_PORTS)]
		IEnumerable<OutputPort> SwitcherOutputPorts { get; }
	}
}