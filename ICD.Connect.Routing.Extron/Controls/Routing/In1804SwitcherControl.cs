using ICD.Connect.Routing.Connections;
using ICD.Connect.Routing.Extron.Devices.Switchers.In1804;

namespace ICD.Connect.Routing.Extron.Controls.Routing
{
	public sealed class In1804SwitcherControl : AbstractExtronSwitcherControl<IIn1804Device>
	{
		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="parent"></param>
		/// <param name="id"></param>
		/// <param name="numInputs"></param>
		/// <param name="numOutputs"></param>
		/// <param name="breakaway"></param>
		public In1804SwitcherControl(IIn1804Device parent, int id, int numInputs, int numOutputs, bool breakaway)
			: base(parent, id, numInputs, numOutputs, breakaway)
		{
		}

		/// <summary>
		/// Removes the given connection type from the output.
		/// </summary>
		/// <param name="output"></param>
		/// <param name="type"></param>
		/// <returns></returns>
		public override bool ClearOutput(int output, eConnectionType type)
		{
			return false;
		}
	}
}