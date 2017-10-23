using ICD.Common.Utils;
using ICD.Common.Utils.Xml;
using ICD.Connect.Devices;
using ICD.Connect.Settings.Attributes;

namespace ICD.Connect.Routing.CrestronPro.DigitalMedia.Dm100xStrBase
{
	public abstract class AbstractDm100XStrBaseAdapterSettings : AbstractDeviceSettings, IDm100XStrBaseAdapterSettings
	{
		private const string ETHERNET_ID_ELEMENT = "EthernetId";

		[SettingsProperty(SettingsProperty.ePropertyType.Ipid)]
		public byte EthernetId { get; set; }

		/// <summary>
		/// Writes property elements to xml.
		/// </summary>
		/// <param name="writer"></param>
		protected override void WriteElements(IcdXmlTextWriter writer)
		{
			base.WriteElements(writer);

			writer.WriteElementString(ETHERNET_ID_ELEMENT, StringUtils.ToIpIdString(EthernetId));
		}

		protected static void ParseXml(AbstractDm100XStrBaseAdapterSettings instance, string xml)
		{
			instance.EthernetId = XmlUtils.TryReadChildElementContentAsByte(xml, ETHERNET_ID_ELEMENT) ?? 0;

			AbstractDeviceSettings.ParseXml(instance, xml);
		}
	}
}