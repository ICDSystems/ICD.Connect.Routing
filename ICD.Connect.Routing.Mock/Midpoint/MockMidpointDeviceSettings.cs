using ICD.Connect.Devices;
using ICD.Connect.Settings.Attributes;

namespace ICD.Connect.Routing.Mock.Midpoint
{
	[KrangSettings("MockMidpointDevice", typeof(MockMidpointDevice))]
	public sealed class MockMidpointDeviceSettings : AbstractDeviceSettings
	{
	}
}
