#if SIMPLSHARP
using Crestron.SimplSharpPro;
#endif
using ICD.Connect.Routing.CrestronPro.DigitalMedia.DmNvx35X;

namespace ICD.Connect.Routing.CrestronPro.DigitalMedia.DmNvx351
{
#if SIMPLSHARP
	public sealed class DmNvx351Adapter :
		AbstractDmNvx35XAdapter<Crestron.SimplSharpPro.DM.Streaming.DmNvx351, DmNvx351AdapterSettings>
#else
	public sealed class DmNvx351Adapter : AbstractDmNvx35XAdapter<DmNvx351AdapterSettings>
#endif
	{
#if SIMPLSHARP
		/// <summary>
		/// Creates a new instance of the wrapped internal switcher.
		/// </summary>
		/// <param name="ethernetId"></param>
		/// <param name="controlSystem"></param>
		/// <returns></returns>
		protected override Crestron.SimplSharpPro.DM.Streaming.DmNvx351 InstantiateSwitcher(uint ethernetId,
		                                                                                    CrestronControlSystem
			                                                                                    controlSystem)
		{
			return new Crestron.SimplSharpPro.DM.Streaming.DmNvx351(ethernetId, controlSystem);
		}
#endif
	}
}
