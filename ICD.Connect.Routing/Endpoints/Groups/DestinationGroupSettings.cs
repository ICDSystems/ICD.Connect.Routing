using System;
using ICD.Common.Properties;
using ICD.Connect.Settings.Attributes;

namespace ICD.Connect.Routing.Endpoints.Groups
{
	public sealed class DestinationGroupSettings : AbstractDestinationGroupSettings
	{
		private const string FACTORY_NAME = "DestinationGroup";

		/// <summary>
		/// Gets the originator factory name.
		/// </summary>
		public override string FactoryName { get { return FACTORY_NAME; } }

		/// <summary>
		/// Gets the type of the originator for this settings instance.
		/// </summary>
		public override Type OriginatorType { get { return typeof(DestinationGroup); } }

		/// <summary>
		/// Loads the settings from XML.
		/// </summary>
		/// <param name="xml"></param>
		/// <returns></returns>
		[PublicAPI, XmlFactoryMethod(FACTORY_NAME)]
		public static DestinationGroupSettings FromXml(string xml)
		{
			DestinationGroupSettings output = new DestinationGroupSettings();
			output.ParseXml(xml);
			return output;
		}
	}
}
