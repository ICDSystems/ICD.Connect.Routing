using ICD.Connect.Panels.Crestron.Controls.TouchScreens;

namespace ICD.Connect.Routing.CrestronPro.ControlSystem.Controls
{
	public abstract class AbstractMPC3x3XXBaseTouchScreenControl : AbstractMPC3BasicTouchScreenControl,
	                                                               IMPC3x3XXBaseTouchScreenControl
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