#if SIMPLSHARP
using Crestron.SimplSharpPro;
#endif
using ICD.Connect.Routing.CrestronPro.DigitalMedia.DmNvx35X;

namespace ICD.Connect.Routing.CrestronPro.DigitalMedia.DmNvx351C
{
#if SIMPLSHARP
	public sealed class DmNvx351CAdapter :
		AbstractDmNvx35XAdapter<Crestron.SimplSharpPro.DM.Streaming.DmNvx351C, DmNvx351CAdapterSettings>
#else
	public sealed class DmNvx351CAdapter : AbstractDmNvx35XAdapter<DmNvx351CAdapterSettings>
#endif
	{
#if SIMPLSHARP
		/// <summary>
		/// Creates a new instance of the wrapped internal switcher.
		/// </summary>
		/// <param name="ethernetId"></param>
		/// <param name="controlSystem"></param>
		/// <returns></returns>
		protected override Crestron.SimplSharpPro.DM.Streaming.DmNvx351C InstantiateSwitcher(uint ethernetId,
		                                                                                    CrestronControlSystem
			                                                                                    controlSystem)
		{
			return new Crestron.SimplSharpPro.DM.Streaming.DmNvx351C(ethernetId, controlSystem);
		}
#endif
	}
}
