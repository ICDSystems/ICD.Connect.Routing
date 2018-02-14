using ICD.Connect.Routing.CrestronPro.DigitalMedia.Dm100xStrBase;

namespace ICD.Connect.Routing.CrestronPro.DigitalMedia.DmNvxBaseClass
{
#if SIMPLSHARP
	public abstract class AbstractDmNvxBaseClassAdapter<TSwitcher, TSettings> :
		AbstractDm100XStrBaseAdapter<TSwitcher, TSettings>, IDmNvxBaseClassAdapter
		where TSwitcher : Crestron.SimplSharpPro.DM.Streaming.DmNvxBaseClass
#else
	public abstract class AbstractDmNvxBaseClassAdapter<TSettings> : AbstractDm100XStrBaseAdapter<TSettings>, IDmNvxBaseClassAdapter
#endif
		where TSettings : IDmNvxBaseClassAdapterSettings, new()
	{
	}

	public interface IDmNvxBaseClassAdapter : IDm100XStrBaseAdapter
	{
	}

	public abstract class AbstractDmNvxBaseClassAdapterSettings : AbstractDm100XStrBaseAdapterSettings,
	                                                              IDmNvxBaseClassAdapterSettings
	{
	}

	public interface IDmNvxBaseClassAdapterSettings : IDm100XStrBaseAdapterSettings
	{
	}
}
