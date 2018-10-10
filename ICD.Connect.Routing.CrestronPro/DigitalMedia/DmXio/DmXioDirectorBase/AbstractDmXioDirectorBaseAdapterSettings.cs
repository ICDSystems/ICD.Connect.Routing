using ICD.Common.Utils.Xml;
using ICD.Connect.Devices;
using ICD.Connect.Settings.Attributes.SettingsProperties;

namespace ICD.Connect.Routing.CrestronPro.DigitalMedia.DmXioDirectorBase
{
	public abstract class AbstractDmXioDirectorBaseAdapterSettings : AbstractDeviceSettings,
	                                                                 IDmXioDirectorBaseAdapterSettings
	{
		private const string ETHERNET_ID_ELEMENT = "EthernetId";

		[CrestronByteSettingsProperty]
		public uint? EthernetId { get; set; }

		/// <summary>
		/// Writes property elements to xml.
		/// </summary>
		/// <param name="writer"></param>
		protected override void WriteElements(IcdXmlTextWriter writer)
		{
			base.WriteElements(writer);

			writer.WriteElementString(ETHERNET_ID_ELEMENT, IcdXmlConvert.ToString(EthernetId));
		}

		/// <summary>
		/// Updates the settings from xml.
		/// </summary>
		/// <param name="xml"></param>
		public override void ParseXml(string xml)
		{
			base.ParseXml(xml);

			EthernetId = XmlUtils.TryReadChildElementContentAsUInt(xml, ETHERNET_ID_ELEMENT);
		}
	}
}
