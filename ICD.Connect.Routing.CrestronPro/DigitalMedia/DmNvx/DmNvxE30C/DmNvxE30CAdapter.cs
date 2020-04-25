#if SIMPLSHARP
using Crestron.SimplSharpPro;
#endif
using ICD.Connect.Routing.CrestronPro.DigitalMedia.DmNvx.DmNvxE3X;

namespace ICD.Connect.Routing.CrestronPro.DigitalMedia.DmNvx.DmNvxE30C
{
#if SIMPLSHARP
	public sealed class DmNvxE30CAdapter :
		AbstractDmNvxE3XAdapter<Crestron.SimplSharpPro.DM.Streaming.DmNvxE30C, DmNvxE30CAdapterSettings>
#else
	public sealed class DmNvxE30CAdapter : AbstractDmNvxE3XAdapter<DmNvxE30CAdapterSettings>
#endif
	{
#if SIMPLSHARP
		/// <summary>
		/// Creates a new instance of the wrapped internal switcher.
		/// </summary>
		/// <param name="ethernetId"></param>
		/// <param name="controlSystem"></param>
		/// <returns></returns>
		public override Crestron.SimplSharpPro.DM.Streaming.DmNvxE30C InstantiateStreamer(uint ethernetId, CrestronControlSystem controlSystem)
		{
			return new Crestron.SimplSharpPro.DM.Streaming.DmNvxE30C(ethernetId, controlSystem);
		}

		/// <summary>
		/// Creates a new instance of the wrapped internal switcher.
		/// </summary>
		/// <param name="endpointId"></param>
		/// <param name="domain"></param>
		/// <param name="isReceiver"></param>
		/// <returns></returns>
		public override Crestron.SimplSharpPro.DM.Streaming.DmNvxE30C InstantiateStreamer(uint endpointId, Crestron.SimplSharpPro.DM.Streaming.DmXioDirectorBase.DmXioDomain domain, bool isReceiver)
		{
			return new Crestron.SimplSharpPro.DM.Streaming.DmNvxE30C(endpointId, domain);
		}
#endif
	}
}
