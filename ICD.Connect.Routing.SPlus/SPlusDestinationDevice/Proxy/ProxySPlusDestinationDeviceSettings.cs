using ICD.Common.Utils.Xml;
using ICD.Connect.Devices.Proxies.Devices;
using ICD.Connect.Settings.Attributes;

namespace ICD.Connect.Routing.SPlus.SPlusDestinationDevice.Proxy
{
	[KrangSettings("ProxySPlusDestinationDevice",typeof(ProxySPlusDestinationDevice))]
	public sealed class ProxySPlusDestinationDeviceSettings : AbstractProxyDeviceSettings
	{
		#region Consts

		private const string POWER_CONTROL_ELEMENT = "PowerControl";
		private const string VOLUME_CONTROL_ELEMENT = "VolumeControl";
		private const string INPUT_COUNT_ELEMENT = "InputCount";

		#endregion

		#region Properties

		public bool PowerControl { get; set; }

		public bool VolumeControl { get; set; }

		public int InputCount { get; set; }

		#endregion

		#region Methods

		/// <summary>
		/// Updates the settings from xml.
		/// </summary>
		/// <param name="xml"></param>
		public override void ParseXml(string xml)
		{
			base.ParseXml(xml);

			PowerControl = XmlUtils.TryReadChildElementContentAsBoolean(xml, POWER_CONTROL_ELEMENT) ?? false;
			VolumeControl = XmlUtils.TryReadChildElementContentAsBoolean(xml, VOLUME_CONTROL_ELEMENT) ?? false;
			InputCount = XmlUtils.TryReadChildElementContentAsInt(xml, INPUT_COUNT_ELEMENT) ?? 1;
		}

		/// <summary>
		/// Writes property elements to xml.
		/// </summary>
		/// <param name="writer"></param>
		protected override void WriteElements(IcdXmlTextWriter writer)
		{
			base.WriteElements(writer);

			writer.WriteElementString(POWER_CONTROL_ELEMENT, IcdXmlConvert.ToString(PowerControl));
			writer.WriteElementString(VOLUME_CONTROL_ELEMENT, IcdXmlConvert.ToString(VolumeControl));
			writer.WriteElementString(INPUT_COUNT_ELEMENT, IcdXmlConvert.ToString(InputCount));
		}

		#endregion


	}
}