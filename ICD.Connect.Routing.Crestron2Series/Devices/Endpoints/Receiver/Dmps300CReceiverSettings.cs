using ICD.Common.Utils.Xml;
using ICD.Connect.Settings.Attributes;

namespace ICD.Connect.Routing.Crestron2Series.Devices.Endpoints.Receiver
{
	[KrangSettings("Dmps300CReceiver", typeof(Dmps300CReceiver))]
	public sealed class Dmps300CReceiverSettings : AbstractDmps300CEndpointDeviceSettings
	{
		private const string DM_OUTPUT_ELEMENT = "DmOutput";

		public int DmOutput { get; set; }

		/// <summary>
		/// Writes property elements to xml.
		/// </summary>
		/// <param name="writer"></param>
		protected override void WriteElements(IcdXmlTextWriter writer)
		{
			base.WriteElements(writer);

			writer.WriteElementString(DM_OUTPUT_ELEMENT, IcdXmlConvert.ToString(DmOutput));
		}

		/// <summary>
		/// Updates the settings from xml.
		/// </summary>
		/// <param name="xml"></param>
		public override void ParseXml(string xml)
		{
			base.ParseXml(xml);

			DmOutput = XmlUtils.TryReadChildElementContentAsInt(xml, DM_OUTPUT_ELEMENT) ?? 0;
		}
	}
}
