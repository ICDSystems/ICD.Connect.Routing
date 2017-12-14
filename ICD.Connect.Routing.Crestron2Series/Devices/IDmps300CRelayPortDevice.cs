namespace ICD.Connect.Routing.Crestron2Series.Devices
{
	public interface IDmps300CRelayPortDevice : IDmps300CDevice
	{
        /// <summary>
        /// Gets the xsig start join for relay outputs
        /// </summary>
		ushort RelayOutputStartJoin { get; }
	}
}