#if SIMPLSHARP
using Crestron.SimplSharpPro.DM;

#endif

namespace ICD.Connect.Routing.CrestronPro.DigitalMedia.DmMd.DmMdMNXN
{
// ReSharper disable once InconsistentNaming
	public interface IDmMdMNXNAdapter : ICrestronSwitchAdapter
	{
#if SIMPLSHARP
		new DmMDMnxn Switcher { get; }
#endif
	}
}
