#if SIMPLSHARP
using Crestron.SimplSharpPro.DM;
#endif

namespace ICD.Connect.Routing.CrestronPro.DigitalMedia.HdMdNXM
{
	public interface IHdMdNXMAdapter : ICrestronSwitchAdapter
	{
#if SIMPLSHARP
		new HdMdNxM Switcher { get; }
#endif
	}
}
