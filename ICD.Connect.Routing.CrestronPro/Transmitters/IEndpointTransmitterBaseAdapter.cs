using ICD.Connect.Misc.CrestronPro.Devices;
using ICD.Connect.Routing.Devices;
#if SIMPLSHARP
using Crestron.SimplSharpPro;
using Crestron.SimplSharpPro.DM;
using Crestron.SimplSharpPro.DM.Endpoints.Transmitters;
#endif

namespace ICD.Connect.Routing.CrestronPro.Transmitters
{
#if SIMPLSHARP
	public delegate void TransmitterChangeCallback(IEndpointTransmitterBaseAdapter sender, EndpointTransmitterBase transmitter);
#endif

	public interface IEndpointTransmitterBaseAdapter : IRouteSourceDevice, IPortParent, IDmEndpoint
	{
#if SIMPLSHARP
		/// <summary>
		/// Raised when the wrapped transmitter changes.
		/// </summary>
		event TransmitterChangeCallback OnTransmitterChanged;

		/// <summary>
		/// Gets the wrapped transmitter instance.
		/// </summary>
		EndpointTransmitterBase Transmitter { get; }
#endif
	}

#if SIMPLSHARP
	public interface IEndpointTransmitterBaseAdapter<TTransmitter> : IEndpointTransmitterBaseAdapter
		where TTransmitter : EndpointTransmitterBase
	{
		/// <summary>
		/// Gets the wrapped transmitter instance.
		/// </summary>
		new TTransmitter Transmitter { get; }

		TTransmitter InstantiateTransmitter(byte ipid, CrestronControlSystem controlSystem);
		TTransmitter InstantiateTransmitter(byte ipid, DMInput input);
		TTransmitter InstantiateTransmitter(DMInput input);
	}
#endif
}
