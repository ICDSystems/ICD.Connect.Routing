#if SIMPLSHARP
using Crestron.SimplSharpPro;
using Crestron.SimplSharpPro.DM;
#endif
using ICD.Connect.Routing.CrestronPro.Transmitters.DmTx4KzX02CBase;

namespace ICD.Connect.Routing.CrestronPro.Transmitters.DmTx4kz302C
{
#if SIMPLSHARP
	public sealed class DmTx4Kz302CAdapter : AbstractDmTx4kzX02CBaseAdapter<Crestron.SimplSharpPro.DM.Endpoints.Transmitters.DmTx4kz302C, DmTx4Kz302CAdapterSettings>
#else
	public sealed class DmTx4Kz302CAdapter : AbstractDmTx4kzX02CBaseAdapter<DmTx4Kz302CAdapterSettings>
#endif
	{
		#region Settings

#if SIMPLSHARP
		public override Crestron.SimplSharpPro.DM.Endpoints.Transmitters.DmTx4kz302C InstantiateTransmitter(byte ipid, CrestronControlSystem controlSystem)
		{
			return new Crestron.SimplSharpPro.DM.Endpoints.Transmitters.DmTx4kz302C(ipid, controlSystem);
		}

		public override Crestron.SimplSharpPro.DM.Endpoints.Transmitters.DmTx4kz302C InstantiateTransmitter(byte ipid, DMInput input)
		{
			return new Crestron.SimplSharpPro.DM.Endpoints.Transmitters.DmTx4kz302C(ipid, input);
		}

		public override Crestron.SimplSharpPro.DM.Endpoints.Transmitters.DmTx4kz302C InstantiateTransmitter(DMInput input)
		{
			return new Crestron.SimplSharpPro.DM.Endpoints.Transmitters.DmTx4kz302C(input);
		}
#endif

		#endregion
	}
}
