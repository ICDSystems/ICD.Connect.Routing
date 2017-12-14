﻿using System;
using ICD.Common.Properties;
using ICD.Common.Utils.Xml;
using ICD.Connect.Protocol.Ports.DigitalInput;
using ICD.Connect.Routing.Crestron2Series.Devices;
using ICD.Connect.Settings.Attributes;
using ICD.Connect.Settings.Attributes.SettingsProperties;

namespace ICD.Connect.Routing.Crestron2Series.Ports.DigitalInputPort
{
	public sealed class Dmps300CDigitalInputPortSettings : AbstractDigitalInputPortSettings
	{
		private const string FACTORY_NAME = "Dmps300CDigitalInputPort";

		private const string DEVICE_ELEMENT = "Device";
		private const string ADDRESS_ELEMENT = "Address";

		[OriginatorIdSettingsProperty(typeof(IDmps300CDevice))]
		public int Device { get; set; }

		public int Address { get; set; }

		/// <summary>
		/// Gets the originator factory name.
		/// </summary>
		public override string FactoryName { get { return FACTORY_NAME; } }

		/// <summary>
		/// Gets the type of the originator for this settings instance.
		/// </summary>
		public override Type OriginatorType { get { return typeof(Dmps300CDigitalInputPort); } }

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
		public static Dmps300CDigitalInputPortSettings FromXml(string xml)
		{
			Dmps300CDigitalInputPortSettings output = new Dmps300CDigitalInputPortSettings
			{
				Device = XmlUtils.TryReadChildElementContentAsInt(xml, DEVICE_ELEMENT) ?? 0,
				Address = XmlUtils.TryReadChildElementContentAsInt(xml, ADDRESS_ELEMENT) ?? 0
			};

			ParseXml(output, xml);
			return output;
		}
	}
}