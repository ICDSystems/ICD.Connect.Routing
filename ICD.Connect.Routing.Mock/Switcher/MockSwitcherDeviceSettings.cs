using ICD.Connect.Devices;
using ICD.Connect.Settings.Attributes;

namespace ICD.Connect.Routing.Mock.Switcher
{
	[KrangSettings("MockSwitcherDevice", typeof(MockSwitcherDevice))]
	public sealed class MockSwitcherDeviceSettings : AbstractDeviceSettings
	{
	}
}
