using Crestron.SimplSharpPro.DM;
using ICD.Connect.Devices;

namespace ICD.Connect.Routing.CrestronPro.DigitalMedia.DmMdNXN
{
// ReSharper disable once InconsistentNaming
	public delegate void DmMdMNXNChangeCallback(IDmMdMNXNAdapter sender, DmMDMnxn switcher);

// ReSharper disable once InconsistentNaming
	public interface IDmMdMNXNAdapter : IDevice
	{
		event DmMdMNXNChangeCallback OnSwitcherChanged;

		DmMDMnxn Switcher { get; }
	}
}