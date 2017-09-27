using ICD.Connect.Routing.CrestronPro.Transmitters.DmTx200Base;

namespace ICD.Connect.Routing.CrestronPro.Transmitters.DmTx201S
{
#if SIMPLSHARP
	public abstract class AbstractDmTx201SAdapter<TTransmitter, TSettings> :
		AbstractDmTx200BaseAdapter<TTransmitter, TSettings>, IDmTx201SAdapter
		where TTransmitter : Crestron.SimplSharpPro.DM.Endpoints.Transmitters.DmTx201S
#else
	public abstract class AbstractDmTx201SAdapter<TSettings> :
		AbstractDmTx200BaseAdapter<TSettings>, IDmTx201SAdapter
#endif
		where TSettings : IDmTx201SAdapterSettings, new()
	{
	}

	public abstract class AbstractDmTx201SAdapterSettings : AbstractDmTx200BaseAdapterSettings, IDmTx201SAdapterSettings
	{
	}

	public interface IDmTx201SAdapter : IDmTx200BaseAdapter
	{
	}

	public interface IDmTx201SAdapterSettings : IDmTx200BaseAdapterSettings
	{
	}
}
