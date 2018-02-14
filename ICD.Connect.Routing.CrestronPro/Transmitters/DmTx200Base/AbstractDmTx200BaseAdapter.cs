namespace ICD.Connect.Routing.CrestronPro.Transmitters.DmTx200Base
{
	/// <summary>
	/// Base class for DmTx200 device adapters.
	/// </summary>
	/// <typeparam name="TTransmitter"></typeparam>
	/// <typeparam name="TSettings"></typeparam>
#if SIMPLSHARP
	public abstract class AbstractDmTx200BaseAdapter<TTransmitter, TSettings> :
		AbstractEndpointTransmitterBaseAdapter<TTransmitter, TSettings>, IDmTx200BaseAdapter
		where TTransmitter : Crestron.SimplSharpPro.DM.Endpoints.Transmitters.DmTx200Base
#else
    public abstract class AbstractDmTx200BaseAdapter<TSettings> : AbstractEndpointTransmitterBaseAdapter<TSettings>, IDmTx200BaseAdapter
#endif
		where TSettings : IDmTx200BaseAdapterSettings, new()
	{
#if SIMPLSHARP
		/// <summary>
		/// Called when the wrapped transmitter is assigned.
		/// </summary>
		/// <param name="transmitter"></param>
		protected override void ConfigureTransmitter(TTransmitter transmitter)
		{
			if (transmitter == null)
				return;

			transmitter.VideoSource = Crestron.SimplSharpPro.DM.Endpoints.Transmitters.DmTx200Base.eSourceSelection.Auto;
		}
#endif
	}

	public abstract class AbstractDmTx200BaseAdapterSettings : AbstractEndpointTransmitterBaseAdapterSettings,
	                                                           IDmTx200BaseAdapterSettings
	{
	}

	public interface IDmTx200BaseAdapter : IEndpointTransmitterBaseAdapter
	{
	}

	public interface IDmTx200BaseAdapterSettings : IEndpointTransmitterBaseAdapterSettings
	{
	}
}
