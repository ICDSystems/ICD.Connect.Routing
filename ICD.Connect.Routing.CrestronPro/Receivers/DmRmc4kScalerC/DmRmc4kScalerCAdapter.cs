using ICD.Connect.Routing.CrestronPro.Receivers.AbstractDmRmc4kScalerC;
#if !NETSTANDARD
using Crestron.SimplSharpPro;
using Crestron.SimplSharpPro.DM;
#endif

namespace ICD.Connect.Routing.CrestronPro.Receivers.DmRmc4kScalerC
{
	/// <summary>
	/// DmRmcScalerCAdapter wraps a DmRmcScalerC to provide a routing device.
	/// </summary>
#if !NETSTANDARD
	// ReSharper disable once InconsistentNaming
	public sealed class DmRmc4kScalerCAdapter :
		AbstractDmRmc4KScalerCAdapter
			<Crestron.SimplSharpPro.DM.Endpoints.Receivers.DmRmc4kScalerC, DmRmc4kScalerCAdapterSettings>
	{
		public override Crestron.SimplSharpPro.DM.Endpoints.Receivers.DmRmc4kScalerC InstantiateReceiver(byte ipid,
																										 CrestronControlSystem
																											 controlSystem)
		{
			return new Crestron.SimplSharpPro.DM.Endpoints.Receivers.DmRmc4kScalerC(ipid, controlSystem);
		}

		public override Crestron.SimplSharpPro.DM.Endpoints.Receivers.DmRmc4kScalerC InstantiateReceiver(byte ipid,
																										 DMOutput output)
		{
			return new Crestron.SimplSharpPro.DM.Endpoints.Receivers.DmRmc4kScalerC(ipid, output);
		}

		public override Crestron.SimplSharpPro.DM.Endpoints.Receivers.DmRmc4kScalerC InstantiateReceiver(DMOutput output)
		{
			return new Crestron.SimplSharpPro.DM.Endpoints.Receivers.DmRmc4kScalerC(output);
		}
	}


#else
	// ReSharper disable once InconsistentNaming
	public sealed class DmRmc4kScalerCAdapter : AbstractDmRmc4kScalerCAdapter<DmRmc4kScalerCAdapterSettings>
    {
    }
#endif
}
