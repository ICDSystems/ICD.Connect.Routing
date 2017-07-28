using Crestron.SimplSharpPro;
using Crestron.SimplSharpPro.DM;
using ICD.Connect.Routing.CrestronPro.Receivers.DmRmcScalerCBase;

namespace ICD.Connect.Routing.CrestronPro.Receivers.DmRmcScalerC
{
	/// <summary>
	/// DmRmcScalerCAdapter wraps a DmRmcScalerC to provide a routing device.
	/// </summary>
	public sealed class DmRmcScalerCAdapter : AbstractDmRmcScalerCAdapter<Crestron.SimplSharpPro.DM.Endpoints.Receivers.DmRmcScalerC, DmRmcScalerCAdapterSettings>
	{
		protected override Crestron.SimplSharpPro.DM.Endpoints.Receivers.DmRmcScalerC InstantiateScaler(byte ipid, CrestronControlSystem controlSystem)
		{
			return new Crestron.SimplSharpPro.DM.Endpoints.Receivers.DmRmcScalerC(ipid, controlSystem);
		}

		protected override Crestron.SimplSharpPro.DM.Endpoints.Receivers.DmRmcScalerC InstantiateScaler(byte ipid, DMOutput output)
		{
			return new Crestron.SimplSharpPro.DM.Endpoints.Receivers.DmRmcScalerC(ipid, output);
		}

		protected override Crestron.SimplSharpPro.DM.Endpoints.Receivers.DmRmcScalerC InstantiateScaler(DMOutput output)
		{
			return new Crestron.SimplSharpPro.DM.Endpoints.Receivers.DmRmcScalerC(output);
		}
	}
}
