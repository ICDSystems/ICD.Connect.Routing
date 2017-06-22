using Crestron.SimplSharpPro;
using Crestron.SimplSharpPro.DM;
using ICD.Connect.Routing.CrestronPro.Transmitters.DmTx200Base;

namespace ICD.Connect.Routing.CrestronPro.Transmitters.DmTx200C2G
{
	/// <summary>
	/// DmTx200C2GAdapter wraps a DmTx200C2G to provide a routing device.
	/// </summary>
	public sealed class DmTx200C2GAdapter : AbstractDmTx200BaseAdapter<global::Crestron.SimplSharpPro.DM.Endpoints.Transmitters.DmTx200C2G, DmTx200C2GAdapterSettings>
	{
		/// <summary>
		/// Constructor.
		/// </summary>
		public DmTx200C2GAdapter()
		{
			Controls.Add(new DmTx200C2GSourceControl(this));
		}

		protected override global::Crestron.SimplSharpPro.DM.Endpoints.Transmitters.DmTx200C2G InstantiateTransmitter(byte ipid, CrestronControlSystem controlSystem)
		{
			return new global::Crestron.SimplSharpPro.DM.Endpoints.Transmitters.DmTx200C2G(ipid, controlSystem);
		}

		protected override global::Crestron.SimplSharpPro.DM.Endpoints.Transmitters.DmTx200C2G InstantiateTransmitter(byte ipid, DMInput input)
		{
			return new global::Crestron.SimplSharpPro.DM.Endpoints.Transmitters.DmTx200C2G(ipid, input);
		}

		protected override global::Crestron.SimplSharpPro.DM.Endpoints.Transmitters.DmTx200C2G InstantiateTransmitter(DMInput input)
		{
			return new global::Crestron.SimplSharpPro.DM.Endpoints.Transmitters.DmTx200C2G(input);
		}
	}
}
