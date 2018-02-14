namespace ICD.Connect.Routing.Crestron2Series.Devices
{
	public interface IDmps300CComPortDevice : IDmps300CDevice
	{
		ushort ComSpecJoin { get; }
	}
}