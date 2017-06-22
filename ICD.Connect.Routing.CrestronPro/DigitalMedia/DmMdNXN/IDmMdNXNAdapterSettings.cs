using ICD.Connect.Settings;

namespace ICD.Connect.Routing.CrestronPro.DigitalMedia.DmMdNXN
{
// ReSharper disable once InconsistentNaming
	public interface IDmMdNXNAdapterSettings : ISettings
	{
		byte Ipid { get; set; }
	}
}