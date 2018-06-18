using Crestron.SimplSharpPro;
using Crestron.SimplSharpPro.DM;
using ICD.Connect.Routing.CrestronPro.Receivers.DmRmc100CBase;

namespace ICD.Connect.Routing.CrestronPro.Receivers.DmRmc4k100C
{
#if SIMPLSHARP
	public sealed class DmRmc4k100CAdapter : AbstractDmRmc100CBaseAdapter<Crestron.SimplSharpPro.DM.Endpoints.Receivers.DmRmc4k100C, DmRmc4k100CAdapterSettings>
#else
	public sealed class DmRmc4k100CAdapter : AbstractDmRmc4k100CBaseAdapter<DmRmc4k100CAdapterSettings>
#endif
	{
#if SIMPLSHARP
		public override Crestron.SimplSharpPro.DM.Endpoints.Receivers.DmRmc4k100C InstantiateReceiver(byte ipid, CrestronControlSystem controlSystem)
		{
			return new Crestron.SimplSharpPro.DM.Endpoints.Receivers.DmRmc4k100C(ipid, controlSystem);
		}

		public override Crestron.SimplSharpPro.DM.Endpoints.Receivers.DmRmc4k100C InstantiateReceiver(byte ipid, DMOutput output)
		{
			return new Crestron.SimplSharpPro.DM.Endpoints.Receivers.DmRmc4k100C(ipid, output);
		}

		public override Crestron.SimplSharpPro.DM.Endpoints.Receivers.DmRmc4k100C InstantiateReceiver(DMOutput output)
		{
			return new Crestron.SimplSharpPro.DM.Endpoints.Receivers.DmRmc4k100C(output);
		}
#endif
	}
}
