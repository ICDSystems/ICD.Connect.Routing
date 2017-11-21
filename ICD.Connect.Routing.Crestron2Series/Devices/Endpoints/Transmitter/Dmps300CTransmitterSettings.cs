using System;
using ICD.Common.Properties;
using ICD.Common.Utils.Xml;
using ICD.Connect.Settings.Attributes;

namespace ICD.Connect.Routing.Crestron2Series.Devices.Endpoints.Transmitter
{
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
		/// Loads the settings from XML.
		/// </summary>
		/// <param name="xml"></param>
		/// <returns></returns>
		[PublicAPI, XmlFactoryMethod(FACTORY_NAME)]
		public static Dmps300CTransmitterSettings FromXml(string xml)
		{
			Dmps300CTransmitterSettings output = new Dmps300CTransmitterSettings
			{
				DmInput = XmlUtils.TryReadChildElementContentAsInt(xml, DM_INPUT_ELEMENT) ?? 0
			};

			ParseXml(output, xml);
			return output;
		}
	}
}