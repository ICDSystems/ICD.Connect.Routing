using System;
using ICD.Connect.Routing.Connections;
using ICD.Connect.Telemetry;

namespace ICD.Connect.Routing.Controls
{
	public abstract class InputOutputPortBase : ITelemetryProvider
	{
		public event EventHandler OnRequestTelemetryRebuild;
		public eConnectionType ConnectionType { get; set; }
	}
}