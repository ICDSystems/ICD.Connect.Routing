using ICD.Common.Utils.Xml;
using ICD.Connect.Devices;
using ICD.Connect.Protocol.Ports;
using ICD.Connect.Settings.Attributes.SettingsProperties;

namespace ICD.Connect.Routing.Extron.Devices.Switchers
{
	public abstract class AbstractDtpCrosspointSettings : AbstractDeviceSettings, IDtpCrosspointSettings
	{
		private const string ELEMENT_PORT = "Port";
		private const string ELEMENT_PASSWORD = "Password";
	    private const string ADDRESS_ELEMENT = "Address";

		/// <summary>
		/// The port id.
		/// </summary>
		[OriginatorIdSettingsProperty(typeof(ISerialPort))]
		public int? Port { get; set; }

		public string Password { get; set; }

        public string Address { get; set; }

		/// <summary>
		/// Writes property elements to xml.
		/// </summary>
		/// <param name="writer"></param>
		protected override void WriteElements(IcdXmlTextWriter writer)
		{
			base.WriteElements(writer);

			writer.WriteElementString(ELEMENT_PORT, IcdXmlConvert.ToString(Port));
			writer.WriteElementString(ELEMENT_PASSWORD, Password);
            writer.WriteElementString(ADDRESS_ELEMENT, Address);
		}

		/// <summary>
		/// Updates the settings from xml.
		/// </summary>
		/// <param name="xml"></param>
		public override void ParseXml(string xml)
		{
			base.ParseXml(xml);

			Port = XmlUtils.TryReadChildElementContentAsInt(xml, ELEMENT_PORT);
			Password = XmlUtils.TryReadChildElementContentAsString(xml, ELEMENT_PASSWORD);
		    Address = XmlUtils.TryReadChildElementContentAsString(xml, ADDRESS_ELEMENT);
		}
	}
}