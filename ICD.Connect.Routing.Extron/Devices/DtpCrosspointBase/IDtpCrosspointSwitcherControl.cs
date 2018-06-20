using ICD.Connect.Routing.Controls;

namespace ICD.Connect.Routing.Extron.Devices.DtpCrosspointBase
{
	public interface IDtpCrosspointSwitcherControl : IRouteSwitcherControl
	{
		int NumberOfInputs { get; }
		int NumberOfOutputs { get; }
	}
}