using ICD.Connect.Devices;

namespace ICD.Connect.Routing.CrestronPro.Transmitters.DmTx200Base
{
#if SIMPLSHARP
	public delegate void TransmitterChangeCallback(
		IDmTx200BaseAdapter sender, Crestron.SimplSharpPro.DM.Endpoints.Transmitters.DmTx200Base transmitter);
#endif

	public interface IDmTx200BaseAdapter : IDevice
	{
#if SIMPLSHARP
		/// <summary>
		/// Raised when the wrapped transmitter changes.
		/// </summary>
		event TransmitterChangeCallback OnTransmitterChanged;

		/// <summary>
		/// Gets the wrapped transmitter instance.
		/// </summary>
		Crestron.SimplSharpPro.DM.Endpoints.Transmitters.DmTx200Base Transmitter { get; }
#endif
	}
}
