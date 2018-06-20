using ICD.Common.Utils.Xml;
using ICD.Connect.Settings;

namespace ICD.Connect.Routing.Extron.Devices.Dtp
{
	public abstract class AbstractDtpHdmiDeviceSettings : AbstractSettings, IDtpHdmiDeviceSettings
	{
		private const string DTP_SWITCH_ELEMENT = "DtpSwitch";

		public int? DtpSwitch { get; set; }

		protected override void WriteElements(IcdXmlTextWriter writer)
		{
			base.WriteElements(writer);

			writer.WriteElementString(DTP_SWITCH_ELEMENT, DtpSwitch.ToString());
		}

		public override void ParseXml(string xml)
		{
			base.ParseXml(xml);

			DtpSwitch = XmlUtils.TryReadChildElementContentAsInt(xml, DTP_SWITCH_ELEMENT);
		}
	}
}