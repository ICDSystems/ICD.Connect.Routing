using System;
#if SIMPLSHARP
using Crestron.SimplSharpPro;
#endif

namespace ICD.Connect.Routing.CrestronPro.Receivers.DmRmc100CBase
{
#if SIMPLSHARP
	public abstract class AbstractDmRmc100CBaseAdapter<TReceiver, TSettings> :
		AbstractEndpointReceiverBaseAdapter<TReceiver, TSettings>, IDmRmc100CBaseAdapter
		where TReceiver : Crestron.SimplSharpPro.DM.Endpoints.Receivers.DmRmc100C
#else
	public abstract class AbstractDmRmc100CBaseAdapter<TSettings> :
		AbstractEndpointReceiverBaseAdapter<TSettings>, IDmRmc100CBaseAdapter
#endif
		where TSettings : IDmRmc100CBaseAdapterSettings, new()
	{
		#if SIMPLSHARP

		protected AbstractDmRmc100CBaseAdapter()
		{
			Controls.Add(new DmRmc100CBaseRouteControl<IDmRmc100CBaseAdapter, TReceiver>(this, 0));
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
