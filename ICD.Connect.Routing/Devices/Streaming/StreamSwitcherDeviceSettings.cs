using ICD.Connect.Devices;
using ICD.Connect.Settings.Attributes;

namespace ICD.Connect.Routing.Devices.Streaming
{
	[KrangSettings("StreamSwitcherDevice", typeof(StreamSwitcherDevice))]
	public sealed class StreamSwitcherDeviceSettings : AbstractDeviceSettings
	{
	}
}
