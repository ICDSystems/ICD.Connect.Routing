using ICD.Connect.Devices;
using ICD.Connect.Settings.Attributes;

namespace ICD.Connect.Routing.Devices.Streaming
{
	[KrangSettings("MockStreamDestinationDevice", typeof(MockStreamDestinationDevice))]
	public sealed class MockStreamDestinationDeviceSettings : AbstractDeviceSettings
	{
	}
}
