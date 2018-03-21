#if SIMPLSHARP
using Crestron.SimplSharpPro.DM;
#endif

namespace ICD.Connect.Routing.CrestronPro.DigitalMedia.HdMd8XN
{
	public interface IHdMd8XNAdapter : ICrestronSwitchAdapter
	{
#if SIMPLSHARP
		new HdMd8xN Switcher { get; }
#endif
	}
}