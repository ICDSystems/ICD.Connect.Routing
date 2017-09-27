using System;
#if SIMPLSHARP
using Crestron.SimplSharpPro;
using Crestron.SimplSharpPro.DM;
#endif
using ICD.Common.Properties;
using ICD.Connect.Settings.Attributes.Factories;

namespace ICD.Connect.Routing.CrestronPro.Transmitters.DmTx201S
{
#if SIMPLSHARP
	public sealed class DmTx201SAdapter :
		AbstractDmTx201SAdapter<Crestron.SimplSharpPro.DM.Endpoints.Transmitters.DmTx201S, DmTx201SAdapterSettings>
	{
		/// <summary>
		/// Constructor.
		/// </summary>
		public DmTx201SAdapter()
		{
			Controls.Add(new DmTx201SSourceControl(this));
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
	public sealed class DmTx201SAdapter : AbstractDmTx201SAdapter<DmTx201SAdapterSettings>
	{
	}
#endif

	public sealed class DmTx201SAdapterSettings : AbstractDmTx201SAdapterSettings
	{
		private const string FACTORY_NAME = "DmTx201S";

		/// <summary>
		/// Gets the originator factory name.
		/// </summary>
		public override string FactoryName { get { return FACTORY_NAME; } }

		/// <summary>
		/// Gets the type of the originator for this settings instance.
		/// </summary>
		public override Type OriginatorType { get { return typeof(DmTx201SAdapter); } }

		/// <summary>
		/// Loads the settings from XML.
		/// </summary>
		/// <param name="xml"></param>
		/// <returns></returns>
		[PublicAPI, XmlDeviceSettingsFactoryMethod(FACTORY_NAME)]
		public static DmTx201SAdapterSettings FromXml(string xml)
		{
			DmTx201SAdapterSettings output = new DmTx201SAdapterSettings();
			ParseXml(output, xml);
			return output;
		}
	}
}
