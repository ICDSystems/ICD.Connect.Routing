using System.Collections.Generic;
using ICD.Connect.Routing.Controls;
using ICD.Connect.Telemetry.Attributes;
using ICD.Connect.Telemetry.Providers.External;

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