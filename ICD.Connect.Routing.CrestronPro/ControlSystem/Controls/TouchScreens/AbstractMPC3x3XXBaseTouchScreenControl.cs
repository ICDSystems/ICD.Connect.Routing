#if SIMPLSHARP
using Crestron.SimplSharpPro;
#endif
using ICD.Connect.Panels.Crestron.Controls.TouchScreens;

namespace ICD.Connect.Routing.CrestronPro.ControlSystem.Controls
{
#if SIMPLSHARP
	public abstract class AbstractMPC3x3XXBaseTouchScreenControl<TTouchScreen> :
		AbstractMPC3BasicTouchScreenControl<TTouchScreen>, IMPC3x3XXBaseTouchScreenControl
		where TTouchScreen : MPC3x3XXBase
#else
	public abstract class AbstractMPC3x3XXBaseTouchScreenControl : AbstractMPC3BasicTouchScreenControl, IMPC3x3XXBaseTouchScreenControl
#endif
	{
		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="parent"></param>
		/// <param name="id"></param>
		protected AbstractMPC3x3XXBaseTouchScreenControl(ControlSystemDevice parent, int id)
			: base(parent, id)
		{
		}
	}
}