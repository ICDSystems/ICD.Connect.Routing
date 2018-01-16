using ICD.Common.Utils;
using ICD.Common.Utils.Xml;
using ICD.Connect.Devices;
using ICD.Connect.Settings.Attributes.SettingsProperties;

namespace ICD.Connect.Routing.CrestronPro.Transmitters
{
	public abstract class AbstractEndpointTransmitterBaseAdapterSettings : AbstractDeviceSettings,
	                                                                       IEndpointTransmitterBaseAdapterSettings
	{
		private const string IPID_ELEMENT = "IPID";
		private const string DM_SWITCH_ELEMENT = "DmSwitch";
		private const string DM_INPUT_ELEMENT = "DmInput";

		[IpIdSettingsProperty]
		public byte? Ipid { get; set; }

		[OriginatorIdSettingsProperty(typeof(IDmParent))]
		public int? DmSwitch { get; set; }

		public int? DmInputAddress { get; set; }

		/// <summary>
		/// Writes property elements to xml.
		/// </summary>
		/// <param name="writer"></param>
		protected override void WriteElements(IcdXmlTextWriter writer)
		{
			base.WriteElements(writer);

			writer.WriteElementString(IPID_ELEMENT, Ipid == null ? null : StringUtils.ToIpIdString((byte)Ipid));
			writer.WriteElementString(DM_SWITCH_ELEMENT, IcdXmlConvert.ToString(DmSwitch));
			writer.WriteElementString(DM_INPUT_ELEMENT, IcdXmlConvert.ToString(DmInputAddress));
		}

		/// <summary>
		/// Parses the xml and applies the properties to the instance.
		/// </summary>
		/// <param name="instance"></param>
		/// <param name="xml"></param>
		protected static void ParseXml(AbstractEndpointTransmitterBaseAdapterSettings instance, string xml)
		{
			instance.Ipid = XmlUtils.TryReadChildElementContentAsByte(xml, IPID_ELEMENT);
			instance.DmSwitch = XmlUtils.TryReadChildElementContentAsInt(xml, DM_SWITCH_ELEMENT);
			instance.DmInputAddress = XmlUtils.TryReadChildElementContentAsInt(xml, DM_INPUT_ELEMENT);

			AbstractDeviceSettings.ParseXml(instance, xml);
		}
	}
}
