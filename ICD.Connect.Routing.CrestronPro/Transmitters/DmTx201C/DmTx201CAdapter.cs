#if SIMPLSHARP
using Crestron.SimplSharpPro;
using Crestron.SimplSharpPro.DM;
#endif
using ICD.Connect.Routing.CrestronPro.Transmitters.DmTx200Base;

namespace ICD.Connect.Routing.CrestronPro.Transmitters.DmTx201C
{
    /// <summary>
    /// DmTx201CAdapter wraps a DmTx201C to provide a routing device.
    /// </summary>
#if SIMPLSHARP
	public sealed class DmTx201CAdapter : AbstractDmTx200BaseAdapter<Crestron.SimplSharpPro.DM.Endpoints.Transmitters.DmTx201C, DmTx201CAdapterSettings>
	{
		/// <summary>
		/// Constructor.
		/// </summary>
		public DmTx201CAdapter()
		{
			Controls.Add(new DmTx201CSourceControl(this));
		}

		protected override Crestron.SimplSharpPro.DM.Endpoints.Transmitters.DmTx201C InstantiateTransmitter(byte ipid, CrestronControlSystem controlSystem)
		{
			return new Crestron.SimplSharpPro.DM.Endpoints.Transmitters.DmTx201C(ipid, controlSystem);
		}

		protected override Crestron.SimplSharpPro.DM.Endpoints.Transmitters.DmTx201C InstantiateTransmitter(byte ipid, DMInput input)
		{
			return new Crestron.SimplSharpPro.DM.Endpoints.Transmitters.DmTx201C(ipid, input);
		}

		protected override Crestron.SimplSharpPro.DM.Endpoints.Transmitters.DmTx201C InstantiateTransmitter(DMInput input)
		{
			return new Crestron.SimplSharpPro.DM.Endpoints.Transmitters.DmTx201C(input);
		}
	}
#else
    public sealed class DmTx201CAdapter : AbstractDmTx200BaseAdapter<DmTx201CAdapterSettings>
    {
    }
#endif
}
