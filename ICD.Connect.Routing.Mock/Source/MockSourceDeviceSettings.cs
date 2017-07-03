﻿using System;
using ICD.Common.Properties;
using ICD.Common.Utils.Xml;
using ICD.Connect.Settings;
using ICD.Connect.Settings.Attributes.Factories;

namespace ICD.Connect.Routing.Mock.Source
{
	/// <summary>
	/// Settings for the MockSourceDevice.
	/// </summary>
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
		/// Loads the settings from XML.
		/// </summary>
		/// <param name="xml"></param>
		/// <returns></returns>
		[PublicAPI, XmlDeviceSettingsFactoryMethod(FACTORY_NAME)]
		public static MockSourceDeviceSettings FromXml(string xml)
		{
			int? outputCount = XmlUtils.TryReadChildElementContentAsInt(xml, OUTPUT_COUNT_ELEMENT);

			MockSourceDeviceSettings output = new MockSourceDeviceSettings
			{
				OutputCount = outputCount ?? 1
			};

			ParseXml(output, xml);
			return output;
		}
	}
}
