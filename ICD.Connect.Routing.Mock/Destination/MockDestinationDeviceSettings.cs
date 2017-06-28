using System;
using ICD.Common.Properties;
using ICD.Connect.Settings;
using ICD.Connect.Settings.Attributes.Factories;

namespace ICD.Connect.Routing.Mock.Destination
{
	/// <summary>
	/// Settings for the MockDestinationDevice.
	/// </summary>
	public sealed class MockDestinationDeviceSettings : AbstractDeviceSettings
	{
		private const string FACTORY_NAME = "MockDestinationDevice";

		/// <summary>
		/// Gets the originator factory name.
		/// </summary>
		public override string FactoryName { get { return FACTORY_NAME; } }

		/// <summary>
		/// Gets the type of the originator for this settings instance.
		/// </summary>
		public override Type OriginatorType { get { return typeof(MockDestinationDevice); } }

		/// <summary>
		/// Loads the settings from XML.
		/// </summary>
		/// <param name="xml"></param>
		/// <returns></returns>
		[PublicAPI, XmlDeviceSettingsFactoryMethod(FACTORY_NAME)]
		public static MockDestinationDeviceSettings FromXml(string xml)
		{
			MockDestinationDeviceSettings output = new MockDestinationDeviceSettings();
			ParseXml(output, xml);
			return output;
		}
	}
}
