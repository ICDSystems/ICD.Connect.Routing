using System;
using ICD.Common.Utils.Xml;
using ICD.Connect.Settings.Attributes;

namespace ICD.Connect.Routing.Crestron2Series.Devices.ControlSystem
{
	[KrangSettings(FACTORY_NAME)]
    public sealed class Dmps300CControlSystemSettings : AbstractDmps300CDeviceSettings
    {
        private const string FACTORY_NAME = "Dmps300CControlSystem";

	    private const string ADDRESS_ELEMENT = "Address";

	    public string Address { get; set; }

        /// <summary>
        /// Gets the originator factory name.
        /// </summary>
        public override string FactoryName { get { return FACTORY_NAME; } }

        /// <summary>
        /// Gets the type of the originator for this settings instance.
        /// </summary>
        public override Type OriginatorType { get { return typeof(Dmps300CControlSystem); } }

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
	    /// Updates the settings from xml.
	    /// </summary>
	    /// <param name="xml"></param>
	    public override void ParseXml(string xml)
	    {
		    base.ParseXml(xml);

		    Address = XmlUtils.TryReadChildElementContentAsString(xml, ADDRESS_ELEMENT);
	    }
    }
}