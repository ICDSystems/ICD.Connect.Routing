#if SIMPLSHARP
using Crestron.SimplSharpPro;
using Crestron.SimplSharpPro.DM;
using Crestron.SimplSharpPro.DM.Endpoints.Transmitters;
#endif
using ICD.Connect.Routing.CrestronPro.Transmitters.DmTx200Base;

namespace ICD.Connect.Routing.CrestronPro.Transmitters.DmTx4Kz100C1G
{
	/// <summary>
	/// DmTx4Kz100C1GAdapter wraps a DmTx4kz100C1G to provide a routing device.
	/// </summary>
#if SIMPLSHARP
	public sealed class DmTx4Kz100C1GAdapter : AbstractDmTx200BaseAdapter<DmTx4kz100C1G, DmTx4Kz100C1GAdapterSettings>
	{
		/// <summary>
		/// Instantiates the transmitter with the given IPID against the control system.
		/// </summary>
		/// <param name="ipid"></param>
		/// <param name="controlSystem"></param>
		/// <returns></returns>
		public override DmTx4kz100C1G InstantiateTransmitter(byte ipid, CrestronControlSystem controlSystem)
		{
			return new DmTx4kz100C1G(ipid, controlSystem);
		}

		/// <summary>
		/// Instantiates the transmitter against the given DM Input and configures it with the given IPID.
		/// </summary>
		/// <param name="ipid"></param>
		/// <param name="input"></param>
		/// <returns></returns>
		public override DmTx4kz100C1G InstantiateTransmitter(byte ipid, DMInput input)
		{
			return new DmTx4kz100C1G(ipid, input);
		}

		/// <summary>
		/// Instantiates the transmitter against the given DM Input.
		/// </summary>
		/// <param name="input"></param>
		/// <returns></returns>
		public override DmTx4kz100C1G InstantiateTransmitter(DMInput input)
		{
			return new DmTx4kz100C1G(input);
		}
	}
#else
	public sealed class DmTx4Kz100C1GAdapter : AbstractDmTx200BaseAdapter<DmTx4Kz100C1GAdapterSettings>
	{
	}
#endif
}