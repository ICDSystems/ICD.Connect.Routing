using ICD.Connect.Routing.Controls;

namespace ICD.Connect.Routing.Extron.Devices.Switchers
{
	public interface IDtpCrosspointSwitcherControl : IRouteSwitcherControl
	{
		int NumberOfInputs { get; }
		int NumberOfOutputs { get; }
	}
}