using ICD.Connect.Devices;

namespace ICD.Connect.Routing.CrestronPro.Transmitters
{
#if SIMPLSHARP
	public delegate void TransmitterChangeCallback(
		IEndpointTransmitterBaseAdapter sender,
		Crestron.SimplSharpPro.DM.Endpoints.Transmitters.EndpointTransmitterBase transmitter);
#endif

	public interface IEndpointTransmitterBaseAdapter : IDevice
	{
#if SIMPLSHARP
		/// <summary>
		/// Raised when the wrapped transmitter changes.
		/// </summary>
		event TransmitterChangeCallback OnTransmitterChanged;

		/// <summary>
		/// Gets the wrapped transmitter instance.
		/// </summary>
		Crestron.SimplSharpPro.DM.Endpoints.Transmitters.EndpointTransmitterBase Transmitter { get; }
#endif
	}
}
