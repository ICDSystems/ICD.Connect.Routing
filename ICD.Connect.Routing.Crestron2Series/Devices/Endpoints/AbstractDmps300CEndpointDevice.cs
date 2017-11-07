namespace ICD.Connect.Routing.Crestron2Series.Devices.Endpoints
{
    public abstract class AbstractDmps300CEndpointDevice<TSettings> : AbstractDmps300CDevice<TSettings>, IDmps300CEndpointDevice
		where TSettings : IDmps300CEndpointDeviceSettings, new()
    {
	}
}
