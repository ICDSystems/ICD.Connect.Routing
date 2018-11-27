#if SIMPLSHARP
using Crestron.SimplSharpPro;
#endif
using ICD.Connect.Panels.Crestron.Controls.TouchScreens;

namespace ICD.Connect.Routing.CrestronPro.ControlSystem.Controls.TouchScreens
{
#if SIMPLSHARP
	public sealed class TSCW730TouchScreenControl : AbstractControlSystemTouchScreenControl<TSCW730TouchScreen>, ITSCW730TouchScreenControl
#else
	public sealed class TSCW730TouchScreenControl : AbstractControlSystemTouchScreenControl, ITSCW730TouchScreenControl
#endif
	{
		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="parent"></param>
		/// <param name="id"></param>
		public TSCW730TouchScreenControl(ControlSystemDevice parent, int id)
			: base(parent, id)
		{
		}
	}
}
