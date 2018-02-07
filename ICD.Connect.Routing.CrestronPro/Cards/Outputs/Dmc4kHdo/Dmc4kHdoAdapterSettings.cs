using System;
using ICD.Common.Properties;
using ICD.Connect.Settings.Attributes;

namespace ICD.Connect.Routing.CrestronPro.Cards.Outputs.Dmc4kHdo
{
	public sealed class Dmc4kHdoAdapterSettings : AbstractOutputCardSettings
	{
		private const string FACTORY_NAME = "Dmc4kHdo";

		/// <summary>
		/// Gets the originator factory name.
		/// </summary>
		public override string FactoryName { get { return FACTORY_NAME; } }

		/// <summary>
		/// Gets the type of the originator for this settings instance.
		/// </summary>
		public override Type OriginatorType { get { return typeof(Dmc4kHdoAdapter); } }

		/// <summary>
		/// Loads the settings from XML.
		/// </summary>
		/// <param name="xml"></param>
		/// <returns></returns>
		[PublicAPI, XmlFactoryMethod(FACTORY_NAME)]
		public static Dmc4kHdoAdapterSettings FromXml(string xml)
		{
			Dmc4kHdoAdapterSettings output = new Dmc4kHdoAdapterSettings();
			output.ParseXml(xml);
			return output;
		}
	}
}
