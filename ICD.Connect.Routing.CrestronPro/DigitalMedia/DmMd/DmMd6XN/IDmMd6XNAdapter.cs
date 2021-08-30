namespace ICD.Connect.Routing.CrestronPro.DigitalMedia.DmMd.DmMd6XN
{
	public interface IDmMd6XNAdapter : ICrestronSwitchAdapter
	{
#if !NETSTANDARD
		new Crestron.SimplSharpPro.DM.DmMd6XN Switcher { get; }
#endif
	}
}
