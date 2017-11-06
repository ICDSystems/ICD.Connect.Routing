using System;
using ICD.Common.Properties;
using ICD.Common.Utils.Xml;
using ICD.Connect.Devices;
using ICD.Connect.Settings.Attributes;

namespace ICD.Connect.Routing.Crestron2Series.ControlSystem
{
    public sealed class Crestron2SeriesControlSystemSettings : AbstractDeviceSettings
    {
        private const string FACTORY_NAME = "Crestron2SeriesControlSystem";

	    private const string ADDRESS_ELEMENT = "Address";

	    public string Address { get; set; }

        /// <summary>
        /// Gets the originator factory name.
        /// </summary>
        public override string FactoryName { get { return FACTORY_NAME; } }

        /// <summary>
        /// Gets the type of the originator for this settings instance.
        /// </summary>
        public override Type OriginatorType { get { return typeof(Crestron2SeriesControlSystem); } }

	    /// <summary>
	    /// Writes property elements to xml.
	    /// </summary>
	    /// <param name="writer"></param>
	    protected override void WriteElements(IcdXmlTextWriter writer)
	    {
		    base.WriteElements(writer);

			writer.WriteElementString(ADDRESS_ELEMENT, Address);
	    }

		/// <summary>
		/// Loads the settings from XML.
		/// </summary>
		/// <param name="xml"></param>
		/// <returns></returns>
		[PublicAPI, XmlFactoryMethod(FACTORY_NAME)]
		public static Crestron2SeriesControlSystemSettings FromXml(string xml)
		{
			Crestron2SeriesControlSystemSettings output = new Crestron2SeriesControlSystemSettings
			{
				Address = XmlUtils.TryReadChildElementContentAsString(xml, ADDRESS_ELEMENT)
			};

			ParseXml(output, xml);
			return output;
		}
    }
}