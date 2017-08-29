using System;
using ICD.Common.Properties;
using ICD.Common.Utils;
using ICD.Common.Utils.Xml;
using ICD.Connect.Devices;
using ICD.Connect.Settings.Attributes;
using ICD.Connect.Settings.Attributes.Factories;

namespace ICD.Connect.Routing.CrestronPro.Transmitters.DmTx4K302C
{
	public sealed class DmTx4K302CAdapterSettings : AbstractDeviceSettings
	{
		private const string FACTORY_NAME = "DmTx4k302C";

		private const string IPID_ELEMENT = "IPID";
		private const string DM_SWITCH_ELEMENT = "DmSwitch";
		private const string DM_INPUT_ELEMENT = "DmInput";

		/// <summary>
		/// Gets the originator factory name.
		/// </summary>
		public override string FactoryName { get { return FACTORY_NAME; } }

		/// <summary>
		/// Gets the type of the originator for this settings instance.
		/// </summary>
		public override Type OriginatorType { get { return typeof(DmTx4K302CAdapter); } }

		[SettingsProperty(SettingsProperty.ePropertyType.Ipid)]
		public byte? Ipid { get; set; }

		[SettingsProperty(SettingsProperty.ePropertyType.DeviceId)]
		public int? DmSwitch { get; set; }

		public int? DmInputAddress { get; set; }

		/// <summary>
		/// Writes property elements to xml.
		/// </summary>
		/// <param name="writer"></param>
		protected override void WriteElements(IcdXmlTextWriter writer)
		{
			base.WriteElements(writer);

			if (Ipid != null)
				writer.WriteElementString(IPID_ELEMENT, StringUtils.ToIpIdString((byte)Ipid));

			if (DmSwitch != null)
				writer.WriteElementString(DM_SWITCH_ELEMENT, IcdXmlConvert.ToString((int)DmSwitch));

			if (DmInputAddress != null)
				writer.WriteElementString(DM_INPUT_ELEMENT, IcdXmlConvert.ToString((int)DmInputAddress));
		}

		/// <summary>
		/// Loads the settings from XML.
		/// </summary>
		/// <param name="xml"></param>
		/// <returns></returns>
		[PublicAPI, XmlDeviceSettingsFactoryMethod(FACTORY_NAME)]
		public static DmTx4K302CAdapterSettings FromXml(string xml)
		{
			DmTx4K302CAdapterSettings output = new DmTx4K302CAdapterSettings
			{
				Ipid = XmlUtils.TryReadChildElementContentAsByte(xml, IPID_ELEMENT),
				DmSwitch = XmlUtils.TryReadChildElementContentAsInt(xml, DM_SWITCH_ELEMENT),
				DmInputAddress = XmlUtils.TryReadChildElementContentAsInt(xml, DM_INPUT_ELEMENT),
			};

			ParseXml(output, xml);
			return output;
		}
	}
}
