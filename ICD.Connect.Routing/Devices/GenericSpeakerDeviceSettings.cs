using ICD.Connect.Devices;
using ICD.Connect.Settings.Attributes;

namespace ICD.Connect.Routing.Devices
{
	[KrangSettings("GenericSpeaker", typeof(GenericSpeakerDevice))]
	public sealed class GenericSpeakerDeviceSettings : AbstractDeviceSettings
	{
	}
}
