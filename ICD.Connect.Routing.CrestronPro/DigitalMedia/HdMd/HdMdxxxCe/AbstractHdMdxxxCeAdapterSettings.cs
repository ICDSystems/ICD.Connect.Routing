using ICD.Common.Utils.Xml;
using ICD.Connect.Settings.Attributes.SettingsProperties;

namespace ICD.Connect.Routing.CrestronPro.DigitalMedia.HdMd.HdMdxxxCe
{
	public abstract class AbstractHdMdxxxCeAdapterSettings : AbstractCrestronSwitchAdapterSettings, IHdMdxxxCeAdapterSettings
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
