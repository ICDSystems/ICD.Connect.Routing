using ICD.Connect.Routing.CrestronPro.DigitalMedia.HdMd.HdMdNxM4kzE;

namespace ICD.Connect.Routing.CrestronPro.DigitalMedia.HdMd.HdMd4xX4kzE
{
	// ReSharper disable once InconsistentNaming
#if !NETSTANDARD
	public abstract class AbstractHdMd4xX4kzEAdapter<TSwitcher, TSettings> : AbstractHdMdNxM4kzEAdapter<TSwitcher, TSettings>
		where TSwitcher : Crestron.SimplSharpPro.DM.HdMd4xX4kzE
#else
	public abstract class AbstractHdMd4xX4kzEAdapter<TSettings> : AbstractHdMdNxM4kzEAdapter<TSettings>
#endif
		where TSettings : AbstractHdMd4xX4kzEAdapterSettings, new()
	{
	}
}