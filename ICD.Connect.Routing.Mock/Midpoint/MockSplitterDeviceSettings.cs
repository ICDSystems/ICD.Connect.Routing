using ICD.Connect.Devices;
using ICD.Connect.Settings.Attributes;

namespace ICD.Connect.Routing.Mock.Midpoint
{
	[KrangSettings("MockSplitterDevice", typeof(MockSplitterDevice))]
	public sealed class MockSplitterDeviceSettings : AbstractDeviceSettings
	{
	}
}