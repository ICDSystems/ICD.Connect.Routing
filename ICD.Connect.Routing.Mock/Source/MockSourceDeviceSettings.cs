using System;
using ICD.Common.Utils.Xml;
using ICD.Connect.Devices;
using ICD.Connect.Settings.Attributes;

namespace ICD.Connect.Routing.Mock.Source
{
	/// <summary>
	/// Settings for the MockSourceDevice.
	/// </summary>
	[KrangSettings(FACTORY_NAME)]
	public sealed class MockSourceDeviceSettings : AbstractDeviceSettings
	{
		private const string FACTORY_NAME = "MockSourceDevice";

		private const string OUTPUT_COUNT_ELEMENT = "OutputCount";

		/// <summary>
		/// Gets the originator factory name.
		/// </summary>
		public override string FactoryName { get { return FACTORY_NAME; } }

		/// <summary>
		/// Gets the type of the originator for this settings instance.
		/// </summary>
		public override Type OriginatorType { get { return typeof(MockSourceDevice); } }

		public int OutputCount { get; set; }

		/// <summary>
		/// Writes property elements to xml.
		/// </summary>
		/// <param name="writer"></param>
		protected override void WriteElements(IcdXmlTextWriter writer)
		{
			base.WriteElements(writer);

			writer.WriteElementString(OUTPUT_COUNT_ELEMENT, IcdXmlConvert.ToString(OutputCount));
		}

		/// <summary>
		/// Updates the settings from xml.
		/// </summary>
		/// <param name="xml"></param>
		public override void ParseXml(string xml)
		{
			base.ParseXml(xml);

			OutputCount = XmlUtils.TryReadChildElementContentAsInt(xml, OUTPUT_COUNT_ELEMENT) ?? 1;
		}
	}
}
