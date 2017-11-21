using System;
using ICD.Common.Properties;
using ICD.Common.Utils.Xml;
using ICD.Connect.Settings.Attributes;

namespace ICD.Connect.Routing.Crestron2Series.Devices.Endpoints.Receiver
{
	public sealed class Dmps300CReceiverSettings : AbstractDmps300CEndpointDeviceSettings
	{
		private const string FACTORY_NAME = "Dmps300CReceiver";

		private const string DM_OUTPUT_ELEMENT = "DmOutput";

		public int DmOutput { get; set; }

		/// <summary>
		/// Gets the originator factory name.
		/// </summary>
		public override string FactoryName { get { return FACTORY_NAME; } }

		/// <summary>
		/// Gets the type of the originator for this settings instance.
		/// </summary>
		public override Type OriginatorType { get { return typeof(Dmps300CReceiver); } }

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
		/// Loads the settings from XML.
		/// </summary>
		/// <param name="xml"></param>
		/// <returns></returns>
		[PublicAPI, XmlFactoryMethod(FACTORY_NAME)]
		public static Dmps300CReceiverSettings FromXml(string xml)
		{
			Dmps300CReceiverSettings output = new Dmps300CReceiverSettings
			{
				DmOutput = XmlUtils.TryReadChildElementContentAsInt(xml, DM_OUTPUT_ELEMENT) ?? 0
			};

			ParseXml(output, xml);
			return output;
		}
	}
}