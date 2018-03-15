using ICD.Common.Utils;
using ICD.Common.Utils.Xml;
using ICD.Connect.Devices;
using ICD.Connect.Settings.Attributes.SettingsProperties;

namespace ICD.Connect.Routing.CrestronPro.DigitalMedia.DmMdMNXN
{
// ReSharper disable once InconsistentNaming
	public abstract class AbstractDmMdMNXNAdapterSettings : AbstractDeviceSettings, IDmMdNXNAdapterSettings
	{
		private const string IPID_ELEMENT = "IPID";

		[IpIdSettingsProperty]
		public byte? Ipid { get; set; }

		/// <summary>
		/// Writes property elements to xml.
		/// </summary>
		/// <param name="writer"></param>
		protected override void WriteElements(IcdXmlTextWriter writer)
		{
			base.WriteElements(writer);

			writer.WriteElementString(IPID_ELEMENT, Ipid == null ? null : StringUtils.ToIpIdString(Ipid.Value));
		}

		/// <summary>
		/// Updates the settings from xml.
		/// </summary>
		/// <param name="xml"></param>
		public override void ParseXml(string xml)
		{
			base.ParseXml(xml);

			Ipid = XmlUtils.ReadChildElementContentAsByte(xml, IPID_ELEMENT);
		}
	}
}
