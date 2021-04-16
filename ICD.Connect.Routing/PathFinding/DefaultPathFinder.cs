using System;
using System.Collections.Generic;
using System.Linq;
using ICD.Common.Properties;
using ICD.Common.Utils;
using ICD.Common.Utils.Extensions;
using ICD.Connect.Devices.Controls;
using ICD.Connect.Routing.Connections;
using ICD.Connect.Routing.Controls;
using ICD.Connect.Routing.Endpoints;
using ICD.Connect.Routing.Extensions;
using ICD.Connect.Routing.RoutingGraphs;

namespace ICD.Connect.Routing.PathFinding
{
	public sealed class DefaultPathFinder : AbstractPathFinder
	{
		private readonly IConnectionsCollection m_Connections;
		private readonly int m_RoomId;

		/// <summary>
		/// Constructor.
		/// </summary>
		public DefaultPathFinder(IRoutingGraph routingGraph, int roomId)
		{
			if (routingGraph == null)
				throw new ArgumentNullException("routingGraph");

			m_Connections = routingGraph.Connections;
			m_RoomId = roomId;
		}

		#region Methods 

		/// <summary>
		/// Returns the best paths for the given builder queries.
		/// </summary>
		/// <param name="queries"></param>
		/// <returns></returns>
		public override IEnumerable<ConnectionPath> FindPaths(IEnumerable<PathBuilderQuery> queries)
		{
			if (queries == null)
				throw new ArgumentNullException("queries");

			return queries.SelectMany(q => FindPaths(q));
		}

		/// <summary>
		/// Returns true if there is a valid path for all of the defined queries.
		/// </summary>
		/// <param name="queries"></param>
		/// <returns></returns>
		public override bool HasPaths(IEnumerable<PathBuilderQuery> queries)
		{
			if (queries == null)
				throw new ArgumentNullException("queries");

			return queries.All(q => HasPaths(q));
		}

		#endregion

		#region Private Methods

		/// <summary>
		/// Returns true if there is a valid path for the given query.
		/// </summary>
		/// <param name="query"></param>
		/// <returns></returns>
		private bool HasPaths([NotNull] PathBuilderQuery query)
		{
			if (query == null)
				throw new ArgumentNullException("query");

			EndpointInfo[] source = query.GetStart().ToArray();
			EndpointInfo[][] destinations = query.GetEnds().ToArray();

			foreach (eConnectionType flag in EnumUtils.GetFlagsExceptNone(query.Type))
			{
				if (!HasPaths(source, destinations, flag))
					return false;
			}

			return true;
		}

		/// <summary>
		/// Returns true if there is a valid path from the source to each destination.
		/// </summary>
		/// <param name="source"></param>
		/// <param name="destinations"></param>
		/// <param name="flag"></param>
		/// <returns></returns>
		private bool HasPaths([NotNull] EndpointInfo[] source, [NotNull] EndpointInfo[][] destinations, eConnectionType flag)
		{
			if (source == null)
				throw new ArgumentNullException("source");

			if (destinations == null)
				throw new ArgumentNullException("destinations");

			if (!EnumUtils.HasSingleFlag(flag))
				throw new ArgumentException("Connection type has multiple flags", "flag");

			foreach (EndpointInfo[] destination in destinations)
			{
				bool destinationHasPath = false;

				foreach (EndpointInfo destinationEndpoint in destination)
				{
					foreach (EndpointInfo sourceEndpoint in source)
					{
						Connection outputConnection =
							m_Connections.GetOutputConnection(sourceEndpoint, destinationEndpoint, flag);
						if (outputConnection == null)
							continue;

						destinationHasPath = true;
						break;
					}

					if (destinationHasPath)
						break;
				}

				if (!destinationHasPath)
					return false;
			}

			return true;
		}

		/// <summary>
		/// Returns the best paths for the given builder query.
		/// </summary>
		/// <param name="query"></param>
		/// <returns></returns>
		[NotNull]
		private IEnumerable<ConnectionPath> FindPaths([NotNull] PathBuilderQuery query)
		{
			if (query == null)
				throw new ArgumentNullException("query");

			EndpointInfo[] source = query.GetStart().ToArray();
			EndpointInfo[][] destinations = query.GetEnds().ToArray();

			foreach (eConnectionType flag in EnumUtils.GetFlagsExceptNone(query.Type))
			{
				// Return a path from the source to each destination
				foreach (ConnectionPath path in FindPaths(source, destinations, flag))
					yield return path;
			}
		}

		/// <summary>
		/// Returns the best path from the source to each destination.
		/// </summary>
		/// <param name="source"></param>
		/// <param name="destinations"></param>
		/// <param name="flag"></param>
		/// <returns></returns>
		[NotNull]
		private IEnumerable<ConnectionPath> FindPaths([NotNull] EndpointInfo[] source,
		                                              [NotNull] EndpointInfo[][] destinations, eConnectionType flag)
		{
			if (source == null)
				throw new ArgumentNullException("source");

			if (destinations == null)
				throw new ArgumentNullException("destinations");

			if (!EnumUtils.HasSingleFlag(flag))
				throw new ArgumentException("Connection type has multiple flags", "flag");

			// Get the output connections for the source
			Connection[] sourceConnections =
				source.Select(e => m_Connections.GetOutputConnection(e, flag))
				      .Where(c => c != null)
				      .ToArray();

			foreach (EndpointInfo[] destination in destinations)
			{
				// Get the input connections for the destination
				Connection[] destinationConnections =
					destination.Select(e => m_Connections.GetInputConnection(e, flag))
					           .Where(c => c != null)
					           .ToArray();

				foreach (Connection sourceConnection in sourceConnections)
				{
					ConnectionPath path = GetConnectionPath(sourceConnection, destinationConnections, destination, flag);
					if (path == null)
						continue;

					yield return path;
					break;
				}
			}
		}

		[CanBeNull]
		private ConnectionPath GetConnectionPath([NotNull] Connection sourceConnection,
		                                         [NotNull] IEnumerable<Connection> destinationConnections,
		                                         [NotNull] IEnumerable<EndpointInfo> destination, eConnectionType flag)
		{
			if (sourceConnection == null)
				throw new ArgumentNullException("sourceConnection");

			if (destinationConnections == null)
				throw new ArgumentNullException("destinationConnections");

			if (destination == null)
				throw new ArgumentNullException("destination");

			IList<EndpointInfo> destinationCollection = destination as IList<EndpointInfo> ?? destination.ToArray();
			if (destinationCollection.Count == 0)
				return null;

			// We are recursing over the EDGES of the graph, rather than the NODES.
			// We start by using sourceConnection as our entry point, but it hasn't been validated yet.
			IRouteDestinationControl immediate =
				sourceConnection.Core
				                .GetRoutingGraph()
				                .GetDestinationControl(sourceConnection);

			// Is the immediate node our destination?
			if (destinationCollection[0].GetDeviceControlInfo() == immediate.GetDeviceControlInfo())
				return new ConnectionPath(sourceConnection.Yield(), flag);

			// Can't path through the immediate node
			if (!(immediate is IRouteMidpointControl))
				return null;

			// First connection is good, continue regular pathfinding
			KeyValuePair<Connection, IEnumerable<Connection>> kvp;
			bool found =
				RecursionUtils
					.BreadthFirstSearchPathManyDestinations(sourceConnection,
					                                        destinationConnections,
					                                        c =>
					                                        GetConnectionChildren(sourceConnection.Source, destinationCollection, c,
					                                                              flag))
					.TryFirst(out kvp);

			return found ? new ConnectionPath(kvp.Value, flag) : null;
		}

		/// <summary>
		/// Gets the potential output connections for the given input connection.
		/// </summary>
		/// <param name="source"></param>
		/// <param name="finalDestinations"></param>
		/// <param name="sourceOutputConnection"></param>
		/// <param name="flag"></param>
		/// <returns></returns>
		[NotNull]
		private IEnumerable<Connection> GetConnectionChildren(EndpointInfo source,
		                                                      [NotNull] IEnumerable<EndpointInfo> finalDestinations,
		                                                      [NotNull] Connection sourceOutputConnection,
		                                                      eConnectionType flag)
		{
			if (sourceOutputConnection == null)
				throw new ArgumentNullException("sourceOutputConnection");

			if (finalDestinations == null)
				throw new ArgumentNullException("finalDestinations");

			if (EnumUtils.HasMultipleFlags(flag))
				throw new ArgumentException("ConnectionType has multiple flags", "flag");

			return
				m_Connections.GetOutputConnections(sourceOutputConnection.Destination.GetDeviceControlInfo(),
				                                   finalDestinations,
				                                   flag)
				             .Where(c =>
				                    c.IsAvailableToSourceDevice(source.Device) &&
				                    c.IsAvailableToRoom(m_RoomId));
		}

		#endregion
	}
}
