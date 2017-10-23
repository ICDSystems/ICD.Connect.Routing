using ICD.Connect.Devices;
using ICD.Connect.Misc.CrestronPro.Devices;

namespace ICD.Connect.Routing.CrestronPro.DigitalMedia.Dm100xStrBase
{
	public delegate void Dm100XStrBaseChangeCallback(
		IDm100XStrBaseAdapter sender, Crestron.SimplSharpPro.DM.Streaming.Dm100xStrBase switcher);

	public interface IDm100XStrBaseAdapter : IDevice, IPortParent
	{
		event Dm100XStrBaseChangeCallback OnSwitcherChanged;

		Crestron.SimplSharpPro.DM.Streaming.Dm100xStrBase Switcher { get; }
	}
}