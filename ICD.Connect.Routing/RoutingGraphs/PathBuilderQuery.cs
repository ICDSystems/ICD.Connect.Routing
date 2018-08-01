using System;
using System.Collections.Generic;
using System.Linq;
using ICD.Common.Utils.Extensions;
using ICD.Connect.Routing.Connections;
using ICD.Connect.Routing.Endpoints;

namespace ICD.Connect.Routing.RoutingGraphs
{
	/// <summary>
	/// Describes a single search for a path.
	/// </summary>
	public sealed class PathBuilderQuery
	{
		private readonly List<EndpointInfo> m_SourceEndpoints;
		private readonly List<EndpointInfo[]> m_DestinationEndpoints;
		private eConnectionType m_ConnectionType;

		/// <summary>
		/// Constructor.
		/// </summary>
		public PathBuilderQuery()
		{
			m_SourceEndpoints = new List<EndpointInfo>();
			m_DestinationEndpoints = new List<EndpointInfo[]>();
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

		/// <summary>
		/// Sets the connection type for the query.
		/// </summary>
		/// <param name="flags"></param>
		public void SetType(eConnectionType flags)
		{
			m_ConnectionType = flags;
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
	}
}
