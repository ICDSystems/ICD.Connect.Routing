#if !NETSTANDARD
using Crestron.SimplSharpPro;
#endif
using ICD.Connect.Routing.CrestronPro.DigitalMedia.DmNvx.DmNvxD3X;

namespace ICD.Connect.Routing.CrestronPro.DigitalMedia.DmNvx.DmNvxD80Ioav
{
#if !NETSTANDARD
	public sealed class DmNvxD80IoavAdapter :
		AbstractDmNvxD3XAdapter<Crestron.SimplSharpPro.DM.Streaming.DmNvxD80Ioav, DmNvxD80IoavAdapterSettings>
#else
	public sealed class DmNvxD80IoavAdapter : AbstractDmNvxD3XAdapter<DmNvxD80IoavAdapterSettings>
#endif
	{
#if !NETSTANDARD
		/// <summary>
		/// Creates a new instance of the wrapped internal switcher.
		/// </summary>
		/// <param name="ethernetId"></param>
		/// <param name="controlSystem"></param>
		/// <returns></returns>
		public override Crestron.SimplSharpPro.DM.Streaming.DmNvxD80Ioav InstantiateStreamer(uint ethernetId, CrestronControlSystem controlSystem)
		{
			return new Crestron.SimplSharpPro.DM.Streaming.DmNvxD80Ioav(ethernetId, controlSystem);
		}

		/// <summary>
		/// Creates a new instance of the wrapped internal switcher.
		/// </summary>
		/// <param name="endpointId"></param>
		/// <param name="domain"></param>
		/// <param name="isReceiver"></param>
		/// <returns></returns>
		public override Crestron.SimplSharpPro.DM.Streaming.DmNvxD80Ioav InstantiateStreamer(uint endpointId, Crestron.SimplSharpPro.DM.Streaming.DmXioDirectorBase.DmXioDomain domain, bool isReceiver)
		{
			return new Crestron.SimplSharpPro.DM.Streaming.DmNvxD80Ioav(endpointId, domain);
		}
#endif
	}
}
