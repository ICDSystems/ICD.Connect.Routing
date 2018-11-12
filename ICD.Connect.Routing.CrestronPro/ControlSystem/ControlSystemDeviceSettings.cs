using ICD.Common.Utils.Xml;
using ICD.Connect.Devices;
using ICD.Connect.Settings.Attributes;

namespace ICD.Connect.Routing.CrestronPro.ControlSystem
{
	[KrangSettings("ControlSystem", typeof(ControlSystemDevice))]
	public sealed class ControlSystemDeviceSettings : AbstractDeviceSettings
	{
		private const string ELEMENT_CONFIG = "Config";

		public string Config { get; set; }

		/// <summary>
		/// Writes property elements to xml.
		/// </summary>
		/// <param name="writer"></param>
		protected override void WriteElements(IcdXmlTextWriter writer)
		{
			base.WriteElements(writer);

			writer.WriteElementString(ELEMENT_CONFIG, Config);
		}

		/// <summary>
		/// Updates the settings from xml.
		/// </summary>
		/// <param name="xml"></param>
		public override void ParseXml(string xml)
		{
			base.ParseXml(xml);

			Config = XmlUtils.TryReadChildElementContentAsString(xml, ELEMENT_CONFIG);
		}
	}
}
