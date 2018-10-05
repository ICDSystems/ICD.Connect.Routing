using ICD.Common.Utils.Xml;
using ICD.Connect.Routing.CrestronPro.DigitalMedia.Dm100xStrBase;

namespace ICD.Connect.Routing.CrestronPro.DigitalMedia.DmNvxBaseClass
{
	public abstract class AbstractDmNvxBaseClassAdapterSettings : AbstractDm100XStrBaseAdapterSettings,
	                                                              IDmNvxBaseClassAdapterSettings
	{
		private const string DEVICE_MODE_ELEMENT = "DeviceMode";

		public eDeviceMode DeviceMode { get; set; }

		/// <summary>
		/// Writes property elements to xml.
		/// </summary>
		/// <param name="writer"></param>
		protected override void WriteElements(IcdXmlTextWriter writer)
		{
			base.WriteElements(writer);

			writer.WriteElementString(DEVICE_MODE_ELEMENT, IcdXmlConvert.ToString(DeviceMode));
		}

		/// <summary>
		/// Updates the settings from xml.
		/// </summary>
		/// <param name="xml"></param>
		public override void ParseXml(string xml)
		{
			base.ParseXml(xml);

			DeviceMode = XmlUtils.TryReadChildElementContentAsEnum<eDeviceMode>(xml, DEVICE_MODE_ELEMENT, true) ??
			             eDeviceMode.Receiver;
		}
	}
}
