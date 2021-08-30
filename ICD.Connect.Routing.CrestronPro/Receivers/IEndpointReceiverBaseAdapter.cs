using ICD.Connect.Misc.CrestronPro.Devices;
#if !NETSTANDARD
using Crestron.SimplSharpPro;
using Crestron.SimplSharpPro.DM;
using Crestron.SimplSharpPro.DM.Endpoints.Receivers;
#endif
using ICD.Connect.Devices;

namespace ICD.Connect.Routing.CrestronPro.Receivers
{
#if !NETSTANDARD
	public delegate void ReceiverChangeCallback(IEndpointReceiverBaseAdapter sender, EndpointReceiverBase midpoint);
#endif

	public interface IEndpointReceiverBaseAdapter : IDevice, IPortParent, IDmEndpoint
	{
#if !NETSTANDARD
		/// <summary>
		/// Raised when the wrapped receiver device changes.
		/// </summary>
		event ReceiverChangeCallback OnReceiverChanged;

		/// <summary>
		/// Gets the currently wrapped receiver device.
		/// </summary>
		EndpointReceiverBase Receiver { get; }
#endif
	}

#if !NETSTANDARD
	public interface IEndpointReceiverBaseAdapter<TReceiver> : IEndpointReceiverBaseAdapter
		where TReceiver : EndpointReceiverBase
	{
		/// <summary>
		/// Gets the currently wrapped receiver device.
		/// </summary>
		new TReceiver Receiver { get; }

		TReceiver InstantiateReceiver(byte ipid, CrestronControlSystem controlSystem);
		TReceiver InstantiateReceiver(byte ipid, DMOutput output);
		TReceiver InstantiateReceiver(DMOutput output);
	}
#endif
}
