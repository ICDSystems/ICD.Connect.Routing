using System;
using ICD.Common.Properties;
using ICD.Connect.Settings.Attributes.Factories;

namespace ICD.Connect.Routing.CrestronPro.Cards.Inputs.DmcC
{
	public sealed class DmcCAdapterSettings : AbstractCardSettings
	{
		private const string FACTORY_NAME = "DmcC";

		/// <summary>
		/// Gets the originator factory name.
		/// </summary>
		public override string FactoryName { get { return FACTORY_NAME; } }

		/// <summary>
		/// Gets the type of the originator for this settings instance.
		/// </summary>
		public override Type OriginatorType { get { return typeof(DmcCAdapter); } }

		/// <summary>
		/// Loads the settings from XML.
		/// </summary>
		/// <param name="xml"></param>
		/// <returns></returns>
		[PublicAPI, XmlDeviceSettingsFactoryMethod(FACTORY_NAME)]
		public static DmcCAdapterSettings FromXml(string xml)
		{
			DmcCAdapterSettings output = new DmcCAdapterSettings();
			ParseXml(output, xml);
			return output;
		}
	}
}