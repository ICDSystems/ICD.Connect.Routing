using System;
using ICD.Common.Utils;
using ICD.Common.Utils.Xml;
using ICD.Connect.Devices;
using ICD.Connect.Settings.Attributes.SettingsProperties;

namespace ICD.Connect.Routing.CrestronPro.DigitalMedia
{
	public abstract class AbstractDmSwitcherAdapterSettings : AbstractDeviceSettings, IDmSwitcherAdapterSettings
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

		/// <summary>
		/// Loads the settings from XML.
		/// </summary>
		/// <param name="instance"></param>
		/// <param name="xml"></param>
		/// <returns></returns>
		protected static void ParseXml(AbstractDmSwitcherAdapterSettings instance, string xml)
		{
			if (instance == null)
				throw new ArgumentNullException("instance");

			instance.Ipid = XmlUtils.TryReadChildElementContentAsByte(xml, IPID_ELEMENT) ?? 0;

			AbstractDeviceSettings.ParseXml(instance, xml);
		}
	}
}