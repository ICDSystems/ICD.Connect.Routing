using System;
using ICD.Common.Properties;
using ICD.Connect.Settings.Attributes;

namespace ICD.Connect.Routing.Endpoints.Sources
{
	public sealed class SourceSettings : AbstractSourceSettings
	{
		private const string FACTORY_NAME = "Source";

		/// <summary>
		/// Gets the originator factory name.
		/// </summary>
		public override string FactoryName { get { return FACTORY_NAME; } }

		/// <summary>
		/// Gets the type of the originator for this settings instance.
		/// </summary>
		public override Type OriginatorType { get { return typeof(Source); } }

		/// <summary>
		/// Loads the settings from XML.
		/// </summary>
		/// <param name="xml"></param>
		/// <returns></returns>
		[PublicAPI, XmlFactoryMethod(FACTORY_NAME)]
		public static SourceSettings FromXml(string xml)
		{
			SourceSettings output = new SourceSettings();
			ParseXml(output, xml);
			return output;
		}
	}
}
