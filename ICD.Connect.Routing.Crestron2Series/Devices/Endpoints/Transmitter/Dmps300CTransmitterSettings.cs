using System;
using ICD.Common.Utils.Xml;
using ICD.Connect.Settings.Attributes;

namespace ICD.Connect.Routing.Crestron2Series.Devices.Endpoints.Transmitter
{
	[KrangSettings(FACTORY_NAME)]
	public sealed class Dmps300CTransmitterSettings : AbstractDmps300CEndpointDeviceSettings
	{
		private const string FACTORY_NAME = "Dmps300CTransmitter";

		private const string DM_INPUT_ELEMENT = "DmInput";

		public int DmInput { get; set; }

		/// <summary>
		/// Gets the originator factory name.
		/// </summary>
		public override string FactoryName { get { return FACTORY_NAME; } }

		/// <summary>
		/// Gets the type of the originator for this settings instance.
		/// </summary>
		public override Type OriginatorType { get { return typeof(Dmps300CTransmitter); } }

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