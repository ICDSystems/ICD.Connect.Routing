#if !NETSTANDARD
using Crestron.SimplSharpPro.DM;
using ICD.Connect.Misc.CrestronPro;
#endif
using ICD.Connect.Routing.CrestronPro.DigitalMedia.HdMd.HdMd8XN;

namespace ICD.Connect.Routing.CrestronPro.DigitalMedia.HdMd.HdMd8X14k
{
#if !NETSTANDARD
	public sealed class HdMd8X14kAdapter : AbstractHdMd8XNAdapter<HdMd8x14k, HdMd8X14kAdapterSettings>
	{
		/// <summary>
		/// Creates a new instance of the wrapped internal switcher.
		/// </summary>
		/// <param name="settings"></param>
		/// <returns></returns>
		protected override HdMd8x14k InstantiateSwitcher(HdMd8X14kAdapterSettings settings)
		{
			return settings.Ipid == null 
			       ? null
				   : new HdMd8x14k(settings.Ipid.Value, ProgramInfo.ControlSystem);
		}
	}
#else
	public sealed class HdMd8X14kAdapter : AbstractHdMd8XNAdapter<HdMd8X14kAdapterSettings>
	{
	}
#endif
}
