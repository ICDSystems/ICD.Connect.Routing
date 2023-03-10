using ICD.Connect.Panels.Crestron.Controls.TouchScreens;
#if !NETSTANDARD
using Crestron.SimplSharpPro;
#endif

namespace ICD.Connect.Routing.CrestronPro.ControlSystem.Controls.TouchScreens
{
#if !NETSTANDARD
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