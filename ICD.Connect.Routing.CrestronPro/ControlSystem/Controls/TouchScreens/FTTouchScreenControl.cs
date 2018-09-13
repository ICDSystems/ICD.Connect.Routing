using ICD.Connect.Panels.Crestron.Controls.TouchScreens;
#if SIMPLSHARP
using Crestron.SimplSharpPro;
#endif

namespace ICD.Connect.Routing.CrestronPro.ControlSystem.Controls.TouchScreens
{
#if SIMPLSHARP
	public sealed class FTTouchScreenControl : AbstractControlSystemTouchScreenControl<FTTouchscreen>, IFTTouchScreenControl
#else
	public sealed class FTTouchScreenControl : AbstractControlSystemTouchScreenControl, IFTTouchScreenControl
#endif
	{
		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="parent"></param>
		/// <param name="id"></param>
		public FTTouchScreenControl(ControlSystemDevice parent, int id)
			: base(parent, id)
		{
		}
	}
}