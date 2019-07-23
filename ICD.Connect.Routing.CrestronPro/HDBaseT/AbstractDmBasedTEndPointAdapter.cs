#if SIMPLSHARP
using Crestron.SimplSharpPro.DM.Endpoints;
#endif

namespace ICD.Connect.Routing.CrestronPro.HDBaseT
{
#if SIMPLSHARP
	public abstract class AbstractDmBasedTEndPointAdapter<TDevice, TSettings> : AbstractHdBaseTWithIrBaseAdapter<TDevice, TSettings>, IDmBasedTEndPointAdapter
		where TDevice : DmHDBasedTEndPoint
#else
	public abstract class AbstractDmBasedTEndPointAdapter<TSettings> : AbstractHdBaseTWithIrBaseAdapter<TSettings>, IDmBasedTEndPointAdapter
#endif
		where TSettings : IDmBasedTEndPointAdapterSettings, new()
	{
	}

	public abstract class AbstractDmBasedTEndPointAdapterSettings : AbstractHdBaseTWithIrBaseAdapterSettings,
	                                                                IDmBasedTEndPointAdapterSettings
	{
	}

	public interface IDmBasedTEndPointAdapter : IHdBaseTWithIrBaseAdapter
	{
	}

	public interface IDmBasedTEndPointAdapterSettings : IHdBaseTWithIrBaseAdapterSettings
	{
	}
}
