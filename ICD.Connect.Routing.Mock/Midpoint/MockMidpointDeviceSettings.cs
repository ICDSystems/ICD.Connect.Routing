using ICD.Connect.Devices.Mock;
using ICD.Connect.Settings.Attributes;

namespace ICD.Connect.Routing.Mock.Midpoint
{
	[KrangSettings("MockMidpointDevice", typeof(MockMidpointDevice))]
	public sealed class MockMidpointDeviceSettings : AbstractMockDeviceSettings
	{
	}
}
