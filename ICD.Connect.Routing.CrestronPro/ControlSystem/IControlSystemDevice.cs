using ICD.Connect.Devices;
using ICD.Connect.Telemetry.Attributes;

namespace ICD.Connect.Routing.CrestronPro.ControlSystem
{
	[ExternalTelemetry("ControlSystemInfo", typeof(ControlSystemExternalTelemetryProvider))]
	public interface IControlSystemDevice : IDevice
	{
	}
}