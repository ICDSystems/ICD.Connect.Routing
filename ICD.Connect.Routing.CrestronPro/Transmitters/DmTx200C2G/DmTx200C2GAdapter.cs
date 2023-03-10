#if !NETSTANDARD
using Crestron.SimplSharpPro;
using Crestron.SimplSharpPro.DM;
#endif
using ICD.Connect.Routing.CrestronPro.Transmitters.DmTx200Base;

namespace ICD.Connect.Routing.CrestronPro.Transmitters.DmTx200C2G
{
	/// <summary>
	/// DmTx200C2GAdapter wraps a DmTx200C2G to provide a routing device.
	/// </summary>
#if !NETSTANDARD
	public sealed class DmTx200C2GAdapter :
		AbstractDmTx200BaseAdapter<Crestron.SimplSharpPro.DM.Endpoints.Transmitters.DmTx200C2G, DmTx200C2GAdapterSettings>
	{
		public override Crestron.SimplSharpPro.DM.Endpoints.Transmitters.DmTx200C2G InstantiateTransmitter(byte ipid,
		                                                                                                      CrestronControlSystem
			                                                                                                      controlSystem)
		{
			return new Crestron.SimplSharpPro.DM.Endpoints.Transmitters.DmTx200C2G(ipid, controlSystem);
		}

		public override Crestron.SimplSharpPro.DM.Endpoints.Transmitters.DmTx200C2G InstantiateTransmitter(byte ipid,
		                                                                                                      DMInput input)
		{
			return new Crestron.SimplSharpPro.DM.Endpoints.Transmitters.DmTx200C2G(ipid, input);
		}

		public override Crestron.SimplSharpPro.DM.Endpoints.Transmitters.DmTx200C2G InstantiateTransmitter(DMInput input)
		{
			return new Crestron.SimplSharpPro.DM.Endpoints.Transmitters.DmTx200C2G(input);
		}
	}
#else
    public sealed class DmTx200C2GAdapter : AbstractDmTx200BaseAdapter<DmTx200C2GAdapterSettings>
    {
    }
#endif
}
