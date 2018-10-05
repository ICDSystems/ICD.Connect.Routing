using ICD.Connect.Routing.CrestronPro.Transmitters.DmTx4kX02CBase;
#if SIMPLSHARP
using Crestron.SimplSharpPro;
using Crestron.SimplSharpPro.DM;
using Crestron.SimplSharpPro.DM.Endpoints.Transmitters;
#endif

namespace ICD.Connect.Routing.CrestronPro.Transmitters.DmTx4K202C
{
#if SIMPLSHARP
	public sealed class DmTx4K202CAdapter : AbstractDmTx4kX02CBaseAdapter<DmTx4k202C, DmTx4K202CAdapterSettings>
#else
	public sealed class DmTx4K202CAdapter : AbstractDmTx4kX02CBaseAdapter<DmTx4K202CAdapterSettings>
#endif
	{
		#region Settings

#if SIMPLSHARP
		public override DmTx4k202C InstantiateTransmitter(byte ipid, CrestronControlSystem controlSystem)
		{
			return new DmTx4k202C(ipid, controlSystem);
		}

		public override DmTx4k202C InstantiateTransmitter(byte ipid, DMInput input)
		{
			return new DmTx4k202C(ipid, input);
		}

		public override DmTx4k202C InstantiateTransmitter(DMInput input)
		{
			return new DmTx4k202C(input);
		}
#endif

		#endregion
	}
}
