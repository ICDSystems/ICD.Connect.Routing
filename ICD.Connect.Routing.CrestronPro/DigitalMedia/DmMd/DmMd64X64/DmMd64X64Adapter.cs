#if !NETSTANDARD
using Crestron.SimplSharpPro.DM;
using ICD.Connect.Misc.CrestronPro;
#endif
using ICD.Connect.Routing.CrestronPro.DigitalMedia.DmMd.BladeSwitch;

namespace ICD.Connect.Routing.CrestronPro.DigitalMedia.DmMd.DmMd64X64
{
#if !NETSTANDARD
	public sealed class DmMd64X64Adapter : AbstractCrestronBladeSwitchAdapter<DmMd64x64, DmMd64X64AdapterSettings>
	{
		/// <summary>
		/// Creates a new instance of the wrapped internal switcher.
		/// </summary>
		/// <param name="settings"></param>
		/// <returns></returns>
		protected override DmMd64x64 InstantiateSwitcher(DmMd64X64AdapterSettings settings)
		{
			return settings.Ipid == null 
				   ? null 
				   : new DmMd64x64(settings.Ipid.Value, ProgramInfo.ControlSystem);
		}
	}
#else
    public sealed class DmMd64X64Adapter : AbstractCrestronBladeSwitchAdapter<DmMd64X64AdapterSettings>
    {
    }
#endif
}
