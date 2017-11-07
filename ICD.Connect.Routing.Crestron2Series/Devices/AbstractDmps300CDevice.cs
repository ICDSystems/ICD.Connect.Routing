using ICD.Connect.Devices;

namespace ICD.Connect.Routing.Crestron2Series.Devices
{
	public abstract class AbstractDmps300CDevice<TSettings> : AbstractDevice<TSettings>, IDmps300CDevice
		where TSettings : IDmps300CDeviceSettings, new()
	{
		public abstract string Address { get; }

		public abstract ushort Port { get; }
	}
}