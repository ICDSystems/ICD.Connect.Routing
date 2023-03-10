using ICD.Common.Utils.Xml;
using ICD.Connect.Settings.Attributes;

namespace ICD.Connect.Routing.Extron.Devices.Endpoints.Rx
{
    [KrangSettings("DtpHdmiRx", typeof(DtpHdmiRx))]
	public sealed class DtpHdmiRxSettings : AbstractDtpHdmiDeviceSettings, IDtpHdmiRxDeviceSettings
	{
		private const string DTP_OUTPUT_ELEMENT = "DtpOutput";

		public int DtpOutput { get; set; }

		protected override void WriteElements(IcdXmlTextWriter writer)
		{
			base.WriteElements(writer);

			writer.WriteElementString(DTP_OUTPUT_ELEMENT, IcdXmlConvert.ToString(DtpOutput));
		}

		public override void ParseXml(string xml)
		{
			base.ParseXml(xml);

			DtpOutput = XmlUtils.TryReadChildElementContentAsInt(xml, DTP_OUTPUT_ELEMENT) ?? 1;
		}
	}
}