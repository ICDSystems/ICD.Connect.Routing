﻿#if SIMPLSHARP
using Crestron.SimplSharpPro;
using Crestron.SimplSharpPro.DM;
using ICD.Connect.Misc.CrestronPro.Devices;
using ICD.Common.Utils;
#endif
using ICD.Connect.Routing.CrestronPro.Receivers.DmRmcScalerCBase;

namespace ICD.Connect.Routing.CrestronPro.Receivers.DmRmc4kScalerC
{
    /// <summary>
    /// DmRmcScalerCAdapter wraps a DmRmcScalerC to provide a routing device.
    /// </summary>
#if SIMPLSHARP
	public sealed class DmRmc4kScalerCAdapter : AbstractDmRmcScalerCAdapter<Crestron.SimplSharpPro.DM.Endpoints.Receivers.DmRmc4kScalerC, DmRmc4kScalerCAdapterSettings>, IPortParent
	{
		protected override Crestron.SimplSharpPro.DM.Endpoints.Receivers.DmRmc4kScalerC InstantiateScaler(byte ipid, CrestronControlSystem controlSystem)
		{
			return new Crestron.SimplSharpPro.DM.Endpoints.Receivers.DmRmc4kScalerC(ipid, controlSystem);
		}

		protected override Crestron.SimplSharpPro.DM.Endpoints.Receivers.DmRmc4kScalerC InstantiateScaler(byte ipid, DMOutput output)
		{
			return new Crestron.SimplSharpPro.DM.Endpoints.Receivers.DmRmc4kScalerC(ipid, output);
		}

		protected override Crestron.SimplSharpPro.DM.Endpoints.Receivers.DmRmc4kScalerC InstantiateScaler(DMOutput output)
		{
			IcdConsole.PrintLine("{0} {1} {2} {3}", output, output.Number, output.Name, output.EndpointOnlineFeedback);


			return new Crestron.SimplSharpPro.DM.Endpoints.Receivers.DmRmc4kScalerC(output);
		}
	}
#else
    public sealed class DmRmc4kScalerCAdapter : AbstractDmRmcScalerCAdapter<DmRmc4kScalerCAdapterSettings>
    {
    }
#endif
}
