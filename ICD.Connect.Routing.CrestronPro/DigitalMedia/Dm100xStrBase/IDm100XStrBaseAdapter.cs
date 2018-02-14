using ICD.Connect.Devices;
using ICD.Connect.Misc.CrestronPro.Devices;

namespace ICD.Connect.Routing.CrestronPro.DigitalMedia.Dm100xStrBase
{
#if SIMPLSHARP
	public delegate void Dm100XStrBaseChangeCallback(
		IDm100XStrBaseAdapter sender, Crestron.SimplSharpPro.DM.Streaming.Dm100xStrBase switcher);
#endif

	public interface IDm100XStrBaseAdapter : IDevice, IPortParent
	{
#if SIMPLSHARP
		event Dm100XStrBaseChangeCallback OnSwitcherChanged;

		Crestron.SimplSharpPro.DM.Streaming.Dm100xStrBase Switcher { get; }
#endif
	}
}