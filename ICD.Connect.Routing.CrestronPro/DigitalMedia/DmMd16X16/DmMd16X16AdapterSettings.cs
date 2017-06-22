using ICD.Common.Properties;
using ICD.Connect.Routing.CrestronPro.DigitalMedia.DmMdNXN;
using ICD.Connect.Settings;
using ICD.Connect.Settings.Attributes.Factories;
using ICD.Connect.Settings.Core;

namespace ICD.Connect.Routing.CrestronPro.DigitalMedia.DmMd16X16
{
	public sealed class DmMd16X16AdapterSettings : AbstractDmMdMNXNAdapterSettings
	{
		private const string FACTORY_NAME = "DmMd16x16";

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
			DmMd16X16Adapter output = new DmMd16X16Adapter();
			output.ApplySettings(this, factory);
			return output;
		}

		/// <summary>
		/// Loads the settings from XML.
		/// </summary>
		/// <param name="xml"></param>
		/// <returns></returns>
		[PublicAPI, XmlDeviceSettingsFactoryMethod(FACTORY_NAME)]
		public static DmMd16X16AdapterSettings FromXml(string xml)
		{
			DmMd16X16AdapterSettings output = new DmMd16X16AdapterSettings();
			ParseXml(output, xml);
			return output;
		}
	}
}