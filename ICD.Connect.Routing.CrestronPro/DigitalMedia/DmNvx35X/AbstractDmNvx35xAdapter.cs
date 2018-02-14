using ICD.Connect.Routing.CrestronPro.DigitalMedia.DmNvxBaseClass;

namespace ICD.Connect.Routing.CrestronPro.DigitalMedia.DmNvx35X
{
#if SIMPLSHARP
	public abstract class AbstractDmNvx35XAdapter<TSwitcher, TSettings> :
		AbstractDmNvxBaseClassAdapter<TSwitcher, TSettings>, IDmNvx35XAdapter
		where TSwitcher : Crestron.SimplSharpPro.DM.Streaming.DmNvx35x
#else
	public abstract class AbstractDmNvx35XAdapter<TSettings> : AbstractDmNvxBaseClassAdapter<TSettings>, IDmNvx35XAdapter
#endif
		where TSettings : IDmNvx35XAdapterSettings, new()
	{
	}

	public abstract class AbstractDmNvx35XAdapterSettings : AbstractDmNvxBaseClassAdapterSettings, IDmNvx35XAdapterSettings
	{
	}

	public interface IDmNvx35XAdapterSettings : IDmNvxBaseClassAdapterSettings
	{
	}

	public interface IDmNvx35XAdapter : IDmNvxBaseClassAdapter
	{
	}
}
