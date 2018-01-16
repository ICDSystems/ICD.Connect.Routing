using System;
using ICD.Common.Properties;
using ICD.Connect.Settings.Attributes;

namespace ICD.Connect.Routing.CrestronPro.Cards.Outputs.Dmc4kCoHd
{
	public sealed class Dmc4kCoHdAdapterSettings : AbstractOutputCardSettings
	{
		private const string FACTORY_NAME = "Dmc4kCoHd";

		/// <summary>
		/// Gets the originator factory name.
		/// </summary>
		public override string FactoryName { get { return FACTORY_NAME; } }

		/// <summary>
		/// Gets the type of the originator for this settings instance.
		/// </summary>
		public override Type OriginatorType { get { return typeof(Dmc4kCoHdAdapter); } }

		/// <summary>
		/// Loads the settings from XML.
		/// </summary>
		/// <param name="xml"></param>
		/// <returns></returns>
		[PublicAPI, XmlFactoryMethod(FACTORY_NAME)]
		public static Dmc4kCoHdAdapterSettings FromXml(string xml)
		{
			Dmc4kCoHdAdapterSettings output = new Dmc4kCoHdAdapterSettings();
			ParseXml(output, xml);
			return output;
		}
	}
}
