using ICD.Common.Properties;
using ICD.Connect.Settings;
using ICD.Connect.Settings.Attributes.Factories;
using ICD.Connect.Settings.Core;

namespace ICD.Connect.Routing.CrestronPro.ControlSystem
{
	public sealed class ControlSystemDeviceSettings : AbstractDeviceSettings
	{
		private const string FACTORY_NAME = "ControlSystem";

		/// <summary>
		/// Gets the originator factory name.
		/// </summary>
		public override string FactoryName { get { return FACTORY_NAME; } }

		/// <summary>
		/// Creates a new originator instance from the settings.
		/// </summary>
		/// <param name="factory"></param>
		/// <returns></returns>
		public override IOriginator ToOriginator(IDeviceFactory factory)
		{
			ControlSystemDevice output = new ControlSystemDevice();
			output.ApplySettings(this, factory);
			return output;
		}

		/// <summary>
		/// Loads the settings from XML.
		/// </summary>
		/// <param name="xml"></param>
		/// <returns></returns>
		[PublicAPI, XmlDeviceSettingsFactoryMethod(FACTORY_NAME)]
		public static ControlSystemDeviceSettings FromXml(string xml)
		{
			ControlSystemDeviceSettings output = new ControlSystemDeviceSettings();
			ParseXml(output, xml);
			return output;
		}
	}
}
