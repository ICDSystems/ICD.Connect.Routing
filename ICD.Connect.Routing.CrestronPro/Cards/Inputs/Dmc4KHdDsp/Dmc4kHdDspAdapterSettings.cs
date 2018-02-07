using System;
using ICD.Common.Properties;
using ICD.Connect.Settings.Attributes;

namespace ICD.Connect.Routing.CrestronPro.Cards.Inputs.Dmc4KHdDsp
{
	public sealed class Dmc4kHdDspAdapterSettings : AbstractInputCardSettings
	{
		private const string FACTORY_NAME = "Dmc4kHdDsp";

		/// <summary>
		/// Gets the originator factory name.
		/// </summary>
		public override string FactoryName { get { return FACTORY_NAME; } }

		/// <summary>
		/// Gets the type of the originator for this settings instance.
		/// </summary>
		public override Type OriginatorType { get { return typeof(Dmc4kHdDspAdapter); } }

		/// <summary>
		/// Loads the settings from XML.
		/// </summary>
		/// <param name="xml"></param>
		/// <returns></returns>
		[PublicAPI, XmlFactoryMethod(FACTORY_NAME)]
		public static Dmc4kHdDspAdapterSettings FromXml(string xml)
		{
			Dmc4kHdDspAdapterSettings output = new Dmc4kHdDspAdapterSettings();
			output.ParseXml(xml);
			return output;
		}
	}
}
