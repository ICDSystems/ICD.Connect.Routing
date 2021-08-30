namespace ICD.Connect.Routing.CrestronPro.DigitalMedia.HdMd.HdMdNxM4kzE
{
// ReSharper disable once InconsistentNaming
	public interface IHdMdNxM4kzEAdapter : ICrestronSwitchAdapter
	{
#if !NETSTANDARD
		new Crestron.SimplSharpPro.DM.HdMdNxM4kzE Switcher { get; }
#endif
	}
}