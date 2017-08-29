using ICD.Common.Utils;
using ICD.Common.Utils.Xml;
using ICD.Connect.Devices;
using ICD.Connect.Settings.Attributes;

namespace ICD.Connect.Routing.CrestronPro.Receivers.DmRmcScalerCBase
{
	/// <summary>
	/// Settings for the DmRmcScalerCAdapter.
	/// </summary>
	public abstract class AbstractDmRmcScalerCAdapterSettings : AbstractDeviceSettings, IDmRmcScalerCAdapterSettings
	{
		private const string IPID_ELEMENT = "IPID";
		private const string DM_SWITCH_ELEMENT = "DmSwitch";
		private const string DM_OUTPUT_ELEMENT = "DmOutput";

		[SettingsProperty(SettingsProperty.ePropertyType.Ipid)]
		public byte? Ipid { get; set; }

		[SettingsProperty(SettingsProperty.ePropertyType.DeviceId)]
		public int? DmSwitch { get; set; }

		public int? DmOutputAddress { get; set; }

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

			if (DmOutputAddress != null)
				writer.WriteElementString(DM_OUTPUT_ELEMENT, IcdXmlConvert.ToString((int)DmOutputAddress));
		}

		/// <summary>
		/// Loads the settings from XML.
		/// </summary>
		/// <param name="instance"></param>
		/// <param name="xml"></param>
		/// <returns></returns>
		public static void ParseXml(AbstractDmRmcScalerCAdapterSettings instance, string xml)
		{
			instance.Ipid = XmlUtils.TryReadChildElementContentAsByte(xml, IPID_ELEMENT);
			instance.DmSwitch = XmlUtils.TryReadChildElementContentAsInt(xml, DM_SWITCH_ELEMENT);
			instance.DmOutputAddress = XmlUtils.TryReadChildElementContentAsInt(xml, DM_OUTPUT_ELEMENT);

			ParseXml((AbstractDeviceSettings)instance, xml);
		}
	}
}
