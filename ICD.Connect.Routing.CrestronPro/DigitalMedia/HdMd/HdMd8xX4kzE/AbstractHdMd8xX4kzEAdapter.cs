using ICD.Connect.Routing.CrestronPro.DigitalMedia.HdMd.HdMdNxM4kzE;

namespace ICD.Connect.Routing.CrestronPro.DigitalMedia.HdMd.HdMd8xX4kzE
{
#if !NETSTANDARD
// ReSharper disable once InconsistentNaming
	public abstract class AbstractHdMd8xX4kzEAdapter<TSwitcher, TSettings> : AbstractHdMdNxM4kzEAdapter<TSwitcher, TSettings>
		where TSwitcher : Crestron.SimplSharpPro.DM.HdMd8xXkzE
#else
	// ReSharper disable once InconsistentNaming
	public abstract class AbstractHdMd8xX4kzEAdapter<TSettings> : AbstractHdMdNxM4kzEAdapter<TSettings>
#endif
		where TSettings : AbstractHdMd8xX4kzEAdapterSettings, new()
	{
	}
}