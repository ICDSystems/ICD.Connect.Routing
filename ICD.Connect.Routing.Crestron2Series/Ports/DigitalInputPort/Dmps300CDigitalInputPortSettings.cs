using ICD.Common.Utils.Xml;
using ICD.Connect.Protocol.Ports.DigitalInput;
using ICD.Connect.Routing.Crestron2Series.Devices;
using ICD.Connect.Settings.Attributes;
using ICD.Connect.Settings.Attributes.SettingsProperties;

namespace ICD.Connect.Routing.Crestron2Series.Ports.DigitalInputPort
{
	[KrangSettings("Dmps300CDigitalInputPort", typeof(Dmps300CDigitalInputPort))]
	public sealed class Dmps300CDigitalInputPortSettings : AbstractDigitalInputPortSettings
	{
		private const string DEVICE_ELEMENT = "Device";
		private const string ADDRESS_ELEMENT = "Address";

		[OriginatorIdSettingsProperty(typeof(IDmps300CDigitalInputPortDevice))]
		public int Device { get; set; }

		public int Address { get; set; }

		/// <summary>
		/// Writes property elements to xml.
		/// </summary>
		/// <param name="writer"></param>
		protected override void WriteElements(IcdXmlTextWriter writer)
		{
			base.WriteElements(writer);

			writer.WriteElementString(DEVICE_ELEMENT, IcdXmlConvert.ToString(Device));
			writer.WriteElementString(ADDRESS_ELEMENT, IcdXmlConvert.ToString(Address));
		}

		/// <summary>
		/// Updates the settings from xml.
		/// </summary>
		/// <param name="xml"></param>
		public override void ParseXml(string xml)
		{
			base.ParseXml(xml);

			Device = XmlUtils.TryReadChildElementContentAsInt(xml, DEVICE_ELEMENT) ?? 0;
			Address = XmlUtils.TryReadChildElementContentAsInt(xml, ADDRESS_ELEMENT) ?? 0;
		}
	}
}