using System;
using ICD.Connect.Routing.CrestronPro.Receivers.AbstractDmRmcScalerC;
#if SIMPLSHARP
using Crestron.SimplSharpPro;
#endif

namespace ICD.Connect.Routing.CrestronPro.Receivers.AbstractDmRmc4kScalerC
{
	///<remarks>
	/// The restriction on TScaler is DmRmcScalerC and not DmRmc4kScalerC because the DmRmc4kScalerCDsp
	/// inherits from the DmRmcScalerC and not the DmRmc4kScalerC. The additon of the IRelayPorts
	/// constraint picks up all scalers with relay ports (which is only 4k Scalers at this time).
	/// </remarks>
#if SIMPLSHARP
	// ReSharper disable once InconsistentNaming
	public abstract class AbstractDmRmc4KScalerCAdapter<TScaler, TSettings> : AbstractDmRmcScalerCAdapter<TScaler, TSettings>, IDmRmc4kScalerCAdapter
		where TScaler : Crestron.SimplSharpPro.DM.Endpoints.Receivers.DmRmcScalerC, IRelayPorts
#else
	// ReSharper disable once InconsistentNaming
	public abstract class AbstractDmRmc4kScalerCAdapter<TSettings> : AbstractDmRmcScalerCAdapter<TSettings>, IDmRmc4kScalerCAdapter
#endif
		 where TSettings : IDmRmc4kScalerCAdapterSettings, new()
	{

#if SIMPLSHARP
		/// <summary>
		/// Gets the port at the given address.
		/// </summary>
		/// <param name="address"></param>
		/// <returns></returns>
		public override Relay GetRelayPort(int address)
		{

			if (address < 1 || address > Receiver.NumberOfRelayPorts)
			{
				string message = string.Format("{0} has no {1}", this, typeof(Relay).Name);
				throw new ArgumentOutOfRangeException("address", message);
			}

			return Receiver.RelayPorts[(uint)address];

		}
#endif


	}
}