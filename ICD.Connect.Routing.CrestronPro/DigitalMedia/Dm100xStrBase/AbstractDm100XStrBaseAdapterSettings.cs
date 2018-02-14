using ICD.Common.Utils;
using ICD.Common.Utils.Xml;
using ICD.Connect.Devices;
using ICD.Connect.Settings.Attributes.SettingsProperties;

namespace ICD.Connect.Routing.CrestronPro.DigitalMedia.Dm100xStrBase
{
	public abstract class AbstractDm100XStrBaseAdapterSettings : AbstractDeviceSettings, IDm100XStrBaseAdapterSettings
	{
		private const string ETHERNET_ID_ELEMENT = "EthernetId";

		[IpIdSettingsProperty]
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

		/// <summary>
		/// Updates the settings from xml.
		/// </summary>
		/// <param name="xml"></param>
		public override void ParseXml(string xml)
		{
			base.ParseXml(xml);

			EthernetId = XmlUtils.TryReadChildElementContentAsByte(xml, ETHERNET_ID_ELEMENT) ?? 0;
		}
	}
}