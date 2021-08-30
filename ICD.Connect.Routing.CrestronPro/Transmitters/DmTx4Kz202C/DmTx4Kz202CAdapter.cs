using ICD.Connect.Routing.CrestronPro.Transmitters.DmTx4KzX02CBase;
#if !NETSTANDARD
using Crestron.SimplSharpPro;
using Crestron.SimplSharpPro.DM;
using Crestron.SimplSharpPro.DM.Endpoints.Transmitters;
#endif

namespace ICD.Connect.Routing.CrestronPro.Transmitters.DmTx4Kz202C
{
#if !NETSTANDARD
	public sealed class DmTx4Kz202CAdapter : AbstractDmTx4kzX02CBaseAdapter<DmTx4kz202C, DmTx4Kz202CAdapterSettings>
#else
	public sealed class DmTx4Kz202CAdapter : AbstractDmTx4kzX02CBaseAdapter<DmTx4Kz202CAdapterSettings>
#endif
	{
		#region Settings

#if !NETSTANDARD
		public override DmTx4kz202C InstantiateTransmitter(byte ipid, CrestronControlSystem controlSystem)
		{
			return new DmTx4kz202C(ipid, controlSystem);
		}

		public override DmTx4kz202C InstantiateTransmitter(byte ipid, DMInput input)
		{
			return new DmTx4kz202C(ipid, input);
		}

		public override DmTx4kz202C InstantiateTransmitter(DMInput input)
		{
			return new DmTx4kz202C(input);
		}
#endif

		#endregion
	}
}
