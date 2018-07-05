#if SIMPLSHARP
using Crestron.SimplSharpPro.DM;
#endif

namespace ICD.Connect.Routing.CrestronPro.DigitalMedia.HdMdxxxCe
{
	public interface IHdMdxxxCeAdapter : ICrestronSwitchAdapter
	{
#if SIMPLSHARP
		new HdMdxxxCE Switcher { get; }
#endif
	}
}
