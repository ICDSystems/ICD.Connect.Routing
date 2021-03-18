using System.Collections.Generic;
using System.Linq;
using ICD.Common.Utils.Services.Logging;
using ICD.Common.Utils.Xml;
using ICD.Connect.Settings;

namespace ICD.Connect.Routing.RoutingGraphs
{
	public abstract class AbstractRoutingGraphSettings : AbstractSettings, IRoutingGraphSettings
	{
		private const string CONNECTIONS_ELEMENT = "Connections";
		private const string STATIC_ROUTES_ELEMENT = "StaticRoutes";
		private const string SOURCES_ELEMENT = "Sources";
		private const string DESTINATIONS_ELEMENT = "Destinations";
		private const string SOURCE_GROUPS_ELEMENT = "SourceGroups";
		private const string DESTINATION_GROUPS_ELEMENT = "DestinationGroups";

		private const string CONNECTION_ELEMENT = "Connection";
		private const string STATIC_ROUTE_ELEMENT = "StaticRoute";
		private const string SOURCE_ELEMENT = "Source";
		private const string DESTINATION_ELEMENT = "Destination";
		private const string SOURCE_GROUP_ELEMENT = "SourceGroup";
		private const string DESTINATION_GROUP_ELEMENT = "DestinationGroup";

		private readonly SettingsCollection m_ConnectionSettings;
		private readonly SettingsCollection m_StaticRouteSettings;
		private readonly SettingsCollection m_SourceSettings;
		private readonly SettingsCollection m_DestinationSettings;
		private readonly SettingsCollection m_SourceGroupSettings;
		private readonly SettingsCollection m_DestinationGroupSettings;

		#region Properties

		public SettingsCollection ConnectionSettings { get { return m_ConnectionSettings; } }
		public SettingsCollection StaticRouteSettings { get { return m_StaticRouteSettings; } }
		public SettingsCollection SourceSettings { get { return m_SourceSettings; } }
		public SettingsCollection DestinationSettings { get { return m_DestinationSettings; } }
		public SettingsCollection SourceGroupSettings { get { return m_SourceGroupSettings; } }
		public SettingsCollection DestinationGroupSettings { get { return m_DestinationGroupSettings; } }

		#endregion

		/// <summary>
		/// Constructor.
		/// </summary>
		protected AbstractRoutingGraphSettings()
		{
			m_ConnectionSettings = new SettingsCollection();
			m_StaticRouteSettings = new SettingsCollection();
			m_SourceSettings = new SettingsCollection();
			m_DestinationSettings = new SettingsCollection();
			m_SourceGroupSettings = new SettingsCollection();
			m_DestinationGroupSettings = new SettingsCollection();
		}

		#region Methods

		/// <summary>
		/// Writes the routing settings to xml.
		/// </summary>
		/// <param name="writer"></param>
		protected override void WriteElements(IcdXmlTextWriter writer)
		{
			base.WriteElements(writer);

			m_ConnectionSettings.ToXml(writer, CONNECTIONS_ELEMENT, CONNECTION_ELEMENT);
			m_StaticRouteSettings.ToXml(writer, STATIC_ROUTES_ELEMENT, STATIC_ROUTE_ELEMENT);
			m_SourceSettings.ToXml(writer, SOURCES_ELEMENT, SOURCE_ELEMENT);
			m_DestinationSettings.ToXml(writer, DESTINATIONS_ELEMENT, DESTINATION_ELEMENT);
			m_SourceGroupSettings.ToXml(writer, SOURCE_GROUPS_ELEMENT, SOURCE_GROUP_ELEMENT);
			m_DestinationGroupSettings.ToXml(writer, DESTINATION_GROUPS_ELEMENT, DESTINATION_GROUP_ELEMENT);
		}

		/// <summary>
		/// Updates the settings from xml.
		/// </summary>
		/// <param name="xml"></param>
		public override void ParseXml(string xml)
		{
			base.ParseXml(xml);

			IEnumerable<ISettings> connections = PluginFactory.GetSettingsFromXml(xml, CONNECTIONS_ELEMENT);
			IEnumerable<ISettings> staticRoutes = PluginFactory.GetSettingsFromXml(xml, STATIC_ROUTES_ELEMENT);
			IEnumerable<ISettings> sources = PluginFactory.GetSettingsFromXml(xml, SOURCES_ELEMENT);
			IEnumerable<ISettings> destinations = PluginFactory.GetSettingsFromXml(xml, DESTINATIONS_ELEMENT);
			IEnumerable<ISettings> sourceGroups = PluginFactory.GetSettingsFromXml(xml, SOURCE_GROUPS_ELEMENT);
			IEnumerable<ISettings> destinationGroups = PluginFactory.GetSettingsFromXml(xml, DESTINATION_GROUPS_ELEMENT);

			AddSettingsLogDuplicates(ConnectionSettings, connections);
			AddSettingsLogDuplicates(StaticRouteSettings, staticRoutes);
			AddSettingsLogDuplicates(SourceSettings, sources);
			AddSettingsLogDuplicates(DestinationSettings, destinations);
			AddSettingsLogDuplicates(SourceGroupSettings, sourceGroups);
			AddSettingsLogDuplicates(DestinationGroupSettings, destinationGroups);
		}

		private void AddSettingsLogDuplicates(SettingsCollection collection, IEnumerable<ISettings> settings)
		{
			foreach (ISettings item in settings.Where(item => !collection.Add(item)))
				Logger.AddEntry(eSeverity.Error, "{0} failed to add duplicate {1}", GetType().Name, item);
		}

		#endregion
	}
}
