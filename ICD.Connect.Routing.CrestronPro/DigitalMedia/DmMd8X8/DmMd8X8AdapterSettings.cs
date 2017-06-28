using System;
using ICD.Common.Properties;
using ICD.Connect.Routing.CrestronPro.DigitalMedia.DmMdNXN;
using ICD.Connect.Settings.Attributes.Factories;

namespace ICD.Connect.Routing.CrestronPro.DigitalMedia.DmMd8X8
{
	public sealed class DmMd8X8AdapterSettings : AbstractDmMdMNXNAdapterSettings
	{
		private const string FACTORY_NAME = "DmMd8x8";

		/// <summary>
		/// Gets the originator factory name.
		/// </summary>
		public override string FactoryName { get { return FACTORY_NAME; } }

		/// <summary>
		/// Gets the type of the originator for this settings instance.
		/// </summary>
		public override Type OriginatorType { get { return typeof(DmMd8X8Adapter); } }

		/// <summary>
		/// Loads the settings from XML.
		/// </summary>
		/// <param name="xml"></param>
		/// <returns></returns>
		[PublicAPI, XmlDeviceSettingsFactoryMethod(FACTORY_NAME)]
		public static DmMd8X8AdapterSettings FromXml(string xml)
		{
			DmMd8X8AdapterSettings output = new DmMd8X8AdapterSettings();
			ParseXml(output, xml);
			return output;
		}
	}
}
