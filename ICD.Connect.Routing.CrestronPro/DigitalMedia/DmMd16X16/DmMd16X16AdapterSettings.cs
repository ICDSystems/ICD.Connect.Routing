using System;
using ICD.Common.Properties;
using ICD.Connect.Routing.CrestronPro.DigitalMedia.DmMdNXN;
using ICD.Connect.Settings.Attributes.Factories;

namespace ICD.Connect.Routing.CrestronPro.DigitalMedia.DmMd16X16
{
	public sealed class DmMd16X16AdapterSettings : AbstractDmMdMNXNAdapterSettings
	{
		private const string FACTORY_NAME = "DmMd16x16";

		/// <summary>
		/// Gets the originator factory name.
		/// </summary>
		public override string FactoryName { get { return FACTORY_NAME; } }

		/// <summary>
		/// Gets the type of the originator for this settings instance.
		/// </summary>
		public override Type OriginatorType { get { return typeof(DmMd16X16Adapter); } }

		/// <summary>
		/// Loads the settings from XML.
		/// </summary>
		/// <param name="xml"></param>
		/// <returns></returns>
		[PublicAPI, XmlDeviceSettingsFactoryMethod(FACTORY_NAME)]
		public static DmMd16X16AdapterSettings FromXml(string xml)
		{
			DmMd16X16AdapterSettings output = new DmMd16X16AdapterSettings();
			ParseXml(output, xml);
			return output;
		}
	}
}