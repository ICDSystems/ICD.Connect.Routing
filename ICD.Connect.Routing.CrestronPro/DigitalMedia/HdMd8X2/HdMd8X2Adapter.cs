#if SIMPLSHARP
using Crestron.SimplSharpPro.DM;
using ICD.Connect.Misc.CrestronPro;
#else
using System;
#endif

namespace ICD.Connect.Routing.CrestronPro.DigitalMedia.HdMd8X2
{
	public sealed class HdMd8X2Adapter : AbstractDmSwitcherAdapter<HdMd8x2, HdMd8X2AdapterSettings>
	{
		/// <summary>
		/// Constructor.
		/// </summary>
		public HdMd8X2Adapter()
		{
#if SIMPLSHARP
            Controls.Add(new HdMd8X2SwitcherControl(this));
#endif
		}

		/// <summary>
		/// Creates a new instance of the wrapped internal switcher.
		/// </summary>
		/// <param name="settings"></param>
		/// <returns></returns>
		protected override HdMd8x2 InstantiateSwitcher(HdMd8X2AdapterSettings settings)
		{
			return new HdMd8x2(settings.Ipid, ProgramInfo.ControlSystem);
		}
	}
}
