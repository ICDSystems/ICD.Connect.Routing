namespace ICD.Connect.Routing.Crestron2Series.Devices
{
	public interface IDmps300CDigitalInputPortDevice : IDmps300CDevice
	{
        /// <summary>
        /// Gets the xsig start join for digital inputs
        /// </summary>
		ushort DigitalInputStartJoin { get; }
	}
}