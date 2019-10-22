using ICD.Connect.Routing.CrestronPro.Receivers.AbstractDmRmcScalerC;
#if SIMPLSHARP
using Crestron.SimplSharpPro;
using Crestron.SimplSharpPro.DM;
#endif

namespace ICD.Connect.Routing.CrestronPro.Receivers.DmRmcScalerC
{
	/// <summary>
	/// DmRmcScalerCAdapter wraps a DmRmcScalerC to provide a routing device.
	/// </summary>
#if SIMPLSHARP
	public sealed class DmRmcScalerCAdapter :
		AbstractDmRmcScalerCAdapter<Crestron.SimplSharpPro.DM.Endpoints.Receivers.DmRmcScalerC, DmRmcScalerCAdapterSettings>
	{
		public override Crestron.SimplSharpPro.DM.Endpoints.Receivers.DmRmcScalerC InstantiateReceiver(byte ipid,
		                                                                                               CrestronControlSystem
			                                                                                               controlSystem)
		{
			return new Crestron.SimplSharpPro.DM.Endpoints.Receivers.DmRmcScalerC(ipid, controlSystem);
		}

		public override Crestron.SimplSharpPro.DM.Endpoints.Receivers.DmRmcScalerC InstantiateReceiver(byte ipid,
		                                                                                               DMOutput output)
		{
			return new Crestron.SimplSharpPro.DM.Endpoints.Receivers.DmRmcScalerC(ipid, output);
		}

		public override Crestron.SimplSharpPro.DM.Endpoints.Receivers.DmRmcScalerC InstantiateReceiver(DMOutput output)
		{
			return new Crestron.SimplSharpPro.DM.Endpoints.Receivers.DmRmcScalerC(output);
		}
	}
#else
    public sealed class DmRmcScalerCAdapter : AbstractDmRmcScalerCAdapter<DmRmcScalerCAdapterSettings>
    {
    }
#endif
}
