using ICD.Common.Utils.Xml;
using ICD.Connect.Settings.Attributes;
using ICD.Connect.Settings.Attributes.SettingsProperties;

namespace ICD.Connect.Routing.Crestron2Series.Devices.ControlSystem
{
	[KrangSettings("Dmps300CControlSystem", typeof(Dmps300CControlSystem))]
    public sealed class Dmps300CControlSystemSettings : AbstractDmps300CDeviceSettings
    {
	    private const string ADDRESS_ELEMENT = "Address";

        [IpAddressSettingsProperty]
	    public string Address { get; set; }

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