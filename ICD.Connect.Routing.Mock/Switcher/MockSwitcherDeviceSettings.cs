using System;
using ICD.Common.Properties;
using ICD.Connect.Devices;
using ICD.Connect.Settings.Attributes.Factories;

namespace ICD.Connect.Routing.Mock.Switcher
{
	public sealed class MockSwitcherDeviceSettings : AbstractDeviceSettings
	{
		private const string FACTORY_NAME = "MockSwitcherDevice";

		#region Properties

		/// <summary>
		/// Gets the originator factory name.
		/// </summary>
		public override string FactoryName { get { return FACTORY_NAME; } }

		/// <summary>
		/// Gets the type of the originator for this settings instance.
		/// </summary>
		public override Type OriginatorType { get { return typeof(MockSwitcherDevice); } }

		#endregion

		#region Methods

		/// <summary>
		/// Loads the settings from XML.
		/// </summary>
		/// <param name="xml"></param>
		/// <returns></returns>
		[PublicAPI, XmlDeviceSettingsFactoryMethod(FACTORY_NAME)]
		public static MockSwitcherDeviceSettings FromXml(string xml)
		{
			MockSwitcherDeviceSettings output = new MockSwitcherDeviceSettings();

			ParseXml(output, xml);
			return output;
		}

		#endregion
	}
}
