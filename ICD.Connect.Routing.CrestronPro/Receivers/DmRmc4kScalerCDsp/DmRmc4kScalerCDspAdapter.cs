using System;
using System.Collections.Generic;
using ICD.Connect.Routing.CrestronPro.Receivers.DmRmcScalerCBase;
#if SIMPLSHARP
using Crestron.SimplSharpPro;
using Crestron.SimplSharpPro.DM;
#endif

namespace ICD.Connect.Routing.CrestronPro.Receivers.DmRmc4kScalerCDsp
{
	/// <summary>
	/// DmRmc4kScalerCDspAdapter wraps a DmRmc4kScalerCDsp to provide a routing device.
	/// </summary>
#if SIMPLSHARP
// ReSharper disable once InconsistentNaming
	public sealed class DmRmc4kScalerCDspAdapter :
		AbstractDmRmcScalerCAdapter
			<Crestron.SimplSharpPro.DM.Endpoints.Receivers.DmRmc4kScalerCDsp, DmRmc4kScalerCDspAdapterSettings>
	{
		/// <summary>
		/// Gets the port at the given address.
		/// </summary>
		/// <param name="address"></param>
		/// <returns></returns>
		public override Relay GetRelayPort(int address)
		{
			if (Scaler == null)
				throw new InvalidOperationException("No scaler instantiated");

			if (address == 1)
				return Scaler.RelayPorts[1];

			string message = string.Format("{0} has no {1} with address {2}", this, typeof(IROutputPort).Name, address);
			throw new KeyNotFoundException(message);
		}

		protected override Crestron.SimplSharpPro.DM.Endpoints.Receivers.DmRmc4kScalerCDsp InstantiateScaler(byte ipid,
		                                                                                                     CrestronControlSystem
			                                                                                                     controlSystem)
		{
			return new Crestron.SimplSharpPro.DM.Endpoints.Receivers.DmRmc4kScalerCDsp(ipid, controlSystem);
		}

		protected override Crestron.SimplSharpPro.DM.Endpoints.Receivers.DmRmc4kScalerCDsp InstantiateScaler(byte ipid,
		                                                                                                     DMOutput output)
		{
			return new Crestron.SimplSharpPro.DM.Endpoints.Receivers.DmRmc4kScalerCDsp(ipid, output);
		}

		protected override Crestron.SimplSharpPro.DM.Endpoints.Receivers.DmRmc4kScalerCDsp InstantiateScaler(DMOutput output)
		{
			return new Crestron.SimplSharpPro.DM.Endpoints.Receivers.DmRmc4kScalerCDsp(output);
		}
	}
#else
    public sealed class DmRmc4kScalerCDspAdapter : AbstractDmRmcScalerCAdapter<DmRmc4kScalerCDspAdapterSettings>
    {
    }
#endif
}
