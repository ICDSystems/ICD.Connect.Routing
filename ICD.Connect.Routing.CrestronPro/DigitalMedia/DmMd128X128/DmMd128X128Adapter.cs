﻿using ICD.Connect.Routing.CrestronPro.DigitalMedia.BladeSwitch;
#if SIMPLSHARP
using Crestron.SimplSharpPro.DM;
using ICD.Connect.Misc.CrestronPro;
#endif

namespace ICD.Connect.Routing.CrestronPro.DigitalMedia.DmMd128X128
{
#if SIMPLSHARP
	public sealed class DmMd128X128Adapter : AbstractCrestronBladeSwitchAdapter<DmMd128x128, DmMd128X128AdapterSettings>
	{
		/// <summary>
		/// Creates a new instance of the wrapped internal switcher.
		/// </summary>
		/// <param name="settings"></param>
		/// <returns></returns>
		protected override DmMd128x128 InstantiateSwitcher(DmMd128X128AdapterSettings settings)
		{
			return settings.Ipid == null 
				   ? null 
				   : new DmMd128x128(settings.Ipid.Value, ProgramInfo.ControlSystem);
		}
	}
#else
    public sealed class DmMd128X128Adapter : AbstractCrestronBladeSwitchAdapter<DmMd128X128AdapterSettings>
    {
    }
#endif
}
