#if !NETSTANDARD
using Crestron.SimplSharpPro;
#endif
using ICD.Connect.Routing.CrestronPro.DigitalMedia.DmNvx.DmNvxD3X;

namespace ICD.Connect.Routing.CrestronPro.DigitalMedia.DmNvx.DmNvxD30C
{
#if !NETSTANDARD
	public sealed class DmNvxD30CAdapter :
		AbstractDmNvxD3XAdapter<Crestron.SimplSharpPro.DM.Streaming.DmNvxD30C, DmNvxD30CAdapterSettings>
#else
	public sealed class DmNvxD30CAdapter : AbstractDmNvxD3XAdapter<DmNvxD30CAdapterSettings>
#endif
	{
#if !NETSTANDARD
		/// <summary>
		/// Creates a new instance of the wrapped internal switcher.
		/// </summary>
		/// <param name="ethernetId"></param>
		/// <param name="controlSystem"></param>
		/// <returns></returns>
		public override Crestron.SimplSharpPro.DM.Streaming.DmNvxD30C InstantiateStreamer(uint ethernetId, CrestronControlSystem controlSystem)
		{
			return new Crestron.SimplSharpPro.DM.Streaming.DmNvxD30C(ethernetId, controlSystem);
		}

		/// <summary>
		/// Creates a new instance of the wrapped internal switcher.
		/// </summary>
		/// <param name="endpointId"></param>
		/// <param name="domain"></param>
		/// <param name="isReceiver"></param>
		/// <returns></returns>
		public override Crestron.SimplSharpPro.DM.Streaming.DmNvxD30C InstantiateStreamer(uint endpointId, Crestron.SimplSharpPro.DM.Streaming.DmXioDirectorBase.DmXioDomain domain, bool isReceiver)
		{
			return new Crestron.SimplSharpPro.DM.Streaming.DmNvxD30C(endpointId, domain);
		}
#endif
	}
}
