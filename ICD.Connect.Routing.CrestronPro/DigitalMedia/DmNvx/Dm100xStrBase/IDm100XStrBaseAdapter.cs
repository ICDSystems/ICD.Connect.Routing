#if SIMPLSHARP
using Crestron.SimplSharpPro;
#endif
using ICD.Connect.Devices;
using ICD.Connect.Misc.CrestronPro.Devices;

namespace ICD.Connect.Routing.CrestronPro.DigitalMedia.Dm100xStrBase
{
#if SIMPLSHARP
	public delegate void Dm100XStrBaseChangeCallback(
		IDm100XStrBaseAdapter sender, Crestron.SimplSharpPro.DM.Streaming.Dm100xStrBase streamer);
#endif

	public interface IDm100XStrBaseAdapter : IDevice, IPortParent
	{
#if SIMPLSHARP
		/// <summary>
		/// Raised when the wrapped streamer instance changes.
		/// </summary>
		event Dm100XStrBaseChangeCallback OnStreamerChanged;

		/// <summary>
		/// Gets the wrapped streamer instance.
		/// </summary>
		Crestron.SimplSharpPro.DM.Streaming.Dm100xStrBase Streamer { get; }
#endif
	}

#if SIMPLSHARP
	public interface IDm100XStrBaseAdapter<TStreamer> : IDm100XStrBaseAdapter
		where TStreamer : Crestron.SimplSharpPro.DM.Streaming.Dm100xStrBase
	{
		/// <summary>
		/// Gets the wrapped streamer instance.
		/// </summary>
		new TStreamer Streamer { get; }

		/// <summary>
		/// Creates a new instance of the wrapped internal switcher.
		/// </summary>
		/// <param name="ethernetId"></param>
		/// <param name="controlSystem"></param>
		/// <returns></returns>
		TStreamer InstantiateStreamer(uint ethernetId, CrestronControlSystem controlSystem);

		/// <summary>
		/// Creates a new instance of the wrapped internal switcher.
		/// </summary>
		/// <param name="endpointId"></param>
		/// <param name="domain"></param>
		/// <param name="isReceiver"></param>
		/// <returns></returns>
		TStreamer InstantiateStreamer(uint endpointId,
		                              Crestron.SimplSharpPro.DM.Streaming.DmXioDirectorBase.DmXioDomain domain,
		                              bool isReceiver);
	}
#endif
}
