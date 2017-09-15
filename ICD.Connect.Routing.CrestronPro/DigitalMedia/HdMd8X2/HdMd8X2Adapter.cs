#if SIMPLSHARP
using Crestron.SimplSharpPro.DM;
using ICD.Connect.Misc.CrestronPro;
#endif

namespace ICD.Connect.Routing.CrestronPro.DigitalMedia.HdMd8X2
{
#if SIMPLSHARP
	public sealed class HdMd8X2Adapter : AbstractDmSwitcherAdapter<HdMd8x2, HdMd8X2AdapterSettings>
	{
		/// <summary>
		/// Constructor.
		/// </summary>
		public HdMd8X2Adapter()
		{
            Controls.Add(new HdMd8X2SwitcherControl(this));
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
#else
	public sealed class HdMd8X2Adapter : AbstractDmSwitcherAdapter<HdMd8X2AdapterSettings>
	{
	}
#endif
}
