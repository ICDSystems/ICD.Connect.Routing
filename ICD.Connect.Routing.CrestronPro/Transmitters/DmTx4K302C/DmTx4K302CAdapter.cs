#if SIMPLSHARP
using Crestron.SimplSharpPro;
using Crestron.SimplSharpPro.DM;
using Crestron.SimplSharpPro.DM.Endpoints.Transmitters;

#else
using System;
#endif

namespace ICD.Connect.Routing.CrestronPro.Transmitters.DmTx4k302C
{
#if SIMPLSHARP
	public sealed class DmTx4k302CAdapter : AbstractEndpointTransmitterBaseAdapter<DmTx4k302C, DmTx4k302CAdapterSettings>
#else
    public sealed class DmTx4k302CAdapter : AbstractEndpointTransmitterBaseAdapter<DmTx4k302C>
#endif
	{
		/// <summary>
		/// Constructor.
		/// </summary>
		public DmTx4k302CAdapter()
		{
#if SIMPLSHARP
			Controls.Add(new DmTx4k302CSourceControl(this));
#endif
		}

		#region Settings

#if SIMPLSHARP
		protected override DmTx4k302C InstantiateTransmitter(byte ipid, CrestronControlSystem controlSystem)
		{
			return new DmTx4k302C(ipid, controlSystem);
		}

		protected override DmTx4k302C InstantiateTransmitter(byte ipid, DMInput input)
		{
			return new DmTx4k302C(ipid, input);
		}

		protected override DmTx4k302C InstantiateTransmitter(DMInput input)
		{
			return new DmTx4k302C(input);
		}
#endif

		#endregion
	}
}
