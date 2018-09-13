using ICD.Connect.Panels.Crestron.Controls.TouchScreens;
#if SIMPLSHARP
using Crestron.SimplSharpPro;
#endif

namespace ICD.Connect.Routing.CrestronPro.ControlSystem.Controls
{
	public sealed class TPCSTouchScreenControl
#if SIMPLSHARP
		: AbstractControlSystemTouchScreenControl<TPCSTouchscreen>, ITPCSTouchScreenControl
#else
		AbstractControlSystemTouchScreenControl, ITPCSTouchScreenControl
#endif
	{
		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="parent"></param>
		/// <param name="id"></param>
		public TPCSTouchScreenControl(ControlSystemDevice parent, int id)
			: base(parent, id)
		{
		}
	}
}
