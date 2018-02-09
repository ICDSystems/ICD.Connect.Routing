using System;
using ICD.Connect.Settings.Attributes;
#if SIMPLSHARP
using Crestron.SimplSharpPro;
using Crestron.SimplSharpPro.DM;
#endif

namespace ICD.Connect.Routing.CrestronPro.Transmitters.DmTx201S
{
#if SIMPLSHARP
	public sealed class DmTx201S2Adapter :
		AbstractDmTx201SAdapter<Crestron.SimplSharpPro.DM.Endpoints.Transmitters.DmTx201S, DmTx201S2AdapterSettings>
	{
		/// <summary>
		/// Constructor.
		/// </summary>
		public DmTx201S2Adapter()
		{
			Controls.Add(new DmTx201S2SourceControl(this));
		}

		protected override Crestron.SimplSharpPro.DM.Endpoints.Transmitters.DmTx201S InstantiateTransmitter(byte ipid,
		                                                                                                    CrestronControlSystem
			                                                                                                    controlSystem)
		{
			return new Crestron.SimplSharpPro.DM.Endpoints.Transmitters.DmTx201S(ipid, controlSystem);
		}

		protected override Crestron.SimplSharpPro.DM.Endpoints.Transmitters.DmTx201S InstantiateTransmitter(byte ipid,
		                                                                                                    DMInput input)
		{
			return new Crestron.SimplSharpPro.DM.Endpoints.Transmitters.DmTx201S(ipid, input);
		}

		protected override Crestron.SimplSharpPro.DM.Endpoints.Transmitters.DmTx201S InstantiateTransmitter(DMInput input)
		{
			return new Crestron.SimplSharpPro.DM.Endpoints.Transmitters.DmTx201S(input);
		}
	}
#else
	public sealed class DmTx201S2Adapter : AbstractDmTx201SAdapter<DmTx201S2AdapterSettings>
	{
	}
#endif

	[KrangSettings(FACTORY_NAME)]
	public sealed class DmTx201S2AdapterSettings : AbstractDmTx201SAdapterSettings
	{
		private const string FACTORY_NAME = "DmTx201S2";

		/// <summary>
		/// Gets the originator factory name.
		/// </summary>
		public override string FactoryName { get { return FACTORY_NAME; } }

		/// <summary>
		/// Gets the type of the originator for this settings instance.
		/// </summary>
		public override Type OriginatorType { get { return typeof(DmTx201S2Adapter); } }
	}
}
