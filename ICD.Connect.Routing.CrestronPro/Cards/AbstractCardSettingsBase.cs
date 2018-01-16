using ICD.Common.Utils.Xml;
using ICD.Connect.Devices;
using ICD.Connect.Routing.CrestronPro.DigitalMedia;
using ICD.Connect.Settings.Attributes.SettingsProperties;

namespace ICD.Connect.Routing.CrestronPro.Cards
{
	public abstract class AbstractCardSettingsBase : AbstractDeviceSettings, ICardSettings
	{
		private const string CARD_NUMBER_ELEMENT = "CardNumber";
		private const string SWITCHER_ID_ELEMENT = "SwitcherId";

		public int? CardNumber { get; set; }

		[OriginatorIdSettingsProperty(typeof(IDmSwitcherAdapter))]
		public int? SwitcherId { get; set; }

		/// <summary>
		/// Writes property elements to xml.
		/// </summary>
		/// <param name="writer"></param>
		protected override void WriteElements(IcdXmlTextWriter writer)
		{
			base.WriteElements(writer);

			writer.WriteElementString(CARD_NUMBER_ELEMENT, CardNumber == null ? null : IcdXmlConvert.ToString((int)CardNumber));

			writer.WriteElementString(SWITCHER_ID_ELEMENT, SwitcherId == null ? null : IcdXmlConvert.ToString((int)SwitcherId));
		}

		/// <summary>
		/// Parses the xml and applies the properties to the instance.
		/// </summary>
		/// <param name="instance"></param>
		/// <param name="xml"></param>
		protected static void ParseXml(AbstractCardSettingsBase instance, string xml)
		{
			instance.CardNumber = XmlUtils.TryReadChildElementContentAsInt(xml, CARD_NUMBER_ELEMENT);
			instance.SwitcherId = XmlUtils.TryReadChildElementContentAsInt(xml, SWITCHER_ID_ELEMENT);

			AbstractDeviceSettings.ParseXml(instance, xml);
		}
	}
}
