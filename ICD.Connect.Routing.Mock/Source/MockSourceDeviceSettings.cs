using ICD.Connect.Devices;
using ICD.Connect.Settings.Attributes;

namespace ICD.Connect.Routing.Mock.Source
{
	/// <summary>
	/// Settings for the MockSourceDevice.
	/// </summary>
	[KrangSettings("MockSourceDevice", typeof(MockSourceDevice))]
	public sealed class MockSourceDeviceSettings : AbstractDeviceSettings
	{
	}
}
