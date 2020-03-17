using ICD.Common.Utils.Xml;
using ICD.Connect.Devices;
using ICD.Connect.Settings.Attributes;
using ICD.Connect.Settings.Attributes.SettingsProperties;

namespace ICD.Connect.Routing.CrestronPro.ControlSystem
{
	[KrangSettings("ControlSystem", typeof(ControlSystemDevice))]
	public sealed class ControlSystemDeviceSettings : AbstractDeviceSettings
	{
		private const string ELEMENT_CONFIG = "Config";
		private const string ELEMENT_OUTPUT_1_MIXER_MODE = "Output1MixerMode";
		private const string ELEMENT_OUTPUT_2_MIXER_MODE = "Output2MixerMode";
		private const string ELEMENT_OUTPUT_3_MIXER_MODE = "Output3MixerMode";
		private const string ELEMENT_OUTPUT_4_MIXER_MODE = "Output4MixerMode";

		[PathSettingsProperty("DMPS3", ".xml")]
		public string Config { get; set; }

		public eOutputMixerMode Output1MixerMode { get; set; }
		public eOutputMixerMode Output2MixerMode { get; set; }
		public eOutputMixerMode Output3MixerMode { get; set; }
		public eOutputMixerMode Output4MixerMode { get; set; }

		/// <summary>
		/// Writes property elements to xml.
		/// </summary>
		/// <param name="writer"></param>
		protected override void WriteElements(IcdXmlTextWriter writer)
		{
			base.WriteElements(writer);

			writer.WriteElementString(ELEMENT_CONFIG, Config);
			writer.WriteElementString(ELEMENT_OUTPUT_1_MIXER_MODE, IcdXmlConvert.ToString(Output1MixerMode));
			writer.WriteElementString(ELEMENT_OUTPUT_2_MIXER_MODE, IcdXmlConvert.ToString(Output2MixerMode));
			writer.WriteElementString(ELEMENT_OUTPUT_3_MIXER_MODE, IcdXmlConvert.ToString(Output3MixerMode));
			writer.WriteElementString(ELEMENT_OUTPUT_4_MIXER_MODE, IcdXmlConvert.ToString(Output4MixerMode));

		}

		/// <summary>
		/// Updates the settings from xml.
		/// </summary>
		/// <param name="xml"></param>
		public override void ParseXml(string xml)
		{
			base.ParseXml(xml);

			Config = XmlUtils.TryReadChildElementContentAsString(xml, ELEMENT_CONFIG);

			Output1MixerMode = XmlUtils.TryReadChildElementContentAsEnum<eOutputMixerMode>(xml, ELEMENT_OUTPUT_1_MIXER_MODE,true) ??
			                   eOutputMixerMode.Auto;
			Output2MixerMode = XmlUtils.TryReadChildElementContentAsEnum<eOutputMixerMode>(xml, ELEMENT_OUTPUT_2_MIXER_MODE, true) ??
							   eOutputMixerMode.Auto;
			Output3MixerMode = XmlUtils.TryReadChildElementContentAsEnum<eOutputMixerMode>(xml, ELEMENT_OUTPUT_3_MIXER_MODE, true) ??
							   eOutputMixerMode.Auto;
			Output4MixerMode = XmlUtils.TryReadChildElementContentAsEnum<eOutputMixerMode>(xml, ELEMENT_OUTPUT_4_MIXER_MODE, true) ??
							   eOutputMixerMode.Auto;
		}
	}
}
