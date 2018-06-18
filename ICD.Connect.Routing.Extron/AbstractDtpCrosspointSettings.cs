using ICD.Common.Utils.Xml;
using ICD.Connect.Devices;
using ICD.Connect.Protocol.Ports;
using ICD.Connect.Settings.Attributes.SettingsProperties;

namespace ICD.Connect.Routing.Extron
{
	public class AbstractDtpCrosspointSettings : AbstractDeviceSettings
	{
		private const string ELEMENT_PORT = "Port";
		private const string ELEMENT_USERNAME = "Username";
		private const string ELEMENT_PASSWORD = "Password";

		/// <summary>
		/// The port id.
		/// </summary>
		[OriginatorIdSettingsProperty(typeof(ISerialPort))]
		public int? Port { get; set; }

		public string Password { get; set; }

		/// <summary>
		/// Writes property elements to xml.
		/// </summary>
		/// <param name="writer"></param>
		protected override void WriteElements(IcdXmlTextWriter writer)
		{
			base.WriteElements(writer);

			writer.WriteElementString(ELEMENT_PORT, IcdXmlConvert.ToString(Port));
			writer.WriteElementString(ELEMENT_PASSWORD, Password);
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
		}
	}
}