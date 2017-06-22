using ICD.Common.Properties;
using ICD.Connect.Settings;
using ICD.Connect.Settings.Attributes.Factories;
using ICD.Connect.Settings.Core;

namespace ICD.Connect.Routing.SPlus
{
	public class SPlusSwitcherSettings : AbstractDeviceSettings
	{
		private const string FACTORY_NAME = "SPlusSwitcher";

		public override string FactoryName { get { return FACTORY_NAME; } }

		public override IOriginator ToOriginator(IDeviceFactory factory)
		{
			SPlusSwitcher switcher = new SPlusSwitcher();
			switcher.ApplySettings(this, factory);
			return switcher;
		}

		/// <summary>
		/// Loads the settings from XML.
		/// </summary>
		/// <param name="xml"></param>
		/// <returns></returns>
		[PublicAPI, XmlDeviceSettingsFactoryMethod(FACTORY_NAME)]
		public static SPlusSwitcherSettings FromXml(string xml)
		{
			SPlusSwitcherSettings output = new SPlusSwitcherSettings();

			ParseXml(output, xml);
			return output;
		}
	}
}
