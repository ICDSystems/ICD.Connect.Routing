using System;
using ICD.Common.Properties;
using ICD.Connect.Settings.Attributes;

namespace ICD.Connect.Routing.CrestronPro.Cards.Inputs.Dmc4kC
{
    public sealed class Dmc4kCAdapterSettings : AbstractCardSettings
    {
        private const string FACTORY_NAME = "Dmc4kC";

        /// <summary>
        /// Gets the originator factory name.
        /// </summary>
        public override string FactoryName { get { return FACTORY_NAME; } }

        /// <summary>
        /// Gets the type of the originator for this settings instance.
        /// </summary>
        public override Type OriginatorType { get { return typeof(Dmc4kCAdapter); } }

        /// <summary>
        /// Loads the settings from XML.
        /// </summary>
        /// <param name="xml"></param>
        /// <returns></returns>
        [PublicAPI, XmlFactoryMethod(FACTORY_NAME)]
        public static Dmc4kCAdapterSettings FromXml(string xml)
        {
            Dmc4kCAdapterSettings output = new Dmc4kCAdapterSettings();
            ParseXml(output, xml);
            return output;
        }

        /// <summary>
        /// Loads the settings from XML.
        /// </summary>
        /// <param name="xml"></param>
        /// <returns></returns>
        [PublicAPI, XmlFactoryMethod(FACTORY_NAME + "Hdcp2")]
        public static Dmc4kCAdapterSettings FromXmlHdcp2(string xml)
        {
            return FromXml(xml);
        }
    }
}