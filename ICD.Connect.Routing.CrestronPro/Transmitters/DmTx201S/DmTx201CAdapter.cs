using System;
using ICD.Common.Properties;
using ICD.Connect.Settings.Attributes;
#if SIMPLSHARP
using Crestron.SimplSharpPro;
using Crestron.SimplSharpPro.DM;
#endif

namespace ICD.Connect.Routing.CrestronPro.Transmitters.DmTx201S
{
	/// <summary>
	/// DmTx201CAdapter wraps a DmTx201C to provide a routing device.
	/// </summary>
#if SIMPLSHARP
	public sealed class DmTx201CAdapter :
		AbstractDmTx201SAdapter<Crestron.SimplSharpPro.DM.Endpoints.Transmitters.DmTx201C, DmTx201CAdapterSettings>
	{
		/// <summary>
		/// Constructor.
		/// </summary>
		public DmTx201CAdapter()
		{
			Controls.Add(new DmTx201CSourceControl(this));
		}

		protected override Crestron.SimplSharpPro.DM.Endpoints.Transmitters.DmTx201C InstantiateTransmitter(byte ipid,
		                                                                                                    CrestronControlSystem
			                                                                                                    controlSystem)
		{
			return new Crestron.SimplSharpPro.DM.Endpoints.Transmitters.DmTx201C(ipid, controlSystem);
		}

		protected override Crestron.SimplSharpPro.DM.Endpoints.Transmitters.DmTx201C InstantiateTransmitter(byte ipid,
		                                                                                                    DMInput input)
		{
			return new Crestron.SimplSharpPro.DM.Endpoints.Transmitters.DmTx201C(ipid, input);
		}

		protected override Crestron.SimplSharpPro.DM.Endpoints.Transmitters.DmTx201C InstantiateTransmitter(DMInput input)
		{
			return new Crestron.SimplSharpPro.DM.Endpoints.Transmitters.DmTx201C(input);
		}
	}
#else
    public sealed class DmTx201CAdapter : AbstractDmTx201SAdapter<DmTx201CAdapterSettings>
    {
    }
#endif

	public sealed class DmTx201CAdapterSettings : AbstractDmTx201SAdapterSettings
	{
		private const string FACTORY_NAME = "DmTx201C";

		/// <summary>
		/// Gets the originator factory name.
		/// </summary>
		public override string FactoryName { get { return FACTORY_NAME; } }

		/// <summary>
		/// Gets the type of the originator for this settings instance.
		/// </summary>
		public override Type OriginatorType { get { return typeof(DmTx201CAdapter); } }

		/// <summary>
		/// Loads the settings from XML.
		/// </summary>
		/// <param name="xml"></param>
		/// <returns></returns>
		[PublicAPI, XmlFactoryMethod(FACTORY_NAME)]
		public static DmTx201CAdapterSettings FromXml(string xml)
		{
			DmTx201CAdapterSettings output = new DmTx201CAdapterSettings();
			ParseXml(output, xml);
			return output;
		}
	}
}
