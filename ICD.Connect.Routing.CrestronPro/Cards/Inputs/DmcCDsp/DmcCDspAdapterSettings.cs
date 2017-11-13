using System;
using ICD.Common.Properties;
using ICD.Connect.Settings.Attributes;

namespace ICD.Connect.Routing.CrestronPro.Cards.Inputs.DmcCDsp
{
	public sealed class DmcCDspAdapterSettings : AbstractCardSettings
	{
		private const string FACTORY_NAME = "DmcCDsp";

		/// <summary>
		/// Gets the originator factory name.
		/// </summary>
		public override string FactoryName { get { return FACTORY_NAME; } }

		/// <summary>
		/// Gets the type of the originator for this settings instance.
		/// </summary>
		public override Type OriginatorType { get { return typeof(DmcCDspAdapter); } }

		/// <summary>
		/// Loads the settings from XML.
		/// </summary>
		/// <param name="xml"></param>
		/// <returns></returns>
		[PublicAPI, XmlFactoryMethod(FACTORY_NAME)]
		public static DmcCDspAdapterSettings FromXml(string xml)
		{
			DmcCDspAdapterSettings output = new DmcCDspAdapterSettings();
			ParseXml(output, xml);
			return output;
		}
	}
}