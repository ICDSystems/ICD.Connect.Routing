using System;
using ICD.Common.Properties;
using ICD.Connect.Routing.CrestronPro.Transmitters.DmTx200Base;
using ICD.Connect.Settings.Attributes;

namespace ICD.Connect.Routing.CrestronPro.Transmitters.DmTx200C2G
{
	public sealed class DmTx200C2GAdapterSettings : AbstractDmTx200BaseAdapterSettings
	{
		private const string FACTORY_NAME = "DmTx200C2G";

		/// <summary>
		/// Gets the originator factory name.
		/// </summary>
		public override string FactoryName { get { return FACTORY_NAME; } }

		/// <summary>
		/// Gets the type of the originator for this settings instance.
		/// </summary>
		public override Type OriginatorType { get { return typeof(DmTx200C2GAdapter); } }

		/// <summary>
		/// Loads the settings from XML.
		/// </summary>
		/// <param name="xml"></param>
		/// <returns></returns>
		[PublicAPI, XmlFactoryMethod(FACTORY_NAME)]
		public static DmTx200C2GAdapterSettings FromXml(string xml)
		{
			DmTx200C2GAdapterSettings output = new DmTx200C2GAdapterSettings();
			output.ParseXml(xml);
			return output;
		}
	}
}
