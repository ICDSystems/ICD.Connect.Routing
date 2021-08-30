#if !NETSTANDARD
using Crestron.SimplSharpPro;
#endif
using ICD.Connect.Routing.CrestronPro.DigitalMedia.DmNvx.DmNvx35X;

namespace ICD.Connect.Routing.CrestronPro.DigitalMedia.DmNvx.DmNvx350
{
#if !NETSTANDARD
	public sealed class DmNvx350Adapter :
		AbstractDmNvx35XAdapter<Crestron.SimplSharpPro.DM.Streaming.DmNvx350, DmNvx350AdapterSettings>
#else
	public sealed class DmNvx350Adapter : AbstractDmNvx35XAdapter<DmNvx350AdapterSettings>
#endif
	{

#if !NETSTANDARD
		/// <summary>
		/// Creates a new instance of the wrapped internal switcher.
		/// </summary>
		/// <param name="ethernetId"></param>
		/// <param name="controlSystem"></param>
		/// <returns></returns>
		public override Crestron.SimplSharpPro.DM.Streaming.DmNvx350 InstantiateStreamer(uint ethernetId, CrestronControlSystem controlSystem)
		{
			return new Crestron.SimplSharpPro.DM.Streaming.DmNvx350(ethernetId, controlSystem);
		}

		/// <summary>
		/// Creates a new instance of the wrapped internal switcher.
		/// </summary>
		/// <param name="endpointId"></param>
		/// <param name="domain"></param>
		/// <param name="isReceiver"></param>
		/// <returns></returns>
		public override Crestron.SimplSharpPro.DM.Streaming.DmNvx350 InstantiateStreamer(uint endpointId, Crestron.SimplSharpPro.DM.Streaming.DmXioDirectorBase.DmXioDomain domain, bool isReceiver)
		{
			return new Crestron.SimplSharpPro.DM.Streaming.DmNvx350(endpointId, domain, isReceiver);
		}
#endif
	}
}
