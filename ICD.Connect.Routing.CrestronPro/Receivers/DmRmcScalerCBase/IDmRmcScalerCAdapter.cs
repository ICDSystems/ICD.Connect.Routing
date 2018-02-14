#if SIMPLSHARP
using ICD.Connect.Devices;

namespace ICD.Connect.Routing.CrestronPro.Receivers.DmRmcScalerCBase
{
	public delegate void DmRmcScalerCChangeCallback(
		IDmRmcScalerCAdapter sender, Crestron.SimplSharpPro.DM.Endpoints.Receivers.DmRmcScalerC midpoint);

	public interface IDmRmcScalerCAdapter : IDevice
	{
		event DmRmcScalerCChangeCallback OnScalerChanged;

		Crestron.SimplSharpPro.DM.Endpoints.Receivers.DmRmcScalerC Scaler { get; }
	}
}

#endif
