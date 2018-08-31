using ICD.Connect.Panels.Crestron.Controls.TouchScreens;

namespace ICD.Connect.Routing.CrestronPro.ControlSystem.Controls
{
	public abstract class AbstractMPC3x101TouchScreenControl : AbstractMPC3BasicTouchScreenControl, IMPC3x101TouchScreenControl
	{
		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="parent"></param>
		/// <param name="id"></param>
		protected AbstractMPC3x101TouchScreenControl(ControlSystemDevice parent, int id)
			: base(parent, id)
		{
		}
	}
}