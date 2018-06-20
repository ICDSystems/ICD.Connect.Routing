using ICD.Common.Utils.Xml;
using ICD.Connect.Settings.Attributes;

namespace ICD.Connect.Routing.Extron.Devices.Dtp.Tx
{
	[KrangSettings("DtpHdmi330Tx", typeof(DtpHdmi330Tx))]
	public class DtpHdmi330TxSettings : AbstractDtpHdmiDeviceSettings, IDtpHdmiTxDeviceSettings
	{
		private const string DTP_INPUT_ELEMENT = "DtpInput";

		public int? DtpInput { get; set; }

		protected override void WriteElements(IcdXmlTextWriter writer)
		{
			base.WriteElements(writer);

			writer.WriteElementString(DTP_INPUT_ELEMENT, DtpInput.ToString());
		}

		public override void ParseXml(string xml)
		{
			base.ParseXml(xml);

			DtpInput = XmlUtils.TryReadChildElementContentAsInt(xml, DTP_INPUT_ELEMENT);
		}
	}
}