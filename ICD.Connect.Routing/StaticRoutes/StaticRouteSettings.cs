using System.Collections.Generic;
using System.Linq;
using ICD.Common.Utils;
using ICD.Common.Utils.Collections;
using ICD.Common.Utils.Extensions;
using ICD.Common.Utils.Xml;
using ICD.Connect.Routing.Connections;
using ICD.Connect.Settings;
using ICD.Connect.Settings.Attributes;

namespace ICD.Connect.Routing.StaticRoutes
{
	[KrangSettings("StaticRoute", typeof(StaticRoute))]
	public sealed class StaticRouteSettings : AbstractSettings
	{
		private const string STATIC_ROUTE_ELEMENT = "StaticRoute";

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

			writer.WriteElementString(CONNECTION_TYPE_ELEMENT, IcdXmlConvert.ToString(ConnectionType));

			XmlUtils.WriteListToXml(writer, m_Connections, CONNECTIONS_ELEMENT, CONNECTION_ELEMENT);
		}

		/// <summary>
		/// Updates the settings from xml.
		/// </summary>
		/// <param name="xml"></param>
		public override void ParseXml(string xml)
		{
			base.ParseXml(xml);

			ConnectionType =
				XmlUtils.TryReadChildElementContentAsEnum<eConnectionType>(xml, CONNECTION_TYPE_ELEMENT, true) ??
				eConnectionType.Audio | eConnectionType.Video;

			IEnumerable<int> connections = XmlUtils.ReadListFromXml(xml, CONNECTIONS_ELEMENT, CONNECTION_ELEMENT,
			                                                        x => XmlUtils.ReadElementContentAsInt(x));

			SetConnections(connections);
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
		public override int DependencyCount { get { return m_Connections.Count; } }
	}
}
