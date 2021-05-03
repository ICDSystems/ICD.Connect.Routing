using System;
using System.Collections.Generic;
using System.Linq;
using ICD.Common.Utils;
using ICD.Common.Utils.Services;
using ICD.Connect.Routing.Connections;
using ICD.Connect.Routing.Endpoints;
using ICD.Connect.Routing.RoutingGraphs;

namespace ICD.Connect.Routing.Controls
{
	public sealed class RoutingGraphSourceConnectionComponent
	{
		private readonly IRouteSourceControl m_SourceControl;

		private readonly eConnectionType m_ConnectionTypeMask;

		private IRoutingGraph m_CachedRoutingGraph;

		public IRouteSourceControl SourceControl { get { return m_SourceControl; } }

		public eConnectionType ConnectionTypeMask { get { return m_ConnectionTypeMask; } }

		/// <summary>
		/// Gets the routing graph.
		/// </summary>
		public IRoutingGraph RoutingGraph
		{
			get { return m_CachedRoutingGraph = m_CachedRoutingGraph ?? ServiceProvider.GetService<IRoutingGraph>(); }
		}

		/// <summary>
		/// Create a midpoint component to use the routing graph to get inputs/outputs
		/// </summary>
		/// <param name="sourceControl">Midpoint control this is being used for</param>
		public RoutingGraphSourceConnectionComponent(IRouteSourceControl sourceControl) : this(sourceControl,
																						   EnumUtils
																							   .GetFlagsAllValue<eConnectionType>())
		{
		}

		/// <summary>
		/// Create a midpoint component to use the routing graph to get inputs/outputs
		/// </summary>
		/// <param name="sourceControl">Midpoint control this is being used for</param>
		/// <param name="connectionTypeMask">Connection Type Mask to restrict connections returned regardless of routing graph</param>
		public RoutingGraphSourceConnectionComponent(IRouteSourceControl sourceControl, eConnectionType connectionTypeMask)
		{
			m_SourceControl = sourceControl;
			m_ConnectionTypeMask = connectionTypeMask;
		}

		/// <summary>
		/// Gets the output at the given address.
		/// </summary>
		/// <param name="address"></param>
		/// <returns></returns>
		public ConnectorInfo GetOutput(int address)
		{
			Connection connection = RoutingGraph.Connections.GetOutputConnection(new EndpointInfo(SourceControl.Parent.Id, SourceControl.Id, address));
			if (connection == null)
				throw new ArgumentOutOfRangeException("address");

			return new ConnectorInfo(connection.Source.Address, GetMaskedConnectionType(connection.ConnectionType));
		}

		/// <summary>
		/// Returns true if the source contains an output at the given address.
		/// </summary>
		/// <param name="output"></param>
		/// <returns></returns>
		public bool ContainsOutput(int output)
		{
			return RoutingGraph.Connections.GetOutputConnection(new EndpointInfo(SourceControl.Parent.Id, SourceControl.Id, output)) != null;
		}

		/// <summary>
		/// Returns the outputs.
		/// </summary>
		/// <returns></returns>
		public IEnumerable<ConnectorInfo> GetOutputs()
		{
			return RoutingGraph.Connections
							   .GetOutputConnections(SourceControl.Parent.Id, SourceControl.Id)
							   .Select(c => new ConnectorInfo(c.Source.Address, GetMaskedConnectionType(c.ConnectionType)));
		}

		private eConnectionType GetMaskedConnectionType(eConnectionType connectionType)
		{
			return EnumUtils.GetFlagsIntersection(connectionType, ConnectionTypeMask);
		}

	}
}
