using ICD.Common.Utils;
using ICD.Common.Utils.Xml;
using ICD.Connect.Devices;
using ICD.Connect.Routing.CrestronPro.DigitalMedia.DmXio.DmXioDirectorBase;
using ICD.Connect.Settings.Attributes.SettingsProperties;

namespace ICD.Connect.Routing.CrestronPro.DigitalMedia.DmNvx.Dm100xStrBase
{
	public abstract class AbstractDm100XStrBaseAdapterSettings : AbstractDeviceSettings, IDm100XStrBaseAdapterSettings
	{
		private const string ETHERNET_ID_ELEMENT = "EthernetId";
		private const string ENDPOINT_ID_ELEMENT = "EndpointId";
		private const string DIRECTOR_ID_ELEMENT = "DirectorId";
		private const string DOMAIN_ID_ELEMENT = "DomainId";

		[CrestronByteSettingsProperty]
		public byte? EthernetId { get; set; }

		public uint? EndpointId { get; set; }

		[OriginatorIdSettingsProperty(typeof(IDmXioDirectorBaseAdapter))]
		public int? DirectorId { get; set; }

		public uint? DomainId { get; set; }

		/// <summary>
		/// Returns true if the settings have been configured for receive.
		/// </summary>
		public abstract bool IsReceiver { get; }

		/// <summary>
		/// Writes property elements to xml.
		/// </summary>
		/// <param name="writer"></param>
		protected override void WriteElements(IcdXmlTextWriter writer)
		{
			base.WriteElements(writer);

			writer.WriteElementString(ETHERNET_ID_ELEMENT, EthernetId == null ? null : StringUtils.ToIpIdString((byte)EthernetId));
			writer.WriteElementString(ENDPOINT_ID_ELEMENT, IcdXmlConvert.ToString(EndpointId));
			writer.WriteElementString(DIRECTOR_ID_ELEMENT, IcdXmlConvert.ToString(DirectorId));
			writer.WriteElementString(DOMAIN_ID_ELEMENT, IcdXmlConvert.ToString(DomainId));
		}

		/// <summary>
		/// Updates the settings from xml.
		/// </summary>
		/// <param name="xml"></param>
		public override void ParseXml(string xml)
		{
			base.ParseXml(xml);

			EthernetId = XmlUtils.TryReadChildElementContentAsByte(xml, ETHERNET_ID_ELEMENT);
			EndpointId = XmlUtils.TryReadChildElementContentAsUInt(xml, ENDPOINT_ID_ELEMENT);
			DirectorId = XmlUtils.TryReadChildElementContentAsInt(xml, DIRECTOR_ID_ELEMENT);
			DomainId = XmlUtils.TryReadChildElementContentAsUInt(xml, DOMAIN_ID_ELEMENT);
		}
	}
}
