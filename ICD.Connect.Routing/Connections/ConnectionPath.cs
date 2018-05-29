using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using ICD.Common.Utils;
using ICD.Common.Utils.Collections;
using ICD.Connect.Routing.Endpoints;
using ICD.Connect.Settings;

namespace ICD.Connect.Routing.Connections
{
	/// <summary>
	/// Describes a contiguous path through a series of connections.
	/// </summary>
	public sealed class ConnectionPath : IEnumerable<Connection>
	{
		private readonly IcdHashSet<Connection> m_Contains;
		private readonly List<Connection> m_Ordered;
		private readonly SafeCriticalSection m_Section;
		private readonly eConnectionType m_Type;

		#region Properties

		/// <summary>
		/// Gets the number of connections in the path.
		/// </summary>
		public int Count { get { return m_Section.Execute(() => m_Ordered.Count); } }

		/// <summary>
		/// Gets the source endpoint info for the path.
		/// </summary>
		public EndpointInfo SourceEndpoint
		{
			get
			{
				m_Section.Enter();

				try
				{
					if (m_Ordered.Count == 0)
						throw new InvalidOperationException("Path is empty.");

					return m_Ordered[0].Source;
				}
				finally
				{
					m_Section.Leave();
				}
			}
		}

		/// <summary>
		/// Gets the destination endpoint info for the path.
		/// </summary>
		public EndpointInfo DestinationEndpoint
		{
			get
			{
				m_Section.Enter();

				try
				{
					if (m_Ordered.Count == 0)
						throw new InvalidOperationException("Path is empty.");

					return m_Ordered[m_Ordered.Count - 1].Destination;
				}
				finally
				{
					m_Section.Leave();
				}
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
		/// <param name="type"></param>
		public ConnectionPath(eConnectionType type)
			: this(Enumerable.Empty<Connection>(), type)
		{
		}

		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="connections"></param>
		/// <param name="type"></param>
		public ConnectionPath(IEnumerable<Connection> connections, eConnectionType type)
		{
			m_Contains = new IcdHashSet<Connection>();
			m_Ordered = new List<Connection>();
			m_Section = new SafeCriticalSection();
			m_Type = type;

			if (connections.Any(item => !Add(item)))
				throw new ArgumentException("Given path is not contiguous", "connections");
		}

		#region Methods

		/// <summary>
		/// Adds the given item to the collection.
		/// </summary>
		/// <param name="item"></param>
		/// <returns>False if the item already exists, or the new item would break the path.</returns>
		public bool Add(Connection item)
		{
			if (item == null)
				throw new ArgumentNullException("item");

			m_Section.Enter();

			try
			{
				if (m_Contains.Contains(item))
					return false;

				// Is this adjacent to the last item?
				if (m_Ordered.Count != 0)
				{
					EndpointInfo destination = m_Ordered[m_Ordered.Count - 1].Destination;
					if (!destination.EqualsControl(item.Source))
						return false;
				}

				m_Ordered.Add(item);
				m_Contains.Add(item);

				return true;
			}
			finally
			{
				m_Section.Leave();
			}
		}

		/// <summary>
		/// Removes the given item from the collection.
		/// </summary>
		/// <param name="item"></param>
		/// <returns>False if the item is missing from the collection, or removing the item would break the path.</returns>
		public bool Remove(Connection item)
		{
			if (item == null)
				throw new ArgumentNullException("item");

			m_Section.Enter();

			try
			{
				if (!m_Contains.Contains(item))
					return false;

				// Prevent removing an item in the middle of the path
				if (item != m_Ordered[0] && item != m_Ordered[m_Ordered.Count - 1])
					return false;

				m_Contains.Remove(item);
				m_Ordered.Remove(item);

				return true;
			}
			finally
			{
				m_Section.Leave();
			}
		}

		/// <summary>
		/// Removes all connections from the path.
		/// </summary>
		public void Clear()
		{
			m_Section.Enter();

			try
			{
				m_Contains.Clear();
				m_Ordered.Clear();
			}
			finally
			{
				m_Section.Leave();
			}
		}

		/// <summary>
		/// Returns true if the path contains the given connection.
		/// </summary>
		/// <param name="item"></param>
		/// <returns></returns>
		public bool Contains(Connection item)
		{
			if (item == null)
				throw new ArgumentNullException("item");

			return m_Section.Execute(() => m_Contains.Contains(item));
		}

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
			m_Section.Enter();

			try
			{
				// Copy the wrapped collection
				return m_Ordered.ToList()
				                .GetEnumerator();
			}
			finally
			{
				m_Section.Leave();
			}
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return GetEnumerator();
		}
	}
}
