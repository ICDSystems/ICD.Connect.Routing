using System;
#if SIMPLSHARP
using Crestron.SimplSharpPro;
#endif

namespace ICD.Connect.Routing.CrestronPro.Receivers.DmRmcScalerCBase
{
	/// <summary>
	/// DmRmcScalerCAdapter wraps a DmRmcScalerC to provide a routing device.
	/// </summary>
#if SIMPLSHARP
	public abstract class AbstractDmRmcScalerCAdapter<TScaler, TSettings> : AbstractEndpointReceiverBaseAdapter<TScaler, TSettings>,
	                                                                        IDmRmcScalerCAdapter
		where TScaler : Crestron.SimplSharpPro.DM.Endpoints.Receivers.DmRmcScalerC
#else
    public abstract class AbstractDmRmcScalerCAdapter<TSettings> : AbstractEndpointReceiverBaseAdapter<TSettings>
#endif
		where TSettings : IDmRmcScalerCAdapterSettings, new()
	{
#if SIMPLSHARP

		protected AbstractDmRmcScalerCAdapter()
		{
			Controls.Add(new DmRmcScalerCBaseRouteControl<IDmRmcScalerCAdapter, TScaler>(this, 0));
		} 

		/// <summary>
		/// Gets the port at the given addres.
		/// </summary>
		/// <param name="address"></param>
		/// <returns></returns>
		public override ComPort GetComPort(int address)
		{
			if (Receiver == null)
				throw new InvalidOperationException("No scaler instantiated");

			if (address == 1)
				return Receiver.ComPorts[1];

			return base.GetComPort(address);
		}

		/// <summary>
		/// Gets the port at the given addres.
		/// </summary>
		/// <param name="address"></param>
		/// <returns></returns>
		public override IROutputPort GetIrOutputPort(int address)
		{
			if (Receiver == null)
				throw new InvalidOperationException("No scaler instantiated");

			if (address == 1)
				return Receiver.IROutputPorts[1];

			return base.GetIrOutputPort(address);
		}
#endif
	}
}
