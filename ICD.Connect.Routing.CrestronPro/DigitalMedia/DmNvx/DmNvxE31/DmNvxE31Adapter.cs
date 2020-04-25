#if SIMPLSHARP
using Crestron.SimplSharpPro;
#endif
using ICD.Connect.Routing.CrestronPro.DigitalMedia.DmNvx.DmNvxE3X;

namespace ICD.Connect.Routing.CrestronPro.DigitalMedia.DmNvx.DmNvxE31
{
#if SIMPLSHARP
	public sealed class DmNvxE31Adapter :
		AbstractDmNvxE3XAdapter<Crestron.SimplSharpPro.DM.Streaming.DmNvxE31, DmNvxE31AdapterSettings>
#else
	public sealed class DmNvxE31Adapter : AbstractDmNvxE3XAdapter<DmNvxE31AdapterSettings>
#endif
	{
#if SIMPLSHARP
		/// <summary>
		/// Creates a new instance of the wrapped internal switcher.
		/// </summary>
		/// <param name="ethernetId"></param>
		/// <param name="controlSystem"></param>
		/// <returns></returns>
		public override Crestron.SimplSharpPro.DM.Streaming.DmNvxE31 InstantiateStreamer(uint ethernetId, CrestronControlSystem controlSystem)
		{
			return new Crestron.SimplSharpPro.DM.Streaming.DmNvxE31(ethernetId, controlSystem);
		}

		/// <summary>
		/// Creates a new instance of the wrapped internal switcher.
		/// </summary>
		/// <param name="endpointId"></param>
		/// <param name="domain"></param>
		/// <param name="isReceiver"></param>
		/// <returns></returns>
		public override Crestron.SimplSharpPro.DM.Streaming.DmNvxE31 InstantiateStreamer(uint endpointId, Crestron.SimplSharpPro.DM.Streaming.DmXioDirectorBase.DmXioDomain domain, bool isReceiver)
		{
			return new Crestron.SimplSharpPro.DM.Streaming.DmNvxE31(endpointId, domain);
		}
#endif
	}
}
