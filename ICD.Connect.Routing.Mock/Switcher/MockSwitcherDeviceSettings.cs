using ICD.Common.Properties;
using ICD.Connect.Settings;
using ICD.Connect.Settings.Attributes.Factories;
using ICD.Connect.Settings.Core;

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

		#endregion

		#region Methods

		/// <summary>
		/// Creates a new originator instance from the settings.
		/// </summary>
		/// <param name="factory"></param>
		/// <returns></returns>
		public override IOriginator ToOriginator(IDeviceFactory factory)
		{
			MockSwitcherDevice output = new MockSwitcherDevice();
			output.ApplySettings(this, factory);

			return output;
		}

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
