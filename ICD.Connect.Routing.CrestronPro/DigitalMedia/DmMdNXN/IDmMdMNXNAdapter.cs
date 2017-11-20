#if SIMPLSHARP
using Crestron.SimplSharpPro.DM;

namespace ICD.Connect.Routing.CrestronPro.DigitalMedia.DmMdNXN
{
// ReSharper disable once InconsistentNaming
	public interface IDmMdMNXNAdapter : IDmSwitcherAdapter
	{
		new DmMDMnxn Switcher { get; }
	}
}
#endif