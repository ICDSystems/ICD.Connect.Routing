using ICD.Connect.Misc.CrestronPro;
#if SIMPLSHARP
using Crestron.SimplSharpPro.DM;
#endif
using ICD.Connect.Routing.CrestronPro.DigitalMedia.DmMdNXN;

namespace ICD.Connect.Routing.CrestronPro.DigitalMedia.DmMd16X16
{
#if SIMPLSHARP
    public sealed class DmMd16X16Adapter : AbstractDmMdMNXNAdapter<DmMd16x16, DmMd16X16AdapterSettings>
	{
	    /// <summary>
	    /// Creates a new instance of the wrapped internal switcher.
	    /// </summary>
	    /// <param name="settings"></param>
	    /// <returns></returns>
	    protected override DmMd16x16 InstantiateSwitcher(DmMd16X16AdapterSettings settings)
	    {
		    return new DmMd16x16(settings.Ipid, ProgramInfo.ControlSystem);
	    }
	}
#else
    public sealed class DmMd16X16Adapter : AbstractDmMdMNXNAdapter<DmMd16X16AdapterSettings>
    {
    }
#endif
}
