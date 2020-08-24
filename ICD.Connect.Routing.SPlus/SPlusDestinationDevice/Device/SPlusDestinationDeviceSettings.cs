using ICD.Common.Utils.Xml;
using ICD.Connect.Devices.CrestronSPlus.Devices.SPlus;
using ICD.Connect.Settings.Attributes;

namespace ICD.Connect.Routing.SPlus.SPlusDestinationDevice.Device
{
	[KrangSettings("SPlusDestinationDevice", typeof(SPlusDestinationDevice))]
	public sealed class SPlusDestinationDeviceSettings : AbstractSPlusDeviceSettings
	{

		#region Consts

		private const string POWER_CONTROL_ELEMENT = "PowerControl";
		private const string VOLUME_CONTROL_ELEMENT = "VolumeControl";
		private const string INPUT_COUNT_ELEMENT = "InputCount";
		private const string VOLUME_MIN_ELEMENT = "VolumeMin";
		private const string VOLUME_MAX_ELEMENT = "VolumeMax";

		#endregion

		#region Properties

		public bool PowerControl { get; set; }

		public bool VolumeControl { get; set; }

		public int InputCount { get; set; }

		public ushort? VolumeMin { get; set; }

		public ushort? VolumeMax { get; set; }

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
			VolumeMin = XmlUtils.TryReadChildElementContentAsUShort(xml, VOLUME_MIN_ELEMENT);
			VolumeMax = XmlUtils.TryReadChildElementContentAsUShort(xml, VOLUME_MAX_ELEMENT);



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
			writer.WriteElementString(VOLUME_MIN_ELEMENT, IcdXmlConvert.ToString(VolumeMin));
			writer.WriteElementString(VOLUME_MAX_ELEMENT, IcdXmlConvert.ToString(VolumeMax));
		}

		#endregion
	}
}