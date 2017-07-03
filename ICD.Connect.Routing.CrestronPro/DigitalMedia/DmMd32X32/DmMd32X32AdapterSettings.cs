using System;
using ICD.Common.Properties;
using ICD.Connect.Routing.CrestronPro.DigitalMedia.DmMdNXN;
using ICD.Connect.Settings.Attributes.Factories;

namespace ICD.Connect.Routing.CrestronPro.DigitalMedia.DmMd32X32
{
	public sealed class DmMd32X32AdapterSettings : AbstractDmMdMNXNAdapterSettings
	{
		private const string FACTORY_NAME = "DmMd32x32";

		/// <summary>
		/// Gets the originator factory name.
		/// </summary>
		public override string FactoryName { get { return FACTORY_NAME; } }

		/// <summary>
		/// Gets the type of the originator for this settings instance.
		/// </summary>
		public override Type OriginatorType { get { return typeof(DmMd32X32Adapter); } }

		/// <summary>
		/// Loads the settings from XML.
		/// </summary>
		/// <param name="xml"></param>
		/// <returns></returns>
		[PublicAPI, XmlDeviceSettingsFactoryMethod(FACTORY_NAME)]
		public static DmMd32X32AdapterSettings FromXml(string xml)
		{
			DmMd32X32AdapterSettings output = new DmMd32X32AdapterSettings();
			ParseXml(output, xml);
			return output;
		}
	}
}