using System;
using System.Collections.Generic;
using System.Linq;
using ICD.Common.Utils;
using ICD.Common.Utils.Collections;
using ICD.Common.Utils.Extensions;
using ICD.Common.Utils.Services;
using ICD.Common.Utils.Services.Logging;
using ICD.Connect.Routing.Connections;
using ICD.Connect.Routing.Controls;
using ICD.Connect.Routing.Endpoints;
using ICD.Connect.Routing.RoutingGraphs;

namespace ICD.Connect.Routing.Pathfinding
{
	public sealed class DefaultPathFinder : AbstractPathFinder
	{
		private readonly IRoutingGraph m_RoutingGraph;
		private readonly int m_RoomId;

		private ILoggerService m_CachedLogger;

		public ILoggerService Logger
		{
			get { return m_CachedLogger = m_CachedLogger ?? ServiceProvider.TryGetService<ILoggerService>(); }
		}

		/// <summary>
		/// Constructor.
		/// </summary>
		public DefaultPathFinder(IRoutingGraph routingGraph, int roomId)
		{
			if (routingGraph == null)
				throw new ArgumentNullException("routingGraph");

			m_RoutingGraph = routingGraph;
			m_RoomId = roomId;
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

			EndpointInfo[][] destinations = query.GetEnds().ToArray();

			foreach (eConnectionType flag in EnumUtils.GetFlagsExceptNone(query.Type))
			{
				IcdHashSet<EndpointInfo[]> completedDestinations = new IcdHashSet<EndpointInfo[]>();

				foreach (EndpointInfo sourceEndpoint in query.GetStart())
				{
					Connection sourceConnection = m_RoutingGraph.Connections.GetOutputConnection(sourceEndpoint, flag);
					if (sourceConnection == null)
						continue;

					foreach (EndpointInfo[] destination in destinations.Where(d => !completedDestinations.Contains(d)))
					{
						IcdHashSet<Connection> destinationConnections = new IcdHashSet<Connection>();

						foreach (EndpointInfo destinationEndpoint in destination)
						{
							// Ensure the destination has a valid input connection
							Connection destinationConnection = m_RoutingGraph.Connections.GetInputConnection(destinationEndpoint, flag);
							if (destinationConnection == null)
								continue;

							destinationConnections.Add(destinationConnection);
						}

						ConnectionPath path = GetConnectionPath(sourceEndpoint, destination, sourceConnection, destinationConnections,
						                                        flag);
						if (path == null)
							continue;

						completedDestinations.Add(destination);

						yield return path;
					}
				}

				// Log errors
				foreach (EndpointInfo[] destination in destinations.Where(d => !completedDestinations.Contains(d)))
				{
					string sourceText = EndpointInfo.ArrayRangeFormat(query.GetStart());
					string destinationText = EndpointInfo.ArrayRangeFormat(destination);

					string message = string.Format("{0} failed to find {1} path from {2} to {3}", GetType().Name, flag, sourceText,
					                               destinationText);

					Logger.AddEntry(eSeverity.Error, message);
				}
			}
		}

		private ConnectionPath GetConnectionPath(EndpointInfo sourceEndpoint, IEnumerable<EndpointInfo> destination,
		                                         Connection sourceConnection, IEnumerable<Connection> destinationConnections,
		                                         eConnectionType flag)
		{
			if (destination == null)
				throw new ArgumentNullException("destination");

			if (sourceConnection == null)
				throw new ArgumentNullException("sourceConnection");

			if (destinationConnections == null)
				throw new ArgumentNullException("destinationConnections");

			KeyValuePair<Connection, IEnumerable<Connection>> kvp;

			bool found =
				RecursionUtils.BreadthFirstSearchManyDestinations(sourceConnection,
				                                                  destinationConnections,
				                                                  c =>
				                                                  GetConnectionChildren(sourceEndpoint, destination, c, flag))
				              .TryFirst(out kvp);

			return found ? new ConnectionPath(kvp.Value, flag) : null;
		}

		/// <summary>
		/// Gets the potential output connections for the given input connection.
		/// </summary>
		/// <param name="source"></param>
		/// <param name="finalDestinations"></param>
		/// <param name="inputConnection"></param>
		/// <param name="flag"></param>
		/// <returns></returns>
		private IEnumerable<Connection> GetConnectionChildren(EndpointInfo source, IEnumerable<EndpointInfo> finalDestinations,
															  Connection inputConnection, eConnectionType flag)
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
				                     c.IsAvailableToRoom(m_RoomId));
		}
	}
}
