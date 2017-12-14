using System;
using ICD.Common.Properties;
using ICD.Common.Utils.Xml;
using ICD.Connect.Protocol.Ports.RelayPort;
using ICD.Connect.Routing.Crestron2Series.Devices;
using ICD.Connect.Settings.Attributes;
using ICD.Connect.Settings.Attributes.SettingsProperties;

namespace ICD.Connect.Routing.Crestron2Series.Ports.RelayPort
{
	public sealed class Dmps300CRelayPortSettings : AbstractRelayPortSettings
	{
		private const string FACTORY_NAME = "Dmps300CRelayPort";

		private const string DEVICE_ELEMENT = "Device";
		private const string ADDRESS_ELEMENT = "Address";

		[OriginatorIdSettingsProperty(typeof(IDmps300CRelayPortDevice))]
		public int Device { get; set; }

		public int Address { get; set; }

		/// <summary>
		/// Gets the originator factory name.
		/// </summary>
		public override string FactoryName { get { return FACTORY_NAME; } }

		/// <summary>
		/// Gets the type of the originator for this settings instance.
		/// </summary>
		public override Type OriginatorType { get { return typeof(Dmps300CRelayPort); } }

		/// <summary>
		/// Writes property elements to xml.
		/// </summary>
		/// <param name="writer"></param>
		protected override void WriteElements(IcdXmlTextWriter writer)
		{
			base.WriteElements(writer);

			writer.WriteElementString(DEVICE_ELEMENT, IcdXmlConvert.ToString(Device));
			writer.WriteElementString(ADDRESS_ELEMENT, IcdXmlConvert.ToString(Address));
		}

		/// <summary>
		/// Loads the settings from XML.
		/// </summary>
		/// <param name="xml"></param>
		/// <returns></returns>
		[PublicAPI, XmlFactoryMethod(FACTORY_NAME)]
		public static Dmps300CRelayPortSettings FromXml(string xml)
		{
			Dmps300CRelayPortSettings output = new Dmps300CRelayPortSettings
			{
				Device = XmlUtils.TryReadChildElementContentAsInt(xml, DEVICE_ELEMENT) ?? 0,
				Address = XmlUtils.TryReadChildElementContentAsInt(xml, ADDRESS_ELEMENT) ?? 0
			};

			ParseXml(output, xml);
			return output;
		}
	}
}