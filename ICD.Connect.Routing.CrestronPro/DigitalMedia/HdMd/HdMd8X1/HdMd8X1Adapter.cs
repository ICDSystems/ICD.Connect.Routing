#if SIMPLSHARP
using Crestron.SimplSharpPro.DM;
using ICD.Connect.Misc.CrestronPro;
#endif
using ICD.Connect.Routing.CrestronPro.DigitalMedia.HdMd.HdMd8XN;

namespace ICD.Connect.Routing.CrestronPro.DigitalMedia.HdMd.HdMd8X1
{
#if SIMPLSHARP
	public sealed class HdMd8X1Adapter : AbstractHdMd8XNAdapter<HdMd8x1, HdMd8X1AdapterSettings>
	{
		/// <summary>
		/// Creates a new instance of the wrapped internal switcher.
		/// </summary>
		/// <param name="settings"></param>
		/// <returns></returns>
		protected override HdMd8x1 InstantiateSwitcher(HdMd8X1AdapterSettings settings)
		{
			return settings.Ipid == null 
			       ? null
				   : new HdMd8x1(settings.Ipid.Value, ProgramInfo.ControlSystem);
		}
	}
#else
	public sealed class HdMd8X1Adapter : AbstractHdMd8XNAdapter<HdMd8X1AdapterSettings>
	{
	}
#endif
}
