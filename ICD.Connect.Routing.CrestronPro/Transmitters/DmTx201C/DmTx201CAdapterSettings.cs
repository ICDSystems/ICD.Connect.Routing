using System;
using ICD.Common.Properties;
using ICD.Connect.Routing.CrestronPro.Transmitters.DmTx200Base;
using ICD.Connect.Settings.Attributes.Factories;

namespace ICD.Connect.Routing.CrestronPro.Transmitters.DmTx201C
{
	public sealed class DmTx201CAdapterSettings : AbstractDmTx200BaseAdapterSettings
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
		[PublicAPI, XmlDeviceSettingsFactoryMethod(FACTORY_NAME)]
		public static DmTx201CAdapterSettings FromXml(string xml)
		{
			DmTx201CAdapterSettings output = new DmTx201CAdapterSettings();
			ParseXml(output, xml);
			return output;
		}
	}
}
