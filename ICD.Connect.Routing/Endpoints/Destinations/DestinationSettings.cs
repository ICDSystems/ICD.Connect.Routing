using ICD.Common.Properties;
using ICD.Connect.Settings;
using ICD.Connect.Settings.Attributes.Factories;
using ICD.Connect.Settings.Core;

namespace ICD.Connect.Routing.Endpoints.Destinations
{
	public sealed class DestinationSettings : AbstractDestinationSettings
	{
		private const string FACTORY_NAME = "Destination";

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
			Destination destination = new Destination();
			destination.ApplySettings(this, factory);
			return destination;
		}

		/// <summary>
		/// Loads the settings from XML.
		/// </summary>
		/// <param name="xml"></param>
		/// <returns></returns>
		[PublicAPI, XmlDestinationSettingsFactoryMethod(FACTORY_NAME)]
		public static DestinationSettings FromXml(string xml)
		{
			DestinationSettings output = new DestinationSettings();
			ParseXml(output, xml);
			return output;
		}
	}
}
