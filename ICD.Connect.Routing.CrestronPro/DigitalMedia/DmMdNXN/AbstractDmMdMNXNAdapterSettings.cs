using ICD.Common.Utils;
using ICD.Common.Utils.Xml;
using ICD.Connect.Devices;
using ICD.Connect.Settings.Attributes.SettingsProperties;

namespace ICD.Connect.Routing.CrestronPro.DigitalMedia.DmMdNXN
{
// ReSharper disable once InconsistentNaming
	public abstract class AbstractDmMdMNXNAdapterSettings : AbstractDeviceSettings, IDmMdNXNAdapterSettings
	{
		private const string IPID_ELEMENT = "IPID";

		[IpIdSettingsProperty]
		public byte Ipid { get; set; }

		/// <summary>
		/// Writes property elements to xml.
		/// </summary>
		/// <param name="writer"></param>
		protected override void WriteElements(IcdXmlTextWriter writer)
		{
			base.WriteElements(writer);

			writer.WriteElementString(IPID_ELEMENT, StringUtils.ToIpIdString(Ipid));
		}

		public static void ParseXml(AbstractDmMdMNXNAdapterSettings instance, string xml)
		{
			instance.Ipid = XmlUtils.ReadChildElementContentAsByte(xml, IPID_ELEMENT);

			AbstractDeviceSettings.ParseXml(instance, xml);
		}
	}
}
