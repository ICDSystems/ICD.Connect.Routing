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
	public sealed class RoutingGraphDestinationConnectionComponent
	{
		private readonly IRouteDestinationControl m_DestinationControl;

		private readonly eConnectionType m_ConnectionTypeMask;

		private IRoutingGraph m_CachedRoutingGraph;

		public IRouteDestinationControl DestinationControl { get { return m_DestinationControl; } }

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
		/// <param name="destinationControl">Midpoint control this is being used for</param>
		public RoutingGraphDestinationConnectionComponent(IRouteDestinationControl destinationControl) : this(destinationControl,
																						   EnumUtils
																							   .GetFlagsAllValue<eConnectionType>())
		{
		}

		/// <summary>
		/// Create a midpoint component to use the routing graph to get inputs/outputs
		/// </summary>
		/// <param name="destinationControl">Midpoint control this is being used for</param>
		/// <param name="connectionTypeMask">Connection Type Mask to restrict connections returned regardless of routing graph</param>
		public RoutingGraphDestinationConnectionComponent(IRouteDestinationControl destinationControl, eConnectionType connectionTypeMask)
		{
			m_DestinationControl = destinationControl;
			m_ConnectionTypeMask = connectionTypeMask;
		}

		/// <summary>
		/// Gets the input at the given address.
		/// </summary>
		/// <param name="input"></param>
		/// <returns></returns>
		public ConnectorInfo GetInput(int input)
		{
			Connection connection = RoutingGraph.Connections.GetInputConnection(new EndpointInfo(DestinationControl.Parent.Id, DestinationControl.Id, input));
			if (connection == null)
				throw new ArgumentOutOfRangeException("input");

			return new ConnectorInfo(connection.Destination.Address, GetMaskedConnectionType(connection.ConnectionType));
		}

		/// <summary>
		/// Returns true if the destination contains an input at the given address.
		/// </summary>
		/// <param name="input"></param>
		/// <returns></returns>
		public bool ContainsInput(int input)
		{
			return RoutingGraph.Connections.GetInputConnection(new EndpointInfo(DestinationControl.Parent.Id, DestinationControl.Id, input)) != null;
		}

		/// <summary>
		/// Returns the inputs.
		/// </summary>
		/// <returns></returns>
		public IEnumerable<ConnectorInfo> GetInputs()
		{
			return RoutingGraph.Connections
							   .GetInputConnections(DestinationControl.Parent.Id, DestinationControl.Id)
							   .Select(c => new ConnectorInfo(c.Destination.Address, GetMaskedConnectionType(c.ConnectionType)));
		}

		private eConnectionType GetMaskedConnectionType(eConnectionType connectionType)
		{
			return EnumUtils.GetFlagsIntersection(connectionType, ConnectionTypeMask);
		}
	}
}
