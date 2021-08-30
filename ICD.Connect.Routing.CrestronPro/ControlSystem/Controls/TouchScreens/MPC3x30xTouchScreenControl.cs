#if !NETSTANDARD
using Crestron.SimplSharpPro;
#endif
using ICD.Connect.Panels.Crestron.Controls.TouchScreens;

namespace ICD.Connect.Routing.CrestronPro.ControlSystem.Controls.TouchScreens
{
	public sealed class MPC3x30xTouchScreenControl :
#if !NETSTANDARD
		AbstractMPC3x3XXBaseTouchScreenControl<MPC3x30xTouchscreen>, IMPC3x30xTouchScreenControl
#else
		AbstractMPC3x3XXBaseTouchScreenControl, IMPC3x30xTouchScreenControl
#endif
	{
		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="parent"></param>
		/// <param name="id"></param>
		public MPC3x30xTouchScreenControl(ControlSystemDevice parent, int id)
			: base(parent, id)
		{
		}
	}
}
