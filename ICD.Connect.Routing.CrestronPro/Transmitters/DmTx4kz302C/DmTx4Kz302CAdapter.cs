using ICD.Connect.Routing.CrestronPro.Transmitters.DmTx4kzX02CBase;
#if SIMPLSHARP
using Crestron.SimplSharpPro;
using Crestron.SimplSharpPro.DM;
using Crestron.SimplSharpPro.DM.Endpoints.Transmitters;
#endif

namespace ICD.Connect.Routing.CrestronPro.Transmitters.DmTx4Kz302C
{
#if SIMPLSHARP
	public sealed class DmTx4Kz302CAdapter : AbstractDmTx4kzX02CBaseAdapter<DmTx4kz302C, DmTx4Kz302CAdapterSettings>
#else
	public sealed class DmTx4Kz302CAdapter : AbstractDmTx4kzX02CBaseAdapter<DmTx4Kz302CAdapterSettings>
#endif
	{
		#region Settings

#if SIMPLSHARP
		public override DmTx4kz302C InstantiateTransmitter(byte ipid, CrestronControlSystem controlSystem)
		{
			return new DmTx4kz302C(ipid, controlSystem);
		}

		public override DmTx4kz302C InstantiateTransmitter(byte ipid, DMInput input)
		{
			return new DmTx4kz302C(ipid, input);
		}

		public override DmTx4kz302C InstantiateTransmitter(DMInput input)
		{
			return new DmTx4kz302C(input);
		}
#endif

		#endregion
	}
}
