using ICD.Common.Utils.Xml;
using ICD.Connect.Settings.Attributes;

namespace ICD.Connect.Routing.Extron.Devices.Endpoints.Tx
{
	[KrangSettings("DtpHdmiTx", typeof(DtpHdmiTx))]
	public sealed class DtpHdmiTxSettings : AbstractDtpHdmiDeviceSettings, IDtpHdmiTxDeviceSettings
	{
		private const string DTP_INPUT_ELEMENT = "DtpInput";

		public int DtpInput { get; set; }

		protected override void WriteElements(IcdXmlTextWriter writer)
		{
			base.WriteElements(writer);

			writer.WriteElementString(DTP_INPUT_ELEMENT, IcdXmlConvert.ToString(DtpInput));
		}

		public override void ParseXml(string xml)
		{
			base.ParseXml(xml);

			DtpInput = XmlUtils.TryReadChildElementContentAsInt(xml, DTP_INPUT_ELEMENT) ?? 1;
		}
	}
}