using ICD.Common.Utils.Xml;
using ICD.Connect.Devices.CrestronSPlus.Devices.SPlus;
using ICD.Connect.Routing.Connections;
using ICD.Connect.Settings.Attributes;

namespace ICD.Connect.Routing.SPlus.SPlusSwitcher.Device
{
	[KrangSettings("SPlusSwitcherDevice", typeof(SPlusSwitcherDevice))]
	public sealed class SPlusSwitcherDeviceSettings : AbstractSPlusDeviceSettings
	{

		#region Consts

		private const string INPUT_COUNT_ELEMENT = "InputCount";
		private const string OUTPUT_COUNT_ELEMENT = "OutputCount";
		private const string SWITCHER_LAYERS_ELEMNET = "SwitcherLayers";
		private const string SUPPORTS_SOURCE_DETECTION_ELEMENT = "SupportsSourceDetection";

		#endregion

		#region Properties
		public ushort InputCount { get; set; }
		
		public ushort OutputCount { get; set; }

		public eConnectionType SwitcherLayers { get; set; }

		public bool SupportsSourceDetection { get; set; }

		#endregion

		#region XML

		/// <summary>
		/// Updates the settings from xml.
		/// </summary>
		/// <param name="xml"></param>
		public override void ParseXml(string xml)
		{
			base.ParseXml(xml);

			InputCount = XmlUtils.ReadChildElementContentAsUShort(xml, INPUT_COUNT_ELEMENT);
			OutputCount = XmlUtils.ReadChildElementContentAsUShort(xml, OUTPUT_COUNT_ELEMENT);
			SwitcherLayers = XmlUtils.ReadChildElementContentAsEnum<eConnectionType>(xml, SWITCHER_LAYERS_ELEMNET,true);
			SupportsSourceDetection = XmlUtils.TryReadChildElementContentAsBoolean(xml, SUPPORTS_SOURCE_DETECTION_ELEMENT) ??
			                          false;
		}

		/// <summary>
		/// Writes property elements to xml.
		/// </summary>
		/// <param name="writer"></param>
		protected override void WriteElements(IcdXmlTextWriter writer)
		{
			base.WriteElements(writer);

			writer.WriteElementString(INPUT_COUNT_ELEMENT,IcdXmlConvert.ToString(InputCount));
			writer.WriteElementString(OUTPUT_COUNT_ELEMENT, IcdXmlConvert.ToString(OutputCount));
			writer.WriteElementString(SWITCHER_LAYERS_ELEMNET, SwitcherLayers.ToString());
			writer.WriteElementString(SUPPORTS_SOURCE_DETECTION_ELEMENT, IcdXmlConvert.ToString(SupportsSourceDetection));
		}

		#endregion
	}
}
