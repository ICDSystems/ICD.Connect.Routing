using System;
using System.Collections.Generic;
using System.Linq;
using ICD.Common.Utils.Extensions;
using ICD.Connect.Routing.Connections;
using ICD.Connect.Routing.Endpoints;

namespace ICD.Connect.Routing.Pathfinding
{
	/// <summary>
	/// Describes a single search for a path.
	/// </summary>
	public sealed class PathBuilderQuery
	{
		private readonly List<EndpointInfo> m_SourceEndpoints;
		private readonly List<EndpointInfo[]> m_DestinationEndpoints;

		/// <summary>
		/// Sets the connection type for the query.
		/// </summary>
		/// <value></value>
		public eConnectionType Type { get; set; }

		/// <summary>
		/// Constructor.
		/// </summary>
		public PathBuilderQuery()
		{
			m_SourceEndpoints = new List<EndpointInfo>();
			m_DestinationEndpoints = new List<EndpointInfo[]>();
		}

		/// <summary>
		/// Gets the ordered source endpoints for the query.
		/// </summary>
		/// <returns></returns>
		public IEnumerable<EndpointInfo> GetStart()
		{
			return m_SourceEndpoints;
		}

		/// <summary>
		/// Sets the source endpoints for the query.
		/// </summary>
		/// <param name="sourceEndpoints"></param>
		public void SetStart(IEnumerable<EndpointInfo> sourceEndpoints)
		{
			if (sourceEndpoints == null)
				throw new ArgumentNullException("sourceEndpoints");

			m_SourceEndpoints.Clear();
			m_SourceEndpoints.AddSorted(sourceEndpoints.Distinct());
		}

		/// <summary>
		/// Sets the destination endpoint for the query.
		/// </summary>
		/// <param name="destinationEndpoints"></param>
		public void SetEnd(IEnumerable<EndpointInfo> destinationEndpoints)
		{
			if (destinationEndpoints == null)
				throw new ArgumentNullException("destinationEndpoints");

			m_DestinationEndpoints.Clear();
			AddEnd(destinationEndpoints);
		}

		public void AddEnd(IEnumerable<EndpointInfo> destinationEndpoints)
		{
			if (destinationEndpoints == null)
				throw new ArgumentNullException("destinationEndpoints");

			EndpointInfo[] endpoints =
				destinationEndpoints.Distinct()
				                    .Order()
				                    .ToArray();

			m_DestinationEndpoints.Add(endpoints);
		}

		public IEnumerable<EndpointInfo[]> GetEnds()
		{
			return m_DestinationEndpoints;
		}
	}
}
