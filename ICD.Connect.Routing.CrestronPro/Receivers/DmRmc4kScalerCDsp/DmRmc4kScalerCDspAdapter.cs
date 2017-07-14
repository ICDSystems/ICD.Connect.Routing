using Crestron.SimplSharpPro;
using Crestron.SimplSharpPro.DM;
using ICD.Connect.Misc.CrestronPro.Devices;
using ICD.Connect.Routing.CrestronPro.Receivers.DmRmcScalerCBase;

namespace ICD.Connect.Routing.CrestronPro.Receivers.DmRmc4kScalerCDsp
{
	/// <summary>
	/// DmRmc4kScalerCDspAdapter wraps a DmRmc4kScalerCDsp to provide a routing device.
	/// </summary>
	public sealed class DmRmc4kScalerCDspAdapter : AbstractDmRmcScalerCAdapter<Crestron.SimplSharpPro.DM.Endpoints.Receivers.DmRmc4kScalerCDsp, DmRmc4kScalerCDspAdapterSettings>, IPortParent
	{
		protected override Crestron.SimplSharpPro.DM.Endpoints.Receivers.DmRmc4kScalerCDsp InstantiateScaler(byte ipid, CrestronControlSystem controlSystem)
		{
			return new Crestron.SimplSharpPro.DM.Endpoints.Receivers.DmRmc4kScalerCDsp(ipid, controlSystem);
		}

		protected override Crestron.SimplSharpPro.DM.Endpoints.Receivers.DmRmc4kScalerCDsp InstantiateScaler(byte ipid, DMOutput output)
		{
			return new Crestron.SimplSharpPro.DM.Endpoints.Receivers.DmRmc4kScalerCDsp(ipid, output);
		}

		protected override Crestron.SimplSharpPro.DM.Endpoints.Receivers.DmRmc4kScalerCDsp InstantiateScaler(DMOutput output)
		{
			return new Crestron.SimplSharpPro.DM.Endpoints.Receivers.DmRmc4kScalerCDsp(output);
		}
	}
}
