using System;
using ICD.Connect.Settings.Attributes;
#if SIMPLSHARP
using Crestron.SimplSharpPro;
using Crestron.SimplSharpPro.DM;
#endif

namespace ICD.Connect.Routing.CrestronPro.Transmitters.DmTx201S
{
	/// <summary>
	/// DmTx201CAdapter wraps a DmTx201C to provide a routing device.
	/// </summary>
#if SIMPLSHARP
	public sealed class DmTx201CAdapter :
		AbstractDmTx201SAdapter<Crestron.SimplSharpPro.DM.Endpoints.Transmitters.DmTx201C, DmTx201CAdapterSettings>
	{

		public override Crestron.SimplSharpPro.DM.Endpoints.Transmitters.DmTx201C InstantiateTransmitter(byte ipid,
		                                                                                                    CrestronControlSystem
			                                                                                                    controlSystem)
		{
			return new Crestron.SimplSharpPro.DM.Endpoints.Transmitters.DmTx201C(ipid, controlSystem);
		}

		public override Crestron.SimplSharpPro.DM.Endpoints.Transmitters.DmTx201C InstantiateTransmitter(byte ipid,
		                                                                                                    DMInput input)
		{
			return new Crestron.SimplSharpPro.DM.Endpoints.Transmitters.DmTx201C(ipid, input);
		}

		public override Crestron.SimplSharpPro.DM.Endpoints.Transmitters.DmTx201C InstantiateTransmitter(DMInput input)
		{
			return new Crestron.SimplSharpPro.DM.Endpoints.Transmitters.DmTx201C(input);
		}
	}
#else
    public sealed class DmTx201CAdapter : AbstractDmTx201SAdapter<DmTx201CAdapterSettings>
    {
    }
#endif

	[KrangSettings("DmTx201C", typeof(DmTx201CAdapter))]
	public sealed class DmTx201CAdapterSettings : AbstractDmTx201SAdapterSettings
	{
	}
}
