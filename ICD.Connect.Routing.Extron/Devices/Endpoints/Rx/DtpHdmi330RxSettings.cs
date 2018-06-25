using ICD.Common.Utils.Xml;
using ICD.Connect.Settings.Attributes;

namespace ICD.Connect.Routing.Extron.Devices.Endpoints.Rx
{
    [KrangSettings("DtpHdmi330Rx", typeof(DtpHdmi330Rx))]
	public class DtpHdmi330RxSettings : AbstractDtpHdmiDeviceSettings, IDtpHdmiRxDeviceSettings
	{
		private const string DTP_OUTPUT_ELEMENT = "DtpOutput";

		public int? DtpOutput { get; set; }

		protected override void WriteElements(IcdXmlTextWriter writer)
		{
			base.WriteElements(writer);

			writer.WriteElementString(DTP_OUTPUT_ELEMENT, DtpOutput.ToString());
		}

		public override void ParseXml(string xml)
		{
			base.ParseXml(xml);

			DtpOutput = XmlUtils.TryReadChildElementContentAsInt(xml, DTP_OUTPUT_ELEMENT);
		}
	}
}