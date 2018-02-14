using ICD.Common.Utils.Xml;
using ICD.Connect.Settings.Attributes.SettingsProperties;

namespace ICD.Connect.Routing.Crestron2Series.Devices.Endpoints
{
	public abstract class AbstractDmps300CEndpointDeviceSettings : AbstractDmps300CDeviceSettings,
	                                                               IDmps300CEndpointDeviceSettings
	{
		private const string PARENT_DEVICE_ELEMENT = "Device";

		[OriginatorIdSettingsProperty(typeof(IDmps300CDevice))]
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
		/// Updates the settings from xml.
		/// </summary>
		/// <param name="xml"></param>
		public override void ParseXml(string xml)
		{
			base.ParseXml(xml);

			Device = XmlUtils.TryReadChildElementContentAsInt(xml, PARENT_DEVICE_ELEMENT) ?? 0;
		}
	}
}