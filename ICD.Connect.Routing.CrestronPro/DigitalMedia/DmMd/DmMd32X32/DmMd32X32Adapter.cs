#if !NETSTANDARD
using Crestron.SimplSharpPro.DM;
using ICD.Connect.Misc.CrestronPro;
#endif
using ICD.Connect.Routing.CrestronPro.DigitalMedia.DmMd.DmMdMNXN;

namespace ICD.Connect.Routing.CrestronPro.DigitalMedia.DmMd.DmMd32X32
{
#if !NETSTANDARD
	public sealed class DmMd32X32Adapter : AbstractDmMdMNXNAdapter<DmMd32x32, DmMd32X32AdapterSettings>
	{
		/// <summary>
		/// Creates a new instance of the wrapped internal switcher.
		/// </summary>
		/// <param name="settings"></param>
		/// <returns></returns>
		protected override DmMd32x32 InstantiateSwitcher(DmMd32X32AdapterSettings settings)
		{
			return settings.Ipid == null 
				   ? null 
				   : new DmMd32x32(settings.Ipid.Value, ProgramInfo.ControlSystem);
		}
	}
#else
    public sealed class DmMd32X32Adapter : AbstractDmMdMNXNAdapter<DmMd32X32AdapterSettings>
    {
    }
#endif
}
