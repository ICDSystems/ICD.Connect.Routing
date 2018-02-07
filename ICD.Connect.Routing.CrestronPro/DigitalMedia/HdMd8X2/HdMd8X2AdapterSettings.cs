using System;
using ICD.Common.Properties;
using ICD.Connect.Settings.Attributes;

namespace ICD.Connect.Routing.CrestronPro.DigitalMedia.HdMd8X2
{
	public sealed class HdMd8X2AdapterSettings : AbstractDmSwitcherAdapterSettings
	{
		private const string FACTORY_NAME = "HdMd8X2";

		/// <summary>
		/// Gets the originator factory name.
		/// </summary>
		public override string FactoryName { get { return FACTORY_NAME; } }

		/// <summary>
		/// Gets the type of the originator for this settings instance.
		/// </summary>
		public override Type OriginatorType { get { return typeof(HdMd8X2Adapter); } }

		/// <summary>
		/// Loads the settings from XML.
		/// </summary>
		/// <param name="xml"></param>
		/// <returns></returns>
		[PublicAPI, XmlFactoryMethod(FACTORY_NAME)]
		public static HdMd8X2AdapterSettings FromXml(string xml)
		{
			HdMd8X2AdapterSettings output = new HdMd8X2AdapterSettings();
			output.ParseXml(xml);
			return output;
		}
	}
}
