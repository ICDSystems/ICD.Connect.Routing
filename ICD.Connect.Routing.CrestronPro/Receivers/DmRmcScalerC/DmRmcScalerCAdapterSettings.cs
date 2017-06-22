using ICD.Common.Attributes.Properties;
using ICD.Common.Properties;
using ICD.Common.Utils;
using ICD.Common.Utils.Xml;
using ICD.Connect.Settings;
using ICD.Connect.Settings.Attributes.Factories;
using ICD.Connect.Settings.Core;

namespace ICD.Connect.Routing.CrestronPro.Receivers.DmRmcScalerC
{
	/// <summary>
	/// Settings for the DmRmcScalerCAdapter.
	/// </summary>
	public sealed class DmRmcScalerCAdapterSettings : AbstractDeviceSettings
	{
		private const string FACTORY_NAME = "DmRmcScalerC";

		private const string IPID_ELEMENT = "IPID";
		private const string DM_SWITCH_ELEMENT = "DmSwitch";
		private const string DM_OUTPUT_ELEMENT = "DmOutput";

		[SettingsProperty(SettingsProperty.ePropertyType.Ipid)]
		public byte? Ipid { get; set; }

		[SettingsProperty(SettingsProperty.ePropertyType.DeviceId)]
		public int? DmSwitch { get; set; }

		public int? DmOutputAddress { get; set; }

		/// <summary>
		/// Gets the originator factory name.
		/// </summary>
		public override string FactoryName { get { return FACTORY_NAME; } }

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

			if (DmOutputAddress != null)
				writer.WriteElementString(DM_OUTPUT_ELEMENT, IcdXmlConvert.ToString((int)DmOutputAddress));
		}

		/// <summary>
		/// Creates a new originator instance from the settings.
		/// </summary>
		/// <param name="factory"></param>
		/// <returns></returns>
		public override IOriginator ToOriginator(IDeviceFactory factory)
		{
			DmRmcScalerCAdapter output = new DmRmcScalerCAdapter();
			output.ApplySettings(this, factory);
			return output;
		}

		/// <summary>
		/// Loads the settings from XML.
		/// </summary>
		/// <param name="xml"></param>
		/// <returns></returns>
		[PublicAPI, XmlDeviceSettingsFactoryMethod(FACTORY_NAME)]
		public static DmRmcScalerCAdapterSettings FromXml(string xml)
		{
			byte? ipid = XmlUtils.TryReadChildElementContentAsByte(xml, IPID_ELEMENT);
			int? dmSwitch = XmlUtils.TryReadChildElementContentAsInt(xml, DM_SWITCH_ELEMENT);
			int? outputAddress = XmlUtils.TryReadChildElementContentAsInt(xml, DM_OUTPUT_ELEMENT);

			DmRmcScalerCAdapterSettings output = new DmRmcScalerCAdapterSettings
			{
				Ipid = ipid,
				DmSwitch = dmSwitch,
				DmOutputAddress = outputAddress
			};

			ParseXml(output, xml);
			return output;
		}
	}
}
