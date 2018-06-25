using ICD.Common.Utils.Xml;
using ICD.Connect.Protocol.Ports.ComPort;
using ICD.Connect.Routing.Extron.Devices.Endpoints;
using ICD.Connect.Routing.Extron.Devices.Switchers;
using ICD.Connect.Settings.Attributes;
using ICD.Connect.Settings.Attributes.SettingsProperties;

namespace ICD.Connect.Routing.Extron.Ports
{
	[KrangSettings("DtpCrosspointComPort", typeof(DtpCrosspointComPort))]
	public class DtpCrosspointComPortSettings : AbstractComPortSettings
	{
		private const string PARENT_ELEMENT = "Parent";
		private const string MODE_ELEMENT = "Mode";

		[OriginatorIdSettingsProperty(typeof (IDtpHdmiDevice))]
		public int Parent { get; set; }

		public eExtronPortInsertionMode? Mode { get; set; }

		protected override void WriteElements(IcdXmlTextWriter writer)
		{
			base.WriteElements(writer);

			writer.WriteElementString(PARENT_ELEMENT, Parent.ToString());
			writer.WriteElementString(MODE_ELEMENT, IcdXmlConvert.ToString(Mode));
		}

		public override void ParseXml(string xml)
		{
			base.ParseXml(xml);

			Parent = XmlUtils.ReadChildElementContentAsInt(xml, PARENT_ELEMENT);
			Mode = XmlUtils.TryReadChildElementContentAsEnum<eExtronPortInsertionMode>(xml, MODE_ELEMENT, true)
			       ?? eExtronPortInsertionMode.Ethernet;
		}
	}
}