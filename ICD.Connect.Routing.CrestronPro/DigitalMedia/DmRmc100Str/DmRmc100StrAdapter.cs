#if SIMPLSHARP
using Crestron.SimplSharpPro;
#endif
using ICD.Connect.Routing.CrestronPro.DigitalMedia.Dm100xStrBase;

namespace ICD.Connect.Routing.CrestronPro.DigitalMedia.DmRmc100Str
{
#if SIMPLSHARP
	public sealed class DmRmc100StrAdapter :
		AbstractDm100XStrBaseAdapter<Crestron.SimplSharpPro.DM.Streaming.DmRmc100Str, DmRmc100StrAdapterSettings>
#else
	public sealed class DmRmc100StrAdapter : AbstractDm100XStrBaseAdapter<DmRmc100StrAdapterSettings>
#endif
	{
#if SIMPLSHARP
		/// <summary>
		/// Creates a new instance of the wrapped internal switcher.
		/// </summary>
		/// <param name="ethernetId"></param>
		/// <param name="controlSystem"></param>
		/// <returns></returns>
		public override Crestron.SimplSharpPro.DM.Streaming.DmRmc100Str InstantiateStreamer(uint ethernetId, CrestronControlSystem controlSystem)
		{
			return new Crestron.SimplSharpPro.DM.Streaming.DmRmc100Str(ethernetId, controlSystem);
		}

		/// <summary>
		/// Creates a new instance of the wrapped internal switcher.
		/// </summary>
		/// <param name="endpointId"></param>
		/// <param name="domain"></param>
		/// <param name="isReceiver"></param>
		/// <returns></returns>
		public override Crestron.SimplSharpPro.DM.Streaming.DmRmc100Str InstantiateStreamer(uint endpointId, Crestron.SimplSharpPro.DM.Streaming.DmXioDirectorBase.DmXioDomain domain, bool isReceiver)
		{
			return new Crestron.SimplSharpPro.DM.Streaming.DmRmc100Str(endpointId, domain, isReceiver);
		}
#endif
	}
}
