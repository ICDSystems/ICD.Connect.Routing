#if !NETSTANDARD
using Crestron.SimplSharpPro;
#endif
using ICD.Connect.Routing.CrestronPro.DigitalMedia.DmNvx.DmNvx35X;

namespace ICD.Connect.Routing.CrestronPro.DigitalMedia.DmNvx.DmNvx351
{
#if !NETSTANDARD
	public sealed class DmNvx351Adapter :
		AbstractDmNvx35XAdapter<Crestron.SimplSharpPro.DM.Streaming.DmNvx351, DmNvx351AdapterSettings>
#else
	public sealed class DmNvx351Adapter : AbstractDmNvx35XAdapter<DmNvx351AdapterSettings>
#endif
	{
#if !NETSTANDARD
		/// <summary>
		/// Creates a new instance of the wrapped internal switcher.
		/// </summary>
		/// <param name="ethernetId"></param>
		/// <param name="controlSystem"></param>
		/// <returns></returns>
		public override Crestron.SimplSharpPro.DM.Streaming.DmNvx351 InstantiateStreamer(uint ethernetId, CrestronControlSystem controlSystem)
		{
			return new Crestron.SimplSharpPro.DM.Streaming.DmNvx351(ethernetId, controlSystem);
		}

		/// <summary>
		/// Creates a new instance of the wrapped internal switcher.
		/// </summary>
		/// <param name="endpointId"></param>
		/// <param name="domain"></param>
		/// <param name="isReceiver"></param>
		/// <returns></returns>
		public override Crestron.SimplSharpPro.DM.Streaming.DmNvx351 InstantiateStreamer(uint endpointId, Crestron.SimplSharpPro.DM.Streaming.DmXioDirectorBase.DmXioDomain domain, bool isReceiver)
		{
			return new Crestron.SimplSharpPro.DM.Streaming.DmNvx351(endpointId, domain, isReceiver);
		}
#endif
	}
}
