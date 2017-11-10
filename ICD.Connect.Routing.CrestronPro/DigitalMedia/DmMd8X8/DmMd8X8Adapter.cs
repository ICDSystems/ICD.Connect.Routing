using ICD.Common.Services.Logging;
using ICD.Connect.Misc.CrestronPro;
#if SIMPLSHARP
using Crestron.SimplSharpPro.DM;
#endif
using ICD.Connect.Routing.CrestronPro.DigitalMedia.DmMdNXN;

namespace ICD.Connect.Routing.CrestronPro.DigitalMedia.DmMd8X8
{
#if SIMPLSHARP
    public sealed class DmMd8X8Adapter : AbstractDmMdMNXNAdapter<DmMd8x8, DmMd8X8AdapterSettings>
	{
	    /// <summary>
	    /// Creates a new instance of the wrapped internal switcher.
	    /// </summary>
	    /// <param name="settings"></param>
	    /// <returns></returns>
	    protected override DmMd8x8 InstantiateSwitcher(DmMd8X8AdapterSettings settings)
	    {
            return new DmMd8x8(settings.Ipid, ProgramInfo.ControlSystem);
	    }
	}
#else
    public sealed class DmMd8X8Adapter : AbstractDmMdMNXNAdapter<DmMd8X8AdapterSettings>
    {
    }
#endif
}
