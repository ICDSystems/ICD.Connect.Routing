using ICD.Connect.Devices.Mock;
using ICD.Connect.Settings.Attributes;

namespace ICD.Connect.Routing.Mock.Destination
{
	/// <summary>
	/// Settings for the MockDestinationDevice.
	/// </summary>
	[KrangSettings("MockDestinationDevice", typeof(MockDestinationDevice))]
	public sealed class MockDestinationDeviceSettings : AbstractMockDeviceSettings
	{
	}
}
