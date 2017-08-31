﻿using ICD.Connect.Misc.CrestronPro;
#if SIMPLSHARP
using Crestron.SimplSharpPro.DM;
#endif
using ICD.Connect.Routing.CrestronPro.DigitalMedia.DmMdNXN;

namespace ICD.Connect.Routing.CrestronPro.DigitalMedia.DmMd32X32
{
#if SIMPLSHARP
    public sealed class DmMd32X32Adapter : AbstractDmMdMNXNAdapter<DmMd32x32, DmMd32X32AdapterSettings>
	{
	    /// <summary>
	    /// Creates a new instance of the wrapped internal switcher.
	    /// </summary>
	    /// <param name="settings"></param>
	    /// <returns></returns>
	    protected override DmMd32x32 InstantiateSwitcher(DmMd32X32AdapterSettings settings)
	    {
		    return new DmMd32x32(settings.Ipid, ProgramInfo.ControlSystem);
	    }
	}
#else
    public sealed class DmMd32X32Adapter : AbstractDmMdMNXNAdapter<DmMd32X32AdapterSettings>
    {
    }
#endif
}
