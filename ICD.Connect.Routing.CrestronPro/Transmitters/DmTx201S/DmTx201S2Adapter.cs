using ICD.Connect.Settings.Attributes;
#if SIMPLSHARP
using Crestron.SimplSharpPro;
using Crestron.SimplSharpPro.DM;
#endif

namespace ICD.Connect.Routing.CrestronPro.Transmitters.DmTx201S
{
#if SIMPLSHARP
	public sealed class DmTx201S2Adapter :
		AbstractDmTx201SAdapter<Crestron.SimplSharpPro.DM.Endpoints.Transmitters.DmTx201S, DmTx201S2AdapterSettings>
	{
		public override Crestron.SimplSharpPro.DM.Endpoints.Transmitters.DmTx201S InstantiateTransmitter(byte ipid,
		                                                                                                    CrestronControlSystem
			                                                                                                    controlSystem)
		{
			return new Crestron.SimplSharpPro.DM.Endpoints.Transmitters.DmTx201S(ipid, controlSystem);
		}

		public override Crestron.SimplSharpPro.DM.Endpoints.Transmitters.DmTx201S InstantiateTransmitter(byte ipid,
		                                                                                                    DMInput input)
		{
			return new Crestron.SimplSharpPro.DM.Endpoints.Transmitters.DmTx201S(ipid, input);
		}

		public override Crestron.SimplSharpPro.DM.Endpoints.Transmitters.DmTx201S InstantiateTransmitter(DMInput input)
		{
			return new Crestron.SimplSharpPro.DM.Endpoints.Transmitters.DmTx201S(input);
		}
	}
#else
	public sealed class DmTx201S2Adapter : AbstractDmTx201SAdapter<DmTx201S2AdapterSettings>
	{
	}
#endif

	[KrangSettings("DmTx201S2", typeof(DmTx201S2Adapter))]
	public sealed class DmTx201S2AdapterSettings : AbstractDmTx201SAdapterSettings
	{
	}
}
