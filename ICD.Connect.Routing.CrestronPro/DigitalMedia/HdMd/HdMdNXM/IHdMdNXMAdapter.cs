#if !NETSTANDARD
using Crestron.SimplSharpPro.DM;

#endif

namespace ICD.Connect.Routing.CrestronPro.DigitalMedia.HdMd.HdMdNXM
{
	public interface IHdMdNXMAdapter : ICrestronSwitchAdapter
	{
#if !NETSTANDARD
		new HdMdNxM Switcher { get; }
#endif
	}
}
