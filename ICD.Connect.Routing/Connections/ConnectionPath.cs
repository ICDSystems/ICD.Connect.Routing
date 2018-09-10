using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using ICD.Common.Utils;
using ICD.Connect.Routing.Endpoints;
using ICD.Connect.Settings;

namespace ICD.Connect.Routing.Connections
{
	/// <summary>
	/// Describes a contiguous path through a series of connections.
	/// </summary>
	public sealed class ConnectionPath : IEnumerable<Connection>
	{
		private readonly Connection[] m_Connections;
		private readonly eConnectionType m_Type;

		#region Properties

		/// <summary>
		/// Gets the source endpoint info for the path.
		/// </summary>
		public EndpointInfo SourceEndpoint
		{
			get
			{
				if (m_Connections.Length == 0)
					throw new InvalidOperationException("Path is empty.");

				return m_Connections[0].Source;
			}
		}

		/// <summary>
		/// Gets the destination endpoint info for the path.
		/// </summary>
		public EndpointInfo DestinationEndpoint
		{
			get
			{
				if (m_Connections.Length == 0)
					throw new InvalidOperationException("Path is empty.");

				return m_Connections[m_Connections.Length - 1].Destination;
			}
		}

		/// <summary>
		/// Gets the connection type associated with this path.
		/// </summary>
		public eConnectionType ConnectionType { get { return m_Type; } }

		#endregion

		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="connections"></param>
		/// <param name="type"></param>
		public ConnectionPath(IEnumerable<Connection> connections, eConnectionType type)
		{
			if (connections == null)
				throw new ArgumentNullException("connections");

			m_Connections = connections.ToArray();
			m_Type = type;
		}

		#region Methods

		/// <summary>
		/// Gets the string representation for this instance.
		/// </summary>
		/// <returns></returns>
		public override string ToString()
		{
			ReprBuilder builder = new ReprBuilder(this);

			builder.AppendProperty("Type", m_Type);
			builder.AppendProperty("Connections", StringUtils.ArrayFormat(this.Select(c => c.ToStringShorthand())));

			return builder.ToString();
		}

		#endregion

		public IEnumerator<Connection> GetEnumerator()
		{
			return (m_Connections as IList<Connection>).GetEnumerator();
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return GetEnumerator();
		}
	}
}
