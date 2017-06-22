using ICD.Common.Properties;
using ICD.Connect.Settings;
using ICD.Connect.Settings.Attributes.Factories;
using ICD.Connect.Settings.Core;

namespace ICD.Connect.Routing.Endpoints.Groups
{
	public sealed class DestinationGroupSettings : AbstractDestinationGroupSettings
	{
		private const string FACTORY_NAME = "DestinationGroup";

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
			DestinationGroup destinationGroup = new DestinationGroup();
			destinationGroup.ApplySettings(this, factory);
			return destinationGroup;
		}

		/// <summary>
		/// Loads the settings from XML.
		/// </summary>
		/// <param name="xml"></param>
		/// <returns></returns>
		[PublicAPI, XmlDestinationGroupSettingsFactoryMethod(FACTORY_NAME)]
		public static DestinationGroupSettings FromXml(string xml)
		{
			DestinationGroupSettings output = new DestinationGroupSettings();
			ParseXml(output, xml);
			return output;
		}
	}
}
