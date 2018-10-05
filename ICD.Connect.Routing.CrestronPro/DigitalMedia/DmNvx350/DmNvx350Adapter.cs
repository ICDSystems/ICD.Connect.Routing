#if SIMPLSHARP
using Crestron.SimplSharpPro;
#endif
using ICD.Connect.Routing.CrestronPro.DigitalMedia.DmNvx35X;

namespace ICD.Connect.Routing.CrestronPro.DigitalMedia.DmNvx350
{
#if SIMPLSHARP
	public sealed class DmNvx350Adapter :
		AbstractDmNvx35XAdapter<Crestron.SimplSharpPro.DM.Streaming.DmNvx350, DmNvx350AdapterSettings>
#else
	public sealed class DmNvx350Adapter : AbstractDmNvx35XAdapter<DmNvx350AdapterSettings>
#endif
	{
#if SIMPLSHARP
		/// <summary>
		/// Creates a new instance of the wrapped internal switcher.
		/// </summary>
		/// <param name="ethernetId"></param>
		/// <param name="controlSystem"></param>
		/// <returns></returns>
		protected override Crestron.SimplSharpPro.DM.Streaming.DmNvx350 InstantiateSwitcher(uint ethernetId,
		                                                                                    CrestronControlSystem
			                                                                                    controlSystem)
		{
			return new Crestron.SimplSharpPro.DM.Streaming.DmNvx350(ethernetId, controlSystem);
		}
#endif
	}
}
