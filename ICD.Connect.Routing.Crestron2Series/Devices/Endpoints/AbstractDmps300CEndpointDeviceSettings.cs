﻿using ICD.Common.Utils.Xml;
using ICD.Connect.Settings.Attributes;

namespace ICD.Connect.Routing.Crestron2Series.Devices.Endpoints
{
	public abstract class AbstractDmps300CEndpointDeviceSettings : AbstractDmps300CDeviceSettings,
	                                                               IDmps300CEndpointDeviceSettings
	{
		private const string PARENT_DEVICE_ELEMENT = "Device";

		[SettingsProperty(SettingsProperty.ePropertyType.Id, typeof(IDmps300CDevice))]
		public int Device { get; set; }

		/// <summary>
		/// Writes property elements to xml.
		/// </summary>
		/// <param name="writer"></param>
		protected override void WriteElements(IcdXmlTextWriter writer)
		{
			base.WriteElements(writer);

			writer.WriteElementString(PARENT_DEVICE_ELEMENT, IcdXmlConvert.ToString(Device));
		}

		/// <summary>
		/// Parses the xml and applies the properties to the instance.
		/// </summary>
		/// <param name="instance"></param>
		/// <param name="xml"></param>
		protected static void ParseXml(AbstractDmps300CEndpointDeviceSettings instance, string xml)
		{
			instance.Device = XmlUtils.TryReadChildElementContentAsInt(xml, PARENT_DEVICE_ELEMENT) ?? 0;

			ParseXml((AbstractDmps300CDeviceSettings)instance, xml);
		}
	}
}