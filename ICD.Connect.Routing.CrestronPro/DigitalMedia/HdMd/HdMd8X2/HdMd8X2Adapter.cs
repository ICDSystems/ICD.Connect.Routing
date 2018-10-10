#if SIMPLSHARP
using Crestron.SimplSharpPro.DM;
using ICD.Connect.Misc.CrestronPro;
#endif
using ICD.Connect.Routing.CrestronPro.DigitalMedia.HdMd.HdMd8XN;

namespace ICD.Connect.Routing.CrestronPro.DigitalMedia.HdMd.HdMd8X2
{
#if SIMPLSHARP
	public sealed class HdMd8X2Adapter : AbstractHdMd8XNAdapter<HdMd8x2, HdMd8X2AdapterSettings>
	{
		/// <summary>
		/// Creates a new instance of the wrapped internal switcher.
		/// </summary>
		/// <param name="settings"></param>
		/// <returns></returns>
		protected override HdMd8x2 InstantiateSwitcher(HdMd8X2AdapterSettings settings)
		{
			return settings.Ipid == null 
			       ? null
				   : new HdMd8x2(settings.Ipid.Value, ProgramInfo.ControlSystem);
		}
	}
#else
	public sealed class HdMd8X2Adapter : AbstractHdMd8XNAdapter<HdMd8X2AdapterSettings>
	{
	}
#endif
}
