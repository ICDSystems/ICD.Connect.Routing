#if SIMPLSHARP
using Crestron.SimplSharpPro;
#endif
using ICD.Connect.Routing.CrestronPro.DigitalMedia.DmNvx.DmNvxE3X;

namespace ICD.Connect.Routing.CrestronPro.DigitalMedia.DmNvx.DmNvxE30
{
#if SIMPLSHARP
	public sealed class DmNvxE30Adapter :
		AbstractDmNvxE3XAdapter<Crestron.SimplSharpPro.DM.Streaming.DmNvxE30, DmNvxE30AdapterSettings>
#else
	public sealed class DmNvxE30Adapter : AbstractDmNvxE3XAdapter<DmNvxE30AdapterSettings>
#endif
	{
#if SIMPLSHARP
		/// <summary>
		/// Creates a new instance of the wrapped internal switcher.
		/// </summary>
		/// <param name="ethernetId"></param>
		/// <param name="controlSystem"></param>
		/// <returns></returns>
		public override Crestron.SimplSharpPro.DM.Streaming.DmNvxE30 InstantiateStreamer(uint ethernetId, CrestronControlSystem controlSystem)
		{
			return new Crestron.SimplSharpPro.DM.Streaming.DmNvxE30(ethernetId, controlSystem);
		}

		/// <summary>
		/// Creates a new instance of the wrapped internal switcher.
		/// </summary>
		/// <param name="endpointId"></param>
		/// <param name="domain"></param>
		/// <param name="isReceiver"></param>
		/// <returns></returns>
		public override Crestron.SimplSharpPro.DM.Streaming.DmNvxE30 InstantiateStreamer(uint endpointId, Crestron.SimplSharpPro.DM.Streaming.DmXioDirectorBase.DmXioDomain domain, bool isReceiver)
		{
			return new Crestron.SimplSharpPro.DM.Streaming.DmNvxE30(endpointId, domain);
		}
#endif
	}
}
