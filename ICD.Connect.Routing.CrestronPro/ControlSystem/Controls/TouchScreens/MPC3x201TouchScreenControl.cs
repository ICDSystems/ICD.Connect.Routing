#if SIMPLSHARP
using Crestron.SimplSharpPro;
#endif
using ICD.Connect.Panels.Crestron.Controls.TouchScreens;

namespace ICD.Connect.Routing.CrestronPro.ControlSystem.Controls.TouchScreens
{
	public sealed class MPC3x201TouchScreenControl :
#if SIMPLSHARP
		AbstractMPC3x101TouchScreenControl<MPC3x201Touchscreen>, IMPC3x201TouchScreenControl
#else
		AbstractMPC3x101TouchScreenControl, IMPC3x201TouchScreenControl
#endif
	{
		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="parent"></param>
		/// <param name="id"></param>
		public MPC3x201TouchScreenControl(ControlSystemDevice parent, int id)
			: base(parent, id)
		{
		}
	}
}
