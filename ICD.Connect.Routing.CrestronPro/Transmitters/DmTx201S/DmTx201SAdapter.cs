using ICD.Connect.Settings.Attributes;
#if !NETSTANDARD
using Crestron.SimplSharpPro;
using Crestron.SimplSharpPro.DM;
#endif

namespace ICD.Connect.Routing.CrestronPro.Transmitters.DmTx201S
{
#if !NETSTANDARD
	public sealed class DmTx201SAdapter :
		AbstractDmTx201SAdapter<Crestron.SimplSharpPro.DM.Endpoints.Transmitters.DmTx201S, DmTx201SAdapterSettings>
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
	public sealed class DmTx201SAdapter : AbstractDmTx201SAdapter<DmTx201SAdapterSettings>
	{
	}
#endif

	[KrangSettings("DmTx201S", typeof(DmTx201SAdapter))]
	public sealed class DmTx201SAdapterSettings : AbstractDmTx201SAdapterSettings
	{
	}
}
