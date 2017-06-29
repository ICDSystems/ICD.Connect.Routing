using System;
using ICD.Common.Properties;
using ICD.Connect.Settings;
using ICD.Connect.Settings.Attributes.Factories;

namespace ICD.Connect.Routing.SPlus
{
	public sealed class SPlusSwitcherSettings : AbstractDeviceSettings
	{
		private const string FACTORY_NAME = "SPlusSwitcher";

		public override string FactoryName { get { return FACTORY_NAME; } }

		/// <summary>
		/// Gets the type of the originator for this settings instance.
		/// </summary>
		public override Type OriginatorType { get { return typeof(SPlusSwitcher); } }

		/// <summary>
		/// Loads the settings from XML.
		/// </summary>
		/// <param name="xml"></param>
		/// <returns></returns>
		[PublicAPI, XmlDeviceSettingsFactoryMethod(FACTORY_NAME)]
		public static SPlusSwitcherSettings FromXml(string xml)
		{
			SPlusSwitcherSettings output = new SPlusSwitcherSettings();

			ParseXml(output, xml);
			return output;
		}
	}
}
