using ICD.Common.Utils.Xml;
using ICD.Connect.Settings.Attributes;

namespace ICD.Connect.Routing.Crestron2Series.Devices.Endpoints.Transmitter
{
	[KrangSettings("Dmps300CTransmitter", typeof(Dmps300CTransmitter))]
	public sealed class Dmps300CTransmitterSettings : AbstractDmps300CEndpointDeviceSettings
	{
		private const string DM_INPUT_ELEMENT = "DmInput";

		public int DmInput { get; set; }

		/// <summary>
		/// Writes property elements to xml.
		/// </summary>
		/// <param name="writer"></param>
		protected override void WriteElements(IcdXmlTextWriter writer)
		{
			base.WriteElements(writer);

			writer.WriteElementString(DM_INPUT_ELEMENT, IcdXmlConvert.ToString(DmInput));
		}

		/// <summary>
		/// Updates the settings from xml.
		/// </summary>
		/// <param name="xml"></param>
		public override void ParseXml(string xml)
		{
			base.ParseXml(xml);

			DmInput = XmlUtils.TryReadChildElementContentAsInt(xml, DM_INPUT_ELEMENT) ?? 0;
		}
	}
}