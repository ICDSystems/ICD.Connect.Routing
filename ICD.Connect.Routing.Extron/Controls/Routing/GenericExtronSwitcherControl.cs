using ICD.Connect.Routing.Extron.Devices.Switchers;

namespace ICD.Connect.Routing.Extron.Controls.Routing
{
	public sealed class GenericExtronSwitcherControl : AbstractExtronSwitcherControl<IExtronSwitcherDevice>
	{
		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="parent"></param>
		/// <param name="id"></param>
		/// <param name="numInputs"></param>
		/// <param name="numOutputs"></param>
		/// <param name="breakaway"></param>
		public GenericExtronSwitcherControl(IExtronSwitcherDevice parent, int id, int numInputs, int numOutputs, bool breakaway)
			: base(parent, id, numInputs, numOutputs, breakaway)
		{
		}
	}
}