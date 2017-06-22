using ICD.Common.Properties;
using ICD.Connect.Routing.CrestronPro.DigitalMedia.DmMdNXN;
using ICD.Connect.Settings;
using ICD.Connect.Settings.Attributes.Factories;
using ICD.Connect.Settings.Core;

namespace ICD.Connect.Routing.CrestronPro.DigitalMedia.DmMd8X8
{
	public sealed class DmMd8X8AdapterSettings : AbstractDmMdMNXNAdapterSettings
	{
		private const string FACTORY_NAME = "DmMd8x8";

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
			DmMd8X8Adapter output = new DmMd8X8Adapter();
			output.ApplySettings(this, factory);
			return output;
		}

		/// <summary>
		/// Loads the settings from XML.
		/// </summary>
		/// <param name="xml"></param>
		/// <returns></returns>
		[PublicAPI, XmlDeviceSettingsFactoryMethod(FACTORY_NAME)]
		public static DmMd8X8AdapterSettings FromXml(string xml)
		{
			DmMd8X8AdapterSettings output = new DmMd8X8AdapterSettings();
			ParseXml(output, xml);
			return output;
		}
	}
}
