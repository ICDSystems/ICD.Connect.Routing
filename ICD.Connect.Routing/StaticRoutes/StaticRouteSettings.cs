using System;
using System.Collections.Generic;
using System.Linq;
using ICD.Common.Properties;
using ICD.Common.Utils;
using ICD.Common.Utils.Collections;
using ICD.Common.Utils.Extensions;
using ICD.Common.Utils.Xml;
using ICD.Connect.Routing.Connections;
using ICD.Connect.Settings;
using ICD.Connect.Settings.Attributes;

namespace ICD.Connect.Routing.StaticRoutes
{
	public sealed class StaticRouteSettings : AbstractSettings
	{
		private const string STATIC_ROUTE_ELEMENT = "StaticRoute";
		private const string FACTORY_NAME = "StaticRoute";

		private const string CONNECTIONS_ELEMENT = "Connections";
		private const string CONNECTION_ELEMENT = "Connection";

		private const string CONNECTION_TYPE_ELEMENT = "ConnectionType";

		private readonly IcdHashSet<int> m_Connections;
		private readonly SafeCriticalSection m_ConnectionsSection;

		#region Properties

		/// <summary>
		/// Gets the xml element.
		/// </summary>
		protected override string Element { get { return STATIC_ROUTE_ELEMENT; } }

		/// <summary>
		/// Gets the originator factory name.
		/// </summary>
		public override string FactoryName { get { return FACTORY_NAME; } }

		/// <summary>
		/// Gets the type of the originator for this settings instance.
		/// </summary>
		public override Type OriginatorType { get { return typeof(StaticRoute); } }

		[SettingsProperty(SettingsProperty.ePropertyType.Enum)]
		public eConnectionType ConnectionType { get; set; }

		#endregion

		/// <summary>
		/// Constructor.
		/// </summary>
		public StaticRouteSettings()
		{
			m_Connections = new IcdHashSet<int>();
			m_ConnectionsSection = new SafeCriticalSection();
		}

		public void SetConnections(IEnumerable<int> connections)
		{
			m_ConnectionsSection.Enter();

			try
			{
				m_Connections.Clear();
				m_Connections.AddRange(connections);
			}
			finally
			{
				m_ConnectionsSection.Leave();
			}
		}

		public IEnumerable<int> GetConnections()
		{
			return m_ConnectionsSection.Execute(() => m_Connections.Order().ToArray());
		}

		/// <summary>
		/// Writes property elements to xml.
		/// </summary>
		/// <param name="writer"></param>
		protected override void WriteElements(IcdXmlTextWriter writer)
		{
			base.WriteElements(writer);

			writer.WriteStartElement(CONNECTIONS_ELEMENT);
			{
				m_ConnectionsSection.Enter();

				try
				{
					foreach (int connection in m_Connections)
						writer.WriteElementString(CONNECTION_ELEMENT, IcdXmlConvert.ToString(connection));
				}
				finally
				{
					m_ConnectionsSection.Leave();
				}
			}
			writer.WriteEndElement();
		}

        /// <summary>
        /// Returns true if the settings depend on a device with the given ID.
        /// For example, to instantiate an IR Port from settings, the device the physical port
        /// belongs to will need to be instantiated first.
        /// </summary>
        /// <returns></returns>
        public override bool HasDeviceDependency(int id)
        {
            return m_Connections.Contains(id);
        }

        /// <summary>
        /// Returns the count from the collection of ids that the settings depends on.
        /// </summary>
        public override int DependencyCount
        {
            get { return m_Connections.Count; }
        }

		/// <summary>
		/// Instantiates Connection settings from an xml element.
		/// </summary>
		/// <param name="xml"></param>
		/// <returns></returns>
		[PublicAPI, XmlFactoryMethod(FACTORY_NAME)]
		public static StaticRouteSettings FromXml(string xml)
		{
			string connectionsElement = XmlUtils.GetChildElementAsString(xml, CONNECTIONS_ELEMENT);
			IEnumerable<int> connections = XmlUtils.GetChildElementsAsString(connectionsElement, CONNECTION_ELEMENT)
			                                       .Select(x => XmlUtils.ReadElementContentAsInt(x));

			eConnectionType connectionType =
				XmlUtils.ReadChildElementContentAsEnum<eConnectionType>(xml, CONNECTION_TYPE_ELEMENT, true);

			StaticRouteSettings output = new StaticRouteSettings
			{
				ConnectionType = connectionType
			};

			output.SetConnections(connections);

			ParseXml(output, xml);
			return output;
		}
	}
}
