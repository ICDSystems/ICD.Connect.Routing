using Crestron.SimplSharpPro;
using Crestron.SimplSharpPro.DM;
using ICD.Connect.Routing.CrestronPro.Receivers.DmRmc100CBase;

namespace ICD.Connect.Routing.CrestronPro.Receivers.DmRmc100C
{
#if SIMPLSHARP
	public sealed class DmRmc100CAdapter :
		AbstractDmRmc100CBaseAdapter<Crestron.SimplSharpPro.DM.Endpoints.Receivers.DmRmc100C, DmRmc100CAdapterSettings>
#else
	public sealed class DmRmc100CAdapter : AbstractDmRmc100CBaseAdapter<DmRmc100CAdapterSettings>
#endif
	{
#if SIMPLSHARP
		public override Crestron.SimplSharpPro.DM.Endpoints.Receivers.DmRmc100C InstantiateReceiver(byte ipid,
		                                                                                            CrestronControlSystem
			                                                                                            controlSystem)
		{
			return new Crestron.SimplSharpPro.DM.Endpoints.Receivers.DmRmc100C(ipid, controlSystem);
		}

		public override Crestron.SimplSharpPro.DM.Endpoints.Receivers.DmRmc100C InstantiateReceiver(byte ipid, DMOutput output)
		{
			return new Crestron.SimplSharpPro.DM.Endpoints.Receivers.DmRmc100C(ipid, output);
		}

		public override Crestron.SimplSharpPro.DM.Endpoints.Receivers.DmRmc100C InstantiateReceiver(DMOutput output)
		{
			return new Crestron.SimplSharpPro.DM.Endpoints.Receivers.DmRmc100C(output);
		}
	}

#endif
}
