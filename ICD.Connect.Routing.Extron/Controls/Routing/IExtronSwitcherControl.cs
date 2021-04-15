using ICD.Connect.Routing.Controls;

namespace ICD.Connect.Routing.Extron.Controls.Routing
{
	public interface IExtronSwitcherControl : IRouteSwitcherControl
	{
		/// <summary>
		/// Gets the number of inputs.
		/// </summary>
		int NumberOfInputs { get; }

		/// <summary>
		/// Gets the number of outputs.
		/// </summary>
		int NumberOfOutputs { get; }
	}
}