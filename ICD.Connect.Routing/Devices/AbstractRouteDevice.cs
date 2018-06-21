using ICD.Connect.Devices;

namespace ICD.Connect.Routing.Devices
{
	public abstract class AbstractRouteDevice<TSettings> : AbstractDevice<TSettings>, IRouteDevice
		where TSettings : IDeviceSettings, new()
	{
	}
}
