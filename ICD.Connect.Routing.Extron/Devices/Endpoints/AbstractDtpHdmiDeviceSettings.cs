using ICD.Common.Utils.Xml;
using ICD.Connect.Devices;
using ICD.Connect.Routing.Extron.Devices.Switchers.DtpCrosspoint;
using ICD.Connect.Settings.Attributes.SettingsProperties;

namespace ICD.Connect.Routing.Extron.Devices.Endpoints
{
	public abstract class AbstractDtpHdmiDeviceSettings : AbstractDeviceSettings, IDtpHdmiDeviceSettings
	{
		private const string DTP_SWITCH_ELEMENT = "DtpSwitch";

        [OriginatorIdSettingsProperty(typeof(IDtpCrosspointDevice))]
		public int? DtpSwitch { get; set; }

		protected override void WriteElements(IcdXmlTextWriter writer)
		{
			base.WriteElements(writer);

			writer.WriteElementString(DTP_SWITCH_ELEMENT, IcdXmlConvert.ToString(DtpSwitch));
		}

		public override void ParseXml(string xml)
		{
			base.ParseXml(xml);

			DtpSwitch = XmlUtils.TryReadChildElementContentAsInt(xml, DTP_SWITCH_ELEMENT);
		}
	}
}