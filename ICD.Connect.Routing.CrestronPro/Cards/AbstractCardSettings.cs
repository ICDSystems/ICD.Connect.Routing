using ICD.Common.Utils;
using ICD.Common.Utils.Xml;
using ICD.Connect.Devices;
using ICD.Connect.Settings.Attributes;

namespace ICD.Connect.Routing.CrestronPro.Cards
{
	public abstract class AbstractCardSettings : AbstractDeviceSettings, ICardSettings
	{
		private const string CRESNET_ID_ELEMENT = "CresnetId";
		private const string CARD_NUMBER_ELEMENT = "CardNumber";
		private const string SWITCHER_ID_ELEMENT = "SwitcherId";

		[SettingsProperty(SettingsProperty.ePropertyType.Ipid)]
		public byte? CresnetId { get; set; }

		public int? CardNumber { get; set; }

		[SettingsProperty(SettingsProperty.ePropertyType.DeviceId)]
		public int? SwitcherId { get; set; }

		/// <summary>
		/// Writes property elements to xml.
		/// </summary>
		/// <param name="writer"></param>
		protected override void WriteElements(IcdXmlTextWriter writer)
		{
			base.WriteElements(writer);

			if (CresnetId != null)
				writer.WriteElementString(CRESNET_ID_ELEMENT, StringUtils.ToIpIdString((byte)CresnetId));

			if (CardNumber != null)
				writer.WriteElementString(CARD_NUMBER_ELEMENT, IcdXmlConvert.ToString((int)CardNumber));

			if (SwitcherId != null)
				writer.WriteElementString(SWITCHER_ID_ELEMENT, IcdXmlConvert.ToString((int)SwitcherId));
		}

		/// <summary>
		/// Parses the xml and applies the properties to the instance.
		/// </summary>
		/// <param name="instance"></param>
		/// <param name="xml"></param>
		protected static void ParseXml(AbstractCardSettings instance, string xml)
		{
			instance.CresnetId = XmlUtils.TryReadChildElementContentAsByte(xml, CRESNET_ID_ELEMENT);
			instance.CardNumber = XmlUtils.TryReadChildElementContentAsInt(xml, CARD_NUMBER_ELEMENT);
			instance.SwitcherId = XmlUtils.TryReadChildElementContentAsInt(xml, SWITCHER_ID_ELEMENT);

			AbstractDeviceSettings.ParseXml(instance, xml);
		}
	}
}
