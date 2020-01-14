using ICD.Connect.Devices;
using ICD.Connect.Settings.Attributes;

namespace ICD.Connect.Routing.Devices.GenericSpeaker
{
	[KrangSettings("GenericSpeaker", typeof(GenericSpeakerDevice))]
	public sealed class GenericSpeakerDeviceSettings : AbstractDeviceSettings
	{
	}
}
