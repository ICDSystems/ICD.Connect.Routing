using ICD.Connect.Routing.CrestronPro.DigitalMedia.HdMd8XN;
#if SIMPLSHARP
using Crestron.SimplSharpPro.DM;
using ICD.Connect.Misc.CrestronPro;
#endif

namespace ICD.Connect.Routing.CrestronPro.DigitalMedia.DmMd8X14kC
{
#if SIMPLSHARP
	public sealed class DmMd8X14kCAdapter : AbstractHdMd8XNAdapter<DmMd8x14kC, DmMd8X14kCAdapterSettings>
	{
		/// <summary>
		/// Creates a new instance of the wrapped internal switcher.
		/// </summary>
		/// <param name="settings"></param>
		/// <returns></returns>
		protected override DmMd8x14kC InstantiateSwitcher(DmMd8X14kCAdapterSettings settings)
		{
			return settings.Ipid == null 
			       ? null
				   : new DmMd8x14kC(settings.Ipid.Value, ProgramInfo.ControlSystem);
		}
	}
#else
	public sealed class DmMd8X14kCAdapter : AbstractHdMd8XNAdapter<DmMd8X14kCAdapterSettings>
	{
	}
#endif
}
