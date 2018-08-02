using System;
using System.Collections.Generic;
using System.Linq;
using ICD.Common.Utils;
using ICD.Connect.Routing.Connections;
using ICD.Connect.Routing.Controls;
using ICD.Connect.Routing.Endpoints;
using ICD.Connect.Routing.RoutingGraphs;

namespace ICD.Connect.Routing.Pathfinding
{
	public sealed class DefaultPathFinder : AbstractPathFinder
	{
		private readonly IRoutingGraph m_RoutingGraph;

		/// <summary>
		/// Constructor.
		/// </summary>
		public DefaultPathFinder(IRoutingGraph routingGraph)
		{
			if (routingGraph == null)
				throw new ArgumentNullException("routingGraph");

			m_RoutingGraph = routingGraph;
		}

		/// <summary>
		/// Returns the best paths for the given builder queries.
		/// </summary>
		/// <param name="queries"></param>
		/// <returns></returns>
		public override IEnumerable<ConnectionPath> FindPaths(IEnumerable<PathBuilderQuery> queries)
		{
			if (queries == null)
				throw new ArgumentNullException("queries");

			// TODO - Aggregate paths into as few paths as possible

			return queries.SelectMany(q => FindPaths(q));
		}

		/// <summary>
		/// Returns the best paths for the given builder query.
		/// </summary>
		/// <param name="query"></param>
		/// <returns></returns>
		private IEnumerable<ConnectionPath> FindPaths(PathBuilderQuery query)
		{
			if (query == null)
				throw new ArgumentNullException("query");

			EndpointInfo[] source = query.GetStart().ToArray();
			EndpointInfo[][] destinations = query.GetEnds().ToArray();

			/*
			IcdHashSet<EndpointInfo> destinationsSet = destinations.ToIcdHashSet();
			IcdHashSet<Connection> destinationConnections = new IcdHashSet<Connection>();
			Dictionary<Connection, EndpointInfo> connectionToDestinations = new Dictionary<Connection, EndpointInfo>();

			Connection sourceConnection = m_Connections.GetOutputConnection(source);

			foreach (EndpointInfo destination in destinationsSet)
			{
				// Ensure the source has a valid output connection.
				if (sourceConnection == null || !sourceConnection.ConnectionType.HasFlag(flag))
				{
					yield return new KeyValuePair<EndpointInfo, ConnectionPath>(destination, null);
					continue;
				}

				// Ensure the destination has a valid input connection
				Connection destinationConnection = m_Connections.GetInputConnection(destination);
				if (destinationConnection == null || !destinationConnection.ConnectionType.HasFlag(flag))
				{
					yield return new KeyValuePair<EndpointInfo, ConnectionPath>(destination, null);
					continue;
				}

				destinationConnections.Add(destinationConnection);
				connectionToDestinations.Add(destinationConnection, destination);
			}

			Dictionary<Connection, IEnumerable<Connection>> paths =
				RecursionUtils.BreadthFirstSearchManyDestinations(sourceConnection,
																  destinationConnections,
																  c => GetConnectionChildren(source, destinationsSet, c, flag, roomId));

			foreach (KeyValuePair<Connection, IEnumerable<Connection>> kvp in paths)
			{
				ConnectionPath finalPath = kvp.Value == null ? null : new ConnectionPath(kvp.Value, flag);
				EndpointInfo destination = connectionToDestinations[kvp.Key];

				yield return new KeyValuePair<EndpointInfo, ConnectionPath>(destination, finalPath);
			}
			 */

			throw new NotImplementedException();
		}

		/// <summary>
		/// Gets the potential output connections for the given input connection.
		/// </summary>
		/// <param name="source"></param>
		/// <param name="finalDestinations"></param>
		/// <param name="inputConnection"></param>
		/// <param name="flag"></param>
		/// <param name="roomId"></param>
		/// <returns></returns>
		private IEnumerable<Connection> GetConnectionChildren(EndpointInfo source, IEnumerable<EndpointInfo> finalDestinations,
															  Connection inputConnection, eConnectionType flag, int roomId)
		{
			if (inputConnection == null)
				throw new ArgumentNullException("inputConnection");

			if (finalDestinations == null)
				throw new ArgumentNullException("finalDestinations");

			if (EnumUtils.HasMultipleFlags(flag))
				throw new ArgumentException("ConnectionType has multiple flags", "flag");

			// Does the input connection lead to a midpoint?
			IRouteMidpointControl midpoint = m_RoutingGraph.GetDestinationControl(inputConnection) as IRouteMidpointControl;
			if (midpoint == null)
				return Enumerable.Empty<Connection>();

			return
				m_RoutingGraph.Connections
				              .GetOutputConnections(inputConnection.Destination.GetDeviceControlInfo(),
				                                    finalDestinations,
				                                    flag)
				              .Where(c =>
				                     c.IsAvailableToSourceDevice(source.Device) &&
				                     c.IsAvailableToRoom(roomId));
		}
	}
}
