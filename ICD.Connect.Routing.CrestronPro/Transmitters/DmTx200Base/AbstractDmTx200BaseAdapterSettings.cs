using ICD.Common.Attributes.Properties;
using ICD.Common.Utils;
using ICD.Common.Utils.Xml;
using ICD.Connect.Settings;

namespace ICD.Connect.Routing.CrestronPro.Transmitters.DmTx200Base
{
	public abstract class AbstractDmTx200BaseAdapterSettings : AbstractDeviceSettings
	{
		private const string IPID_ELEMENT = "IPID";
		private const string DM_SWITCH_ELEMENT = "DmSwitch";
		private const string DM_INPUT_ELEMENT = "DmInput";

		[SettingsProperty(SettingsProperty.ePropertyType.Ipid)]
		public byte? Ipid { get; set; }

		[SettingsProperty(SettingsProperty.ePropertyType.DeviceId)]
		public int? DmSwitch { get; set; }

		public int? DmInputAddress { get; set; }

		/// <summary>
		/// Writes property elements to xml.
		/// </summary>
		/// <param name="writer"></param>
		protected override void WriteElements(IcdXmlTextWriter writer)
		{
			base.WriteElements(writer);

			if (Ipid != null)
				writer.WriteElementString(IPID_ELEMENT, StringUtils.ToIpIdString((byte)Ipid));

			if (DmSwitch != null)
				writer.WriteElementString(DM_SWITCH_ELEMENT, IcdXmlConvert.ToString((int)DmSwitch));

			if (DmInputAddress != null)
				writer.WriteElementString(DM_INPUT_ELEMENT, IcdXmlConvert.ToString((int)DmInputAddress));
		}

		/// <summary>
		/// Parses the xml and applies the properties to the instance.
		/// </summary>
		/// <param name="instance"></param>
		/// <param name="xml"></param>
		protected static void ParseXml(AbstractDmTx200BaseAdapterSettings instance, string xml)
		{
			instance.Ipid = XmlUtils.TryReadChildElementContentAsByte(xml, IPID_ELEMENT);
			instance.DmSwitch = XmlUtils.TryReadChildElementContentAsInt(xml, DM_SWITCH_ELEMENT);
			instance.DmInputAddress = XmlUtils.TryReadChildElementContentAsInt(xml, DM_INPUT_ELEMENT);

			AbstractDeviceSettings.ParseXml(instance, xml);
		}
	}
}
