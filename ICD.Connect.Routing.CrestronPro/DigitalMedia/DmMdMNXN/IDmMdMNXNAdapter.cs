#if SIMPLSHARP
using Crestron.SimplSharpPro.DM;
#endif

namespace ICD.Connect.Routing.CrestronPro.DigitalMedia.DmMdMNXN
{
// ReSharper disable once InconsistentNaming
	public interface IDmMdMNXNAdapter : IDmSwitcherAdapter
	{
#if SIMPLSHARP
		new DmMDMnxn Switcher { get; }
#endif
	}
}
