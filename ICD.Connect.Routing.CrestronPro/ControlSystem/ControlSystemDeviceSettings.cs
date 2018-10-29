using ICD.Connect.Devices;
using ICD.Connect.Settings.Attributes;

namespace ICD.Connect.Routing.CrestronPro.ControlSystem
{
	[KrangSettings("ControlSystem", typeof(ControlSystemDevice))]
	public sealed class ControlSystemDeviceSettings : AbstractDeviceSettings
	{
	}
}
