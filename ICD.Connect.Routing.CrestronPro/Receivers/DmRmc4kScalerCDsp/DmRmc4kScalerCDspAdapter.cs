using System;
using System.Collections.Generic;
using System.Linq;
using ICD.Common.Utils;
using ICD.Common.Utils.Extensions;
using ICD.Connect.Routing.Connections;
using ICD.Connect.Routing.CrestronPro.Receivers.DmRmcScalerCBase;
using ICD.Connect.Routing.EventArguments;
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
		AbstractDmRmcScalerCBaseAdapter
			<Crestron.SimplSharpPro.DM.Endpoints.Receivers.DmRmc4kScalerCDsp, DmRmc4kScalerCDspAdapterSettings>
	{
		/// <summary>
		/// Gets the port at the given address.
		/// </summary>
		/// <param name="address"></param>
		/// <returns></returns>
		public override Relay GetRelayPort(int address)
		{
			if (Receiver == null)
				throw new InvalidOperationException("No scaler instantiated");

			if (address == 1)
				return Receiver.RelayPorts[1];

			return base.GetRelayPort(address);
		}

		public override Crestron.SimplSharpPro.DM.Endpoints.Receivers.DmRmc4kScalerCDsp InstantiateReceiver(byte ipid,
		                                                                                                    CrestronControlSystem
			                                                                                                    controlSystem)
		{
			return new Crestron.SimplSharpPro.DM.Endpoints.Receivers.DmRmc4kScalerCDsp(ipid, controlSystem);
		}

		public override Crestron.SimplSharpPro.DM.Endpoints.Receivers.DmRmc4kScalerCDsp InstantiateReceiver(byte ipid,
		                                                                                                    DMOutput output)
		{
			return new Crestron.SimplSharpPro.DM.Endpoints.Receivers.DmRmc4kScalerCDsp(ipid, output);
		}

		public override Crestron.SimplSharpPro.DM.Endpoints.Receivers.DmRmc4kScalerCDsp InstantiateReceiver(DMOutput output)
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
