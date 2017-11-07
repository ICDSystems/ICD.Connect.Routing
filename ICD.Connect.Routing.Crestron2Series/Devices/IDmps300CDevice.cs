using ICD.Connect.Devices;

namespace ICD.Connect.Routing.Crestron2Series.Devices
{
	public interface IDmps300CDevice : IDevice
	{
		string Address { get; }

		ushort Port { get; }
	}
}
