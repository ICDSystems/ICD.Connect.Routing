#if !NETSTANDARD
using Crestron.SimplSharpPro.DM;

#endif

namespace ICD.Connect.Routing.CrestronPro.DigitalMedia.HdMd.HdMd8XN
{
	public interface IHdMd8XNAdapter : ICrestronSwitchAdapter
	{
#if !NETSTANDARD
		new HdMd8xN Switcher { get; }
#endif
	}
}