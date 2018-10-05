namespace ICD.Connect.Routing.CrestronPro.DigitalMedia.DmMd6XN
{
	public interface IDmMd6XNAdapter : ICrestronSwitchAdapter
	{
#if SIMPLSHARP
		new Crestron.SimplSharpPro.DM.DmMd6XN Switcher { get; }
#endif
	}
}
