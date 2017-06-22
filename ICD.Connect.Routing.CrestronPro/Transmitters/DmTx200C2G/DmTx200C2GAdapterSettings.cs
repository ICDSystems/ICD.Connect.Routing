using ICD.Common.Properties;
using ICD.Connect.Routing.CrestronPro.Transmitters.DmTx200Base;
using ICD.Connect.Settings;
using ICD.Connect.Settings.Attributes.Factories;
using ICD.Connect.Settings.Core;

namespace ICD.Connect.Routing.CrestronPro.Transmitters.DmTx200C2G
{
	public sealed class DmTx200C2GAdapterSettings : AbstractDmTx200BaseAdapterSettings
	{
		private const string FACTORY_NAME = "DmTx200C2G";

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
			DmTx200C2GAdapter output = new DmTx200C2GAdapter();
			output.ApplySettings(this, factory);

			return output;
		}

		/// <summary>
		/// Loads the settings from XML.
		/// </summary>
		/// <param name="xml"></param>
		/// <returns></returns>
		[PublicAPI, XmlDeviceSettingsFactoryMethod(FACTORY_NAME)]
		public static DmTx200C2GAdapterSettings FromXml(string xml)
		{
			DmTx200C2GAdapterSettings output = new DmTx200C2GAdapterSettings();
			ParseXml(output, xml);
			return output;
		}
	}
}
