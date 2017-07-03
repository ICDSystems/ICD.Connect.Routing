using System;
using ICD.Common.Attributes.Properties;
using ICD.Common.Properties;
using ICD.Common.Utils;
using ICD.Common.Utils.Xml;
using ICD.Connect.Settings;
using ICD.Connect.Settings.Attributes.Factories;

namespace ICD.Connect.Routing.CrestronPro.DigitalMedia.HdMd4X24kE
{
	public sealed class HdMd4X24kEAdapterSettings : AbstractDeviceSettings
	{
		private const string FACTORY_NAME = "HdMd4X24kE";

		private const string IPID_ELEMENT = "IPID";
		private const string ADDRESS_ELEMENT = "Address";

		[SettingsProperty(SettingsProperty.ePropertyType.Ipid)]
		public byte Ipid { get; set; }

		public string Address { get; set; }

		/// <summary>
		/// Gets the originator factory name.
		/// </summary>
		public override string FactoryName { get { return FACTORY_NAME; } }

		/// <summary>
		/// Gets the type of the originator for this settings instance.
		/// </summary>
		public override Type OriginatorType { get { return typeof(HdMd4X24kEAdapter); } }

		/// <summary>
		/// Writes property elements to xml.
		/// </summary>
		/// <param name="writer"></param>
		protected override void WriteElements(IcdXmlTextWriter writer)
		{
			base.WriteElements(writer);

			writer.WriteElementString(ADDRESS_ELEMENT, Address);
			writer.WriteElementString(IPID_ELEMENT, StringUtils.ToIpIdString(Ipid));
		}

		/// <summary>
		/// Loads the settings from XML.
		/// </summary>
		/// <param name="xml"></param>
		/// <returns></returns>
		[PublicAPI, XmlDeviceSettingsFactoryMethod(FACTORY_NAME)]
		public static HdMd4X24kEAdapterSettings FromXml(string xml)
		{
			byte ipid = XmlUtils.ReadChildElementContentAsByte(xml, IPID_ELEMENT);
			string address = XmlUtils.ReadChildElementContentAsString(xml, ADDRESS_ELEMENT);

			HdMd4X24kEAdapterSettings output = new HdMd4X24kEAdapterSettings
			{
				Ipid = ipid,
				Address = address
			};

			ParseXml(output, xml);
			return output;
		}
	}
}
