using ICD.Connect.Devices.Mock;
using ICD.Connect.Settings.Attributes;

namespace ICD.Connect.Routing.Mock.Midpoint
{
	[KrangSettings("MockSplitterDevice", typeof(MockSplitterDevice))]
	public sealed class MockSplitterDeviceSettings : AbstractMockDeviceSettings
	{
	}
}