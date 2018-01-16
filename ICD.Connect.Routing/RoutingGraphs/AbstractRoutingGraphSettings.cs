﻿using System;
using System.Collections.Generic;
using ICD.Common.Utils.Xml;
using ICD.Connect.Settings;

namespace ICD.Connect.Routing.RoutingGraphs
{
	public abstract class AbstractRoutingGraphSettings : AbstractSettings, IRoutingGraphSettings
	{
		private const string ELEMENT_NAME = "Routing";

		private const string CONNECTIONS_ELEMENT = "Connections";
		private const string STATIC_ROUTES_ELEMENT = "StaticRoutes";
		private const string SOURCES_ELEMENT = "Sources";
		private const string DESTINATIONS_ELEMENT = "Destinations";
		private const string DESTINATION_GROUPS_ELEMENT = "DestinationGroups";

		private readonly SettingsCollection m_ConnectionSettings;
		private readonly SettingsCollection m_StaticRouteSettings;
		private readonly SettingsCollection m_SourceSettings;
		private readonly SettingsCollection m_DestinationSettings;
		private readonly SettingsCollection m_DestinationGroupSettings;

		#region Properties

		protected override string Element { get { return ELEMENT_NAME; } }
		public SettingsCollection ConnectionSettings { get { return m_ConnectionSettings; } }
		public SettingsCollection StaticRouteSettings { get { return m_StaticRouteSettings; } }
		public SettingsCollection SourceSettings { get { return m_SourceSettings; } }
		public SettingsCollection DestinationSettings { get { return m_DestinationSettings; } }
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

			m_ConnectionSettings.ToXml(writer, CONNECTIONS_ELEMENT);
			m_StaticRouteSettings.ToXml(writer, STATIC_ROUTES_ELEMENT);
			m_SourceSettings.ToXml(writer, SOURCES_ELEMENT);
			m_DestinationSettings.ToXml(writer, DESTINATIONS_ELEMENT);
			m_DestinationGroupSettings.ToXml(writer, DESTINATION_GROUPS_ELEMENT);
		}

		protected static void ParseXml(AbstractRoutingGraphSettings instance, string xml)
		{
			if (instance == null)
				throw new ArgumentNullException("instance");

			IEnumerable<ISettings> connections = PluginFactory.GetSettingsFromXml(xml, CONNECTIONS_ELEMENT);
			IEnumerable<ISettings> staticRoutes = PluginFactory.GetSettingsFromXml(xml, STATIC_ROUTES_ELEMENT);
			IEnumerable<ISettings> sources = PluginFactory.GetSettingsFromXml(xml, SOURCES_ELEMENT);
			IEnumerable<ISettings> destinations = PluginFactory.GetSettingsFromXml(xml, DESTINATIONS_ELEMENT);
			IEnumerable<ISettings> destinationGroups = PluginFactory.GetSettingsFromXml(xml, DESTINATION_GROUPS_ELEMENT);

			instance.ConnectionSettings.SetRange(connections);
			instance.StaticRouteSettings.SetRange(staticRoutes);
			instance.SourceSettings.SetRange(sources);
			instance.DestinationSettings.SetRange(destinations);
			instance.DestinationGroupSettings.SetRange(destinationGroups);

			AbstractSettings.ParseXml(instance, xml);
		}

		#endregion
	}
}
