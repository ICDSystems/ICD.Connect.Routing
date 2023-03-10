using ICD.Common.Utils.Xml;
using ICD.Connect.Protocol.Ports.ComPort;
using ICD.Connect.Routing.Crestron2Series.Devices;
using ICD.Connect.Settings.Attributes;
using ICD.Connect.Settings.Attributes.SettingsProperties;

namespace ICD.Connect.Routing.Crestron2Series.Ports.ComPort
{
	[KrangSettings("Dmps300CComPort", typeof(Dmps300CComPort))]
	public sealed class Dmps300CComPortSettings : AbstractComPortSettings
	{
		private const string DEVICE_ELEMENT = "Device";
		private const string ADDRESS_ELEMENT = "Address";
		
		[ControlPortParentSettingsProperty]
		[OriginatorIdSettingsProperty(typeof(IDmps300CComPortDevice))]
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