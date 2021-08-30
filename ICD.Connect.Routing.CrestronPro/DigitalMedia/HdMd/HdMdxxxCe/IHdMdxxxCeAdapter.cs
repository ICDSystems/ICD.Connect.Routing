#if !NETSTANDARD
using Crestron.SimplSharpPro.DM;

#endif

namespace ICD.Connect.Routing.CrestronPro.DigitalMedia.HdMd.HdMdxxxCe
{
	public interface IHdMdxxxCeAdapter : ICrestronSwitchAdapter
	{
#if !NETSTANDARD
		new HdMdxxxCE Switcher { get; }
#endif
	}
}
