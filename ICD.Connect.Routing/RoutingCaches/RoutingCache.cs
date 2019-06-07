using System;
using System.Collections.Generic;
using System.Linq;
using ICD.Common.Properties;
using ICD.Common.Utils;
using ICD.Common.Utils.Collections;
using ICD.Common.Utils.Extensions;
using ICD.Connect.API.Commands;
using ICD.Connect.API.Nodes;
using ICD.Connect.Routing.Connections;
using ICD.Connect.Routing.Controls;
using ICD.Connect.Routing.Endpoints;
using ICD.Connect.Routing.Endpoints.Destinations;
using ICD.Connect.Routing.Endpoints.Sources;
using ICD.Connect.Routing.EventArguments;
using ICD.Connect.Routing.RoutingGraphs;

namespace ICD.Connect.Routing.RoutingCaches
{
	public sealed class RoutingCache : IDisposable, IConsoleNode
	{
		#region Events

		public event EventHandler<CacheStateChangedEventArgs> OnTransmissionStateChanged;
		public event EventHandler<CacheStateChangedEventArgs> OnDetectionStateChanged;
		public event EventHandler<CacheStateChangedEventArgs> OnDestinationEndpointActiveChanged;
		public event EventHandler<EndpointRouteChangedEventArgs> OnEndpointRouteChanged;
		public event EventHandler<SourceDestinationRouteChangedEventArgs> OnSourceDestinationRouteChanged;

		#endregion

		#region Private Members

		private readonly IRoutingGraph m_RoutingGraph;

		private readonly Dictionary<ISource, IcdHashSet<EndpointInfo>> m_SourceToEndpoints;
		private readonly Dictionary<EndpointInfo, IcdHashSet<ISource>> m_EndpointToSources;

		private readonly Dictionary<IDestination, IcdHashSet<EndpointInfo>> m_DestinationToEndpoints;
		private readonly Dictionary<EndpointInfo, IcdHashSet<IDestination>> m_EndpointToDestinations;

		private readonly Dictionary<ISource, eConnectionType> m_SourceTransmitting;
		private readonly Dictionary<ISource, eConnectionType> m_SourceDetected;
		private readonly Dictionary<EndpointInfo, eConnectionType> m_SourceEndpointTransmitting;
		private readonly Dictionary<EndpointInfo, eConnectionType> m_SourceEndpointDetected;
		private readonly Dictionary<EndpointInfo, eConnectionType> m_DestinationEndpointActive;

		private readonly Dictionary<EndpointInfo, Dictionary<eConnectionType, IcdHashSet<EndpointInfo>>> m_DestinationEndpointToSourceEndpointCache;
		private readonly Dictionary<EndpointInfo, Dictionary<eConnectionType, IcdHashSet<EndpointInfo>>> m_SourceEndpointToDestinationEndpointCache;

		private readonly RoutingCacheMidpointCache m_MidpointCache;

		private readonly SafeCriticalSection m_CacheSection;

		private bool m_DebugEnabled;

		#endregion

		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="routingGraph"></param>
		public RoutingCache(IRoutingGraph routingGraph)
		{
			if (routingGraph == null)
				throw new ArgumentNullException("routingGraph");

			m_SourceToEndpoints = new Dictionary<ISource, IcdHashSet<EndpointInfo>>();
			m_EndpointToSources = new Dictionary<EndpointInfo, IcdHashSet<ISource>>();

			m_DestinationToEndpoints = new Dictionary<IDestination, IcdHashSet<EndpointInfo>>();
			m_EndpointToDestinations = new Dictionary<EndpointInfo, IcdHashSet<IDestination>>();

			m_SourceTransmitting = new Dictionary<ISource, eConnectionType>();
			m_SourceDetected = new Dictionary<ISource, eConnectionType>();
			m_SourceEndpointTransmitting = new Dictionary<EndpointInfo, eConnectionType>();
			m_SourceEndpointDetected = new Dictionary<EndpointInfo, eConnectionType>();
			m_DestinationEndpointActive = new Dictionary<EndpointInfo, eConnectionType>();

			m_DestinationEndpointToSourceEndpointCache = new Dictionary<EndpointInfo, Dictionary<eConnectionType, IcdHashSet<EndpointInfo>>>();
			m_SourceEndpointToDestinationEndpointCache = new Dictionary<EndpointInfo, Dictionary<eConnectionType, IcdHashSet<EndpointInfo>>>();

			m_MidpointCache = new RoutingCacheMidpointCache();

			m_CacheSection = new SafeCriticalSection();

			m_RoutingGraph = routingGraph;
			Subscribe(m_RoutingGraph);

			RebuildCache();
		}

		/// <summary>
		/// Release resources.
		/// </summary>
		public void Dispose()
		{
			OnTransmissionStateChanged = null;
			OnDetectionStateChanged = null;
			OnDestinationEndpointActiveChanged = null;
			OnEndpointRouteChanged = null;
			OnSourceDestinationRouteChanged = null;

			Unsubscribe(m_RoutingGraph);

			ClearCache();
		}

		#region Public Methods

		/// <summary>
		/// Clears and rebuilds the cache initial states.
		/// </summary>
		public void RebuildCache()
		{
			m_CacheSection.Enter();

			try
			{
				ClearCache();

				InitializeMidpointCaches();
				InitializeSourceCaches();
				InitializeDestinationCaches();
				InitializeRoutes();

				if (m_DebugEnabled)
					PrintAllCacheStates();
			}
			finally
			{
				m_CacheSection.Leave();
			}
		}

		/// <summary>
		/// Clears all of the cached states.
		/// </summary>
		public void ClearCache()
		{
			m_CacheSection.Enter();

			try
			{
				m_SourceToEndpoints.Clear();
				m_EndpointToSources.Clear();

				m_DestinationToEndpoints.Clear();
				m_EndpointToDestinations.Clear();

				m_SourceTransmitting.Clear();
				m_SourceDetected.Clear();
				m_SourceEndpointTransmitting.Clear();
				m_SourceEndpointDetected.Clear();
				m_DestinationEndpointActive.Clear();

				m_DestinationEndpointToSourceEndpointCache.Clear();
				m_SourceEndpointToDestinationEndpointCache.Clear();

				m_MidpointCache.Clear();
			}
			finally
			{
				m_CacheSection.Leave();
			}
		}

		#region Source State

		/// <summary>
		/// Returns true if the source is detected for the given connection type.
		/// </summary>
		/// <param name="source"></param>
		/// <param name="flag"></param>
		/// <returns></returns>
		public bool GetSourceDetected(ISource source, eConnectionType flag)
		{
			if (source == null)
				throw new ArgumentNullException("source");

			if (!EnumUtils.HasSingleFlag(flag))
				throw new ArgumentException("type cannot have multiple flags", "flag");

			m_CacheSection.Enter();

			try
			{
				eConnectionType detected;
				return m_SourceDetected.TryGetValue(source, out detected) && detected.HasFlag(flag);
			}
			finally
			{
				m_CacheSection.Leave();
			}
		}

		/// <summary>
		/// Returns true if the source endpoint is detected for the given connection type.
		/// </summary>
		/// <param name="sourceEndpoint"></param>
		/// <param name="flag"></param>
		/// <returns></returns>
		public bool GetSourceEndpointDetected(EndpointInfo sourceEndpoint, eConnectionType flag)
		{
			if (!EnumUtils.HasSingleFlag(flag))
				throw new ArgumentException("type cannot have multiple flags", "flag");

			m_CacheSection.Enter();

			try
			{
				eConnectionType detected;
				return m_SourceEndpointDetected.TryGetValue(sourceEndpoint, out detected) && detected.HasFlag(flag);
			}
			finally
			{
				m_CacheSection.Leave();
			}
		}

		/// <summary>
		/// Returns true if the source is transmitting for the given connection type.
		/// </summary>
		/// <param name="source"></param>
		/// <param name="flag"></param>
		/// <returns></returns>
		public bool GetSourceTransmitting(ISource source, eConnectionType flag)
		{
			if (source == null)
				throw new ArgumentNullException("source");

			if (!EnumUtils.HasSingleFlag(flag))
				throw new ArgumentException("type cannot have multiple flags", "flag");

			m_CacheSection.Enter();

			try
			{
				eConnectionType transmitting;
				return m_SourceTransmitting.TryGetValue(source, out transmitting) && transmitting.HasFlag(flag);
			}
			finally
			{
				m_CacheSection.Leave();
			}
		}

		/// <summary>
		/// Returns true if the source endpoint is transmitting for the given connection type.
		/// </summary>
		/// <param name="sourceEndpoint"></param>
		/// <param name="flag"></param>
		/// <returns></returns>
		public bool GetSourceEndpointTransmitting(EndpointInfo sourceEndpoint, eConnectionType flag)
		{
			if (sourceEndpoint == null)
				throw new ArgumentNullException("source");

			if (!EnumUtils.HasSingleFlag(flag))
				throw new ArgumentException("type cannot have multiple flags", "flag");

			m_CacheSection.Enter();

			try
			{
				eConnectionType transmitting;
				return m_SourceEndpointTransmitting.TryGetValue(sourceEndpoint, out transmitting) && transmitting.HasFlag(flag);
			}
			finally
			{
				m_CacheSection.Leave();
			}
		}

		/// <summary>
		/// Returns true if the destination endpoint is active for the given connection type.
		/// </summary>
		/// <param name="destinationEndpoint"></param>
		/// <param name="flag"></param>
		/// <returns></returns>
		public bool GetDestinationEndpointActive(EndpointInfo destinationEndpoint, eConnectionType flag)
		{
			if (!EnumUtils.HasSingleFlag(flag))
				throw new ArgumentException("type cannot have multiple flags", "flag");

			m_CacheSection.Enter();

			try
			{
				eConnectionType active;
				return m_DestinationEndpointActive.TryGetValue(destinationEndpoint, out active) && active.HasFlag(flag);
			}
			finally
			{
				m_CacheSection.Leave();
			}
		}

		#endregion

		#region Get Sources

		/// <summary>
		/// Gets the sources with the given source endpoint.
		/// </summary>
		/// <param name="sourceEndpoint"></param>
		/// <returns></returns>
		public IEnumerable<ISource> GetSources(EndpointInfo sourceEndpoint)
		{
			m_CacheSection.Enter();

			try
			{
				IcdHashSet<ISource> sources;
				return m_EndpointToSources.TryGetValue(sourceEndpoint, out sources)
					       ? sources.ToArray(sources.Count)
					       : Enumerable.Empty<ISource>();
			}
			finally
			{
				m_CacheSection.Leave();
			}
		}

		/// <summary>
		/// Returns all of the sources routed to the given destination for the given connection type.
		/// </summary>
		[PublicAPI]
		public IEnumerable<ISource> GetSourcesForDestination(IDestination destination, eConnectionType flag)
		{
			if (destination == null)
				throw new ArgumentNullException("destination");

			if (!EnumUtils.HasSingleFlag(flag))
				throw new ArgumentException("type cannot have multiple flags", "flag");

			return GetSourcesForDestination(destination, flag, false, false);
		}

		/// <summary>
		/// Gets all of the source endpoints currently routed to the given destination for the given flag.
		/// </summary>
		/// <param name="destination"></param>
		/// <param name="flag"></param>
		/// <param name="signalDetected">When true only return where the source is detected.</param>
		/// <param name="inputActive">When true only return for active inputs.</param>
		/// <returns></returns>
		[PublicAPI]
		public IEnumerable<ISource> GetSourcesForDestination(IDestination destination, eConnectionType flag,
		                                                     bool signalDetected, bool inputActive)
		{
			if (destination == null)
				throw new ArgumentNullException("destination");

			if (!EnumUtils.HasSingleFlag(flag))
				throw new ArgumentException("type cannot have multiple flags", "flag");

			m_CacheSection.Enter();

			try
			{
				return GetSourceEndpointsForDestination(destination, flag, signalDetected, inputActive)
					.SelectMany(e => GetSources(e))
					.Distinct()
					.ToArray();
			}
			finally
			{
				m_CacheSection.Leave();
			}
		}

		/// <summary>
		/// Returns all of the sources routed to the given destination endpoint for the given connection type.
		/// </summary>
		[PublicAPI]
		public IEnumerable<ISource> GetSourcesForDestinationEndpoint(EndpointInfo destinationEndpoint, eConnectionType flag)
		{
			if (!EnumUtils.HasSingleFlag(flag))
				throw new ArgumentException("type cannot have multiple flags", "flag");

			return GetSourcesForDestinationEndpoint(destinationEndpoint, flag, false, false);
		}

		/// <summary>
		/// Returns all of the sources routed to the given destination endpoint for the given connection type.
		/// </summary>
		[PublicAPI]
		public IEnumerable<ISource> GetSourcesForDestinationEndpoint(EndpointInfo destinationEndpoint, eConnectionType flag,
		                                                             bool signalDetected, bool inputActive)
		{
			if (!EnumUtils.HasSingleFlag(flag))
				throw new ArgumentException("type cannot have multiple flags", "flag");

			// TODO - Do we need to check this for every source endpoints?
			if (inputActive && !GetDestinationEndpointActive(destinationEndpoint, flag))
				return Enumerable.Empty<ISource>();

			m_CacheSection.Enter();

			try
			{
				Dictionary<eConnectionType, IcdHashSet<EndpointInfo>> cache;
				if (!m_DestinationEndpointToSourceEndpointCache.TryGetValue(destinationEndpoint, out cache))
					return Enumerable.Empty<ISource>();

				IcdHashSet<EndpointInfo> sourceEndpoints;
				if (!cache.TryGetValue(flag, out sourceEndpoints))
					return Enumerable.Empty<ISource>();

				return sourceEndpoints.Where(s => !signalDetected || GetSourceEndpointDetected(s, flag))
				                      .SelectMany(endpoint => GetSources(endpoint))
				                      .Distinct()
				                      .ToArray();
			}
			finally
			{
				m_CacheSection.Leave();
			}
		}

		/// <summary>
		/// Returns all of the source endpoints routed from the given destination for the given connection type.
		/// </summary>
		[PublicAPI]
		public IEnumerable<EndpointInfo> GetSourceEndpointsForDestination(IDestination destination, eConnectionType flag)
		{
			if (destination == null)
				throw new ArgumentNullException("destination");

			if (!EnumUtils.HasSingleFlag(flag))
				throw new ArgumentException("type cannot have multiple flags", "flag");

			return GetSourceEndpointsForDestination(destination, flag, false, false);
		}

		/// <summary>
		/// Gets all of the source endpoints currently routed to the given destination for the given flag.
		/// </summary>
		/// <param name="destination"></param>
		/// <param name="flag"></param>
		/// <param name="signalDetected">When true only return where the source is detected.</param>
		/// <param name="inputActive">When true only return for active inputs.</param>
		/// <returns></returns>
		[PublicAPI]
		public IEnumerable<EndpointInfo> GetSourceEndpointsForDestination(IDestination destination, eConnectionType flag,
		                                                                  bool signalDetected, bool inputActive)
		{
			if (destination == null)
				throw new ArgumentNullException("destination");

			if (!EnumUtils.HasSingleFlag(flag))
				throw new ArgumentException("type cannot have multiple flags", "flag");

			m_CacheSection.Enter();

			try
			{
				IcdHashSet<EndpointInfo> cache;
				if (!m_DestinationToEndpoints.TryGetValue(destination, out cache))
					return Enumerable.Empty<EndpointInfo>();

				return cache.Where(d => !inputActive || GetDestinationEndpointActive(d, flag))
				            .SelectMany(d => GetSourceEndpointsForDestinationEndpoint(d, flag))
				            .Distinct()
				            .Where(s => !signalDetected || GetSourceEndpointDetected(s, flag))
				            .ToArray();
			}
			finally
			{
				m_CacheSection.Leave();
			}
		}

		/// <summary>
		/// Returns all of the source endpoints routed to the given destination endpoint for the given connection type.
		/// </summary>
		[PublicAPI]
		public IEnumerable<EndpointInfo> GetSourceEndpointsForDestinationEndpoint(EndpointInfo destinationEndpoint,
		                                                                          eConnectionType flag)
		{
			if (!EnumUtils.HasSingleFlag(flag))
				throw new ArgumentException("type cannot have multiple flags", "flag");

			m_CacheSection.Enter();

			try
			{
				Dictionary<eConnectionType, IcdHashSet<EndpointInfo>> cache;
				if (!m_DestinationEndpointToSourceEndpointCache.TryGetValue(destinationEndpoint, out cache))
					return Enumerable.Empty<EndpointInfo>();

				IcdHashSet<EndpointInfo> result;
				return cache.TryGetValue(flag, out result) ? result.ToArray(result.Count) : Enumerable.Empty<EndpointInfo>();
			}
			finally
			{
				m_CacheSection.Leave();
			}
		}

		#endregion

		#region Get Destinations

		/// <summary>
		/// Gets the destinations with the given endpoint.
		/// </summary>
		/// <param name="destinationEndpoint"></param>
		/// <returns></returns>
		public IEnumerable<IDestination> GetDestinations(EndpointInfo destinationEndpoint)
		{
			m_CacheSection.Enter();

			try
			{
				IcdHashSet<IDestination> destinations;
				return m_EndpointToDestinations.TryGetValue(destinationEndpoint, out destinations)
					       ? destinations.ToArray(destinations.Count)
					       : Enumerable.Empty<IDestination>();
			}
			finally
			{
				m_CacheSection.Leave();
			}
		}

		/// <summary>
		/// Returns all of the destinations routed from the given source for the given connection type.
		/// </summary>
		[PublicAPI]
		public IEnumerable<IDestination> GetDestinationsForSource(ISource source, eConnectionType flag)
		{
			if (source == null)
				throw new ArgumentNullException("source");

			if (!EnumUtils.HasSingleFlag(flag))
				throw new ArgumentException("type cannot have multiple flags", "flag");

			return GetDestinationsForSource(source, flag, false, false);
		}

		/// <summary>
		/// Gets all of the destination endpoints currently routed from the given source for the given flag.
		/// </summary>
		/// <param name="source"></param>
		/// <param name="flag"></param>
		/// <param name="signalDetected">When true only return where the source is detected.</param>
		/// <param name="inputActive">When true only return for active inputs.</param>
		/// <returns></returns>
		[PublicAPI]
		public IEnumerable<IDestination> GetDestinationsForSource(ISource source, eConnectionType flag,
		                                                          bool signalDetected, bool inputActive)
		{
			if (source == null)
				throw new ArgumentNullException("source");

			if (!EnumUtils.HasSingleFlag(flag))
				throw new ArgumentException("type cannot have multiple flags", "flag");

			m_CacheSection.Enter();

			try
			{
				return GetDestinationEndpointsForSource(source, flag, signalDetected, inputActive)
					.SelectMany(e => GetDestinations(e))
					.Distinct()
					.ToArray();
			}
			finally
			{
				m_CacheSection.Leave();
			}
		}

		/// <summary>
		/// Returns all of the destinations routed from the given source endpoint for the given connection type.
		/// </summary>
		[PublicAPI]
		public IEnumerable<IDestination> GetDestinationsForSourceEndpoint(EndpointInfo sourceEndpoint, eConnectionType flag)
		{
			if (!EnumUtils.HasSingleFlag(flag))
				throw new ArgumentException("type cannot have multiple flags", "flag");

			m_CacheSection.Enter();

			try
			{
				Dictionary<eConnectionType, IcdHashSet<EndpointInfo>> cache;
				if (!m_SourceEndpointToDestinationEndpointCache.TryGetValue(sourceEndpoint, out cache))
					return Enumerable.Empty<IDestination>();

				IcdHashSet<EndpointInfo> endpoints;
				if (!cache.TryGetValue(flag, out endpoints))
					return Enumerable.Empty<IDestination>();

				return endpoints.SelectMany(endpoint => GetDestinations(endpoint))
				                .Distinct()
				                .ToArray();
			}
			finally
			{
				m_CacheSection.Leave();
			}
		}

		/// <summary>
		/// Returns all of the destination endpoints routed from the given source for the given connection type.
		/// </summary>
		[PublicAPI]
		public IEnumerable<EndpointInfo> GetDestinationEndpointsForSource(ISource source, eConnectionType flag)
		{
			if (source == null)
				throw new ArgumentNullException("source");

			if (!EnumUtils.HasSingleFlag(flag))
				throw new ArgumentException("type cannot have multiple flags", "flag");

			return GetDestinationEndpointsForSource(source, flag, false, false);
		}

		/// <summary>
		/// Gets all of the destination endpoints currently routed from the given source for the given flag.
		/// </summary>
		/// <param name="source"></param>
		/// <param name="flag"></param>
		/// <param name="signalDetected">When true only return where the source is detected.</param>
		/// <param name="inputActive">When true only return for active inputs.</param>
		/// <returns></returns>
		[PublicAPI]
		public IEnumerable<EndpointInfo> GetDestinationEndpointsForSource(ISource source, eConnectionType flag,
		                                                                  bool signalDetected, bool inputActive)
		{
			if (source == null)
				throw new ArgumentNullException("source");

			if (!EnumUtils.HasSingleFlag(flag))
				throw new ArgumentException("type cannot have multiple flags", "flag");

			m_CacheSection.Enter();

			try
			{
				IcdHashSet<EndpointInfo> cache;
				if (!m_SourceToEndpoints.TryGetValue(source, out cache))
					return Enumerable.Empty<EndpointInfo>();

				return cache.Where(s => !signalDetected || GetSourceEndpointDetected(s, flag))
							.SelectMany(s => GetDestinationEndpointsForSourceEndpoint(s, flag))
				            .Distinct()
							.Where(d => !inputActive || GetDestinationEndpointActive(d, flag))
				            .ToArray();
			}
			finally
			{
				m_CacheSection.Leave();
			}
		}

		/// <summary>
		/// Returns all of the destination endpoints routed from the given source endpoint for the given connection type.
		/// </summary>
		[PublicAPI]
		public IEnumerable<EndpointInfo> GetDestinationEndpointsForSourceEndpoint(EndpointInfo sourceEndpoint,
		                                                                          eConnectionType flag)
		{
			if (!EnumUtils.HasSingleFlag(flag))
				throw new ArgumentException("type cannot have multiple flags", "flag");

			m_CacheSection.Enter();

			try
			{
				Dictionary<eConnectionType, IcdHashSet<EndpointInfo>> types;
				if (!m_SourceEndpointToDestinationEndpointCache.TryGetValue(sourceEndpoint, out types))
					return Enumerable.Empty<EndpointInfo>();

				IcdHashSet<EndpointInfo> result;
				return types.TryGetValue(flag, out result) ? result.ToArray(result.Count) : Enumerable.Empty<EndpointInfo>();
			}
			finally
			{
				m_CacheSection.Leave();
			}
		}

		#endregion

		#endregion

		#region Private Methods

		/// <summary>
		/// Initializes the midpoint caches.
		/// </summary>
		private void InitializeMidpointCaches()
		{
			m_CacheSection.Enter();

			try
			{
				foreach (Connection connection in m_RoutingGraph.Connections)
				{
					IRouteMidpointControl midpoint = m_RoutingGraph.GetSourceControl(connection) as IRouteMidpointControl;
					if (midpoint == null)
						continue;

					ConnectorInfo outputConnector = midpoint.GetOutput(connection.Source.Address);
					foreach (eConnectionType flag in EnumUtils.GetFlagsExceptNone(outputConnector.ConnectionType))
					{
						ConnectorInfo? inputConnector = midpoint.GetInput(outputConnector.Address, flag);
						int? inputAddress = inputConnector.HasValue ? inputConnector.Value.Address : (int?) null;

						m_MidpointCache.SetCachedInputForOutput(midpoint, null, inputAddress, outputConnector.Address, flag);
					}
				}
			}
			finally
			{
				m_CacheSection.Leave();
			}
		}

		/// <summary>
		/// Initializes the source caches.
		/// </summary>
		private void InitializeSourceCaches()
		{
			m_CacheSection.Enter();

			try
			{
				eConnectionType all = EnumUtils.GetFlagsAllValue<eConnectionType>();

				foreach (ISource source in m_RoutingGraph.Sources)
				{
					IcdHashSet<EndpointInfo> sourceEndpoints =
						m_RoutingGraph.Connections
						              .FilterEndpointsAny(source, all)
						              .ToIcdHashSet();

					m_SourceToEndpoints.Add(source, sourceEndpoints);

					foreach (EndpointInfo endpoint in sourceEndpoints)
					{
						IcdHashSet<ISource> sources;
						if (!m_EndpointToSources.TryGetValue(endpoint, out sources))
						{
							sources = new IcdHashSet<ISource>();
							m_EndpointToSources.Add(endpoint, sources);
						}

						sources.Add(source);
					}
				}

				foreach (EndpointInfo endpoint in m_EndpointToSources.Keys)
					UpdateSourceEndpoint(endpoint);

				foreach (Connection connection in m_RoutingGraph.Connections)
				{
					IRouteSourceControl sourceControl = m_RoutingGraph.GetSourceControl(connection);
					ConnectorInfo outputConnector = sourceControl.GetOutput(connection.Source.Address);

					foreach (eConnectionType flag in EnumUtils.GetFlagsExceptNone(outputConnector.ConnectionType))
					{
						bool transmission = sourceControl.GetActiveTransmissionState(outputConnector.Address, flag);
						UpdateSourceEndpointTransmissionState(connection.Source, flag, transmission);
					}
				}
			}
			finally
			{
				m_CacheSection.Leave();
			}
		}

		/// <summary>
		/// Initializes the destination caches.
		/// </summary>
		private void InitializeDestinationCaches()
		{
			m_CacheSection.Enter();

			try
			{
				eConnectionType all = EnumUtils.GetFlagsAllValue<eConnectionType>();

				foreach (IDestination destination in m_RoutingGraph.Destinations)
				{
					IcdHashSet<EndpointInfo> destinationEndpoints =
						m_RoutingGraph.Connections
						              .FilterEndpointsAny(destination, all)
						              .ToIcdHashSet();

					m_DestinationToEndpoints.Add(destination, destinationEndpoints);

					foreach (EndpointInfo endpoint in destinationEndpoints)
					{
						IcdHashSet<IDestination> destinations;
						if (!m_EndpointToDestinations.TryGetValue(endpoint, out destinations))
						{
							destinations = new IcdHashSet<IDestination>();
							m_EndpointToDestinations.Add(endpoint, destinations);
						}

						destinations.Add(destination);
					}
				}

				foreach (EndpointInfo endpoint in m_EndpointToDestinations.Keys)
					UpdateDestinationEndpoint(endpoint);

				foreach (Connection connection in m_RoutingGraph.Connections)
				{
					IRouteDestinationControl destinationControl = m_RoutingGraph.GetDestinationControl(connection);
					ConnectorInfo inputConnector = destinationControl.GetInput(connection.Destination.Address);

					foreach (eConnectionType flag in EnumUtils.GetFlagsExceptNone(inputConnector.ConnectionType))
					{
						bool detected = destinationControl.GetSignalDetectedState(inputConnector.Address, flag);
						UpdateSourceEndpointDetectionState(connection.Source, flag, detected);

						bool active = destinationControl.GetInputActiveState(inputConnector.Address, flag);
						UpdateDestinationEndpointInputActiveState(connection.Destination, flag, active);
					}
				}
			}
			finally
			{
				m_CacheSection.Leave();
			}
		}

		/// <summary>
		/// Initializes the route caches.
		/// </summary>
		private void InitializeRoutes()
		{
			m_CacheSection.Enter();

			try
			{
				foreach (
					EndpointInfo destinationEndpoint in m_RoutingGraph.Connections.Select(c => c.Destination).Distinct())
				{
					Dictionary<eConnectionType, IcdHashSet<EndpointInfo>> typeToSourceEndpoints;
					if (!m_DestinationEndpointToSourceEndpointCache.TryGetValue(destinationEndpoint, out typeToSourceEndpoints))
					{
						typeToSourceEndpoints = new Dictionary<eConnectionType, IcdHashSet<EndpointInfo>>();
						m_DestinationEndpointToSourceEndpointCache.Add(destinationEndpoint, typeToSourceEndpoints);
					}

					IRouteDestinationControl finalDestinationControl =
						m_RoutingGraph.GetDestinationControl(destinationEndpoint.Device, destinationEndpoint.Control);
					if (!finalDestinationControl.ContainsInput(destinationEndpoint.Address))
						continue;

					ConnectorInfo connector = finalDestinationControl.GetInput(destinationEndpoint.Address);

					foreach (eConnectionType flag in EnumUtils.GetFlagsExceptNone(connector.ConnectionType))
					{
						IcdHashSet<EndpointInfo> activeRouteSourceEndpoints = new IcdHashSet<EndpointInfo>();
						IcdHashSet<EndpointInfo> activeRouteDestinationEndpoints = new IcdHashSet<EndpointInfo>();

						Queue<EndpointInfo> process = new Queue<EndpointInfo>();
						process.Enqueue(destinationEndpoint);

						EndpointInfo currentDestinationEndpoint;

						while (process.Dequeue(out currentDestinationEndpoint))
						{
							Connection connection = m_RoutingGraph.Connections.GetInputConnection(currentDestinationEndpoint, flag);
							if (connection == null)
								continue;

							EndpointInfo currentSourceEndpoint = connection.Source;
							activeRouteSourceEndpoints.Add(currentSourceEndpoint);
							activeRouteDestinationEndpoints.Add(currentDestinationEndpoint);

							IRouteMidpointControl midControl =
								m_RoutingGraph.GetControl<IRouteSourceControl>(currentSourceEndpoint) as IRouteMidpointControl;
							if (midControl == null)
								continue;

							ConnectorInfo? activeInput = midControl.GetInput(currentSourceEndpoint.Address, flag);
							if (!activeInput.HasValue)
								continue;

							process.Enqueue(new EndpointInfo(currentSourceEndpoint.Device,
							                                 currentSourceEndpoint.Control,
							                                 activeInput.Value.Address));
						}

						if (activeRouteSourceEndpoints.Count == 0 || activeRouteDestinationEndpoints.Count == 0)
							continue;

						IcdHashSet<EndpointInfo> sourceEndpoints;
						if (!typeToSourceEndpoints.TryGetValue(flag, out sourceEndpoints))
						{
							sourceEndpoints = new IcdHashSet<EndpointInfo>();
							typeToSourceEndpoints.Add(flag, sourceEndpoints);
						}

						sourceEndpoints.AddRange(activeRouteSourceEndpoints);

						foreach (EndpointInfo sourceKey in activeRouteSourceEndpoints)
						{
							Dictionary<eConnectionType, IcdHashSet<EndpointInfo>> typeToDestinationEndpoints;
							if (!m_SourceEndpointToDestinationEndpointCache.TryGetValue(sourceKey,
							                                                            out typeToDestinationEndpoints))
							{
								typeToDestinationEndpoints = new Dictionary<eConnectionType, IcdHashSet<EndpointInfo>>();
								m_SourceEndpointToDestinationEndpointCache.Add(sourceKey, typeToDestinationEndpoints);
							}

							IcdHashSet<EndpointInfo> destinationEndpoints;
							if (!typeToDestinationEndpoints.TryGetValue(flag, out destinationEndpoints))
							{
								destinationEndpoints = new IcdHashSet<EndpointInfo>();
								typeToDestinationEndpoints.Add(flag, destinationEndpoints);
							}

							destinationEndpoints.AddRange(activeRouteDestinationEndpoints);
						}
					}
				}
			}
			finally
			{
				m_CacheSection.Leave();
			}
		}

		private void UpdateSourceEndpoint(EndpointInfo endpoint)
		{
			m_CacheSection.Enter();

			try
			{
				IRouteSourceControl control = m_RoutingGraph.GetSourceControl(endpoint);
				if (!control.ContainsOutput(endpoint.Address))
					return;

				ConnectorInfo connector = control.GetOutput(endpoint.Address);

				foreach (eConnectionType flag in EnumUtils.GetFlagsExceptNone(connector.ConnectionType))
				{
					bool transmission = control.GetActiveTransmissionState(endpoint.Address, flag);
					UpdateSourceEndpointTransmissionState(endpoint, flag, transmission);

					bool detection = m_RoutingGraph.SourceDetected(endpoint, flag);
					UpdateSourceEndpointDetectionState(endpoint, flag, detection);
				}
			}
			finally
			{
				m_CacheSection.Leave();
			}
		}

		private void UpdateDestinationEndpoint(EndpointInfo endpoint)
		{
			m_CacheSection.Enter();

			try
			{
				IRouteDestinationControl control = m_RoutingGraph.GetDestinationControl(endpoint);
				if (!control.ContainsInput(endpoint.Address))
					return;

				ConnectorInfo connector = control.GetInput(endpoint.Address);

				foreach (eConnectionType flag in EnumUtils.GetFlagsExceptNone(connector.ConnectionType))
				{
					bool active = control.GetInputActiveState(endpoint.Address, flag);
					UpdateDestinationEndpointInputActiveState(endpoint, flag, active);
				}
			}
			finally
			{
				m_CacheSection.Leave();
			}
		}

		private bool UpdateSourceEndpointTransmissionState(EndpointInfo endpoint, eConnectionType type, bool state)
		{
			m_CacheSection.Enter();

			try
			{
				eConnectionType oldFlags = m_SourceEndpointTransmitting.GetDefault(endpoint);
				eConnectionType newFlags = oldFlags;

				if (state)
					newFlags |= type;
				else
					newFlags &= ~type;

				if (newFlags == oldFlags)
					return false;

				m_SourceEndpointTransmitting[endpoint] = newFlags;

				IcdHashSet<ISource> sources;
				if (m_EndpointToSources.TryGetValue(endpoint, out sources))
				{
					foreach (ISource source in sources)
						UpdateSourceTransmissionState(source);
				}

				if (m_DebugEnabled)
					PrintEndpointTransmitting();

				return true;
			}
			finally
			{
				m_CacheSection.Leave();
			}
		}

		private bool UpdateSourceTransmissionState(ISource source)
		{
			if (source == null)
				throw new ArgumentNullException("source");

			m_CacheSection.Enter();

			try
			{
				eConnectionType flags =
					m_SourceToEndpoints.GetDefault(source)
					                   .Aggregate(eConnectionType.None,
					                              (current, endpoint) =>
					                              current | m_SourceEndpointTransmitting.GetDefault(endpoint));

				eConnectionType oldFlags = m_SourceTransmitting.GetDefault(source);

				if (flags == oldFlags)
					return false;

				m_SourceTransmitting[source] = flags;

				if (m_DebugEnabled)
					PrintSourceTransmitting();

				return true;
			}
			finally
			{
				m_CacheSection.Leave();
			}
		}

		private bool UpdateSourceEndpointDetectionState(EndpointInfo endpoint, eConnectionType type, bool state)
		{
			m_CacheSection.Enter();

			try
			{
				eConnectionType oldFlags = m_SourceEndpointDetected.GetDefault(endpoint);
				eConnectionType newFlags = oldFlags;

				if (state)
					newFlags |= type;
				else
					newFlags &= ~type;

				if (newFlags == oldFlags)
					return false;

				m_SourceEndpointDetected[endpoint] = newFlags;

				if (m_DebugEnabled)
					PrintEndpointDetectedMap();

				IcdHashSet<ISource> sources;
				if (m_EndpointToSources.TryGetValue(endpoint, out sources))
				{
					foreach (ISource source in sources)
						UpdateSourceDetectionState(source);
				}

				return true;
			}
			finally
			{
				m_CacheSection.Leave();
			}
		}

		private bool UpdateSourceDetectionState(ISource source)
		{
			if (source == null)
				throw new ArgumentNullException("source");

			m_CacheSection.Enter();

			try
			{
				eConnectionType flags =
					m_SourceToEndpoints.GetDefault(source)
					                   .Aggregate(eConnectionType.None,
					                              (current, endpoint) =>
					                              current | m_SourceEndpointDetected.GetDefault(endpoint));

				eConnectionType oldFlags = m_SourceDetected.GetDefault(source);

				if (flags == oldFlags)
					return false;

				m_SourceDetected[source] = flags;

				if (m_DebugEnabled)
					PrintSourceDetectedMap();

				return true;
			}
			finally
			{
				m_CacheSection.Leave();
			}
		}

		private bool UpdateDestinationEndpointInputActiveState(EndpointInfo endpoint, eConnectionType type, bool state)
		{
			m_CacheSection.Enter();

			try
			{
				eConnectionType oldFlags = m_DestinationEndpointActive.GetDefault(endpoint);
				eConnectionType newFlags = oldFlags;

				if (state)
					newFlags |= type;
				else
					newFlags &= ~type;

				if (newFlags == oldFlags)
					return false;

				m_DestinationEndpointActive[endpoint] = newFlags;

				if (m_DebugEnabled)
					PrintDestinationEndpointActiveMap();

				return true;
			}
			finally
			{
				m_CacheSection.Leave();
			}
		}

		/// <summary>
		/// Removes each source endpoint from the cache for each destination endpoint.
		/// </summary>
		/// <param name="oldSourceEndpoints"></param>
		/// <param name="destinations"></param>
		/// <param name="type"></param>
		/// <returns></returns>
		private bool RemoveOldValuesFromDestinationCache(IcdHashSet<EndpointInfo> oldSourceEndpoints,
														 IEnumerable<EndpointInfo> destinations,
														 eConnectionType type)
		{
			if (!EnumUtils.HasSingleFlag(type))
				throw new ArgumentException("Type has multiple flags", "type");

			if (oldSourceEndpoints == null)
				throw new ArgumentNullException("oldSourceEndpoints");

			if (destinations == null)
				throw new ArgumentNullException("destinations");

			m_CacheSection.Enter();

			try
			{
				bool changed = false;

				foreach (EndpointInfo destination in destinations)
				{
					Dictionary<eConnectionType, IcdHashSet<EndpointInfo>> destinationCache;
					if (!m_DestinationEndpointToSourceEndpointCache.TryGetValue(destination, out destinationCache))
						continue;

					IcdHashSet<EndpointInfo> typeCache;
					if (!destinationCache.TryGetValue(type, out typeCache))
						continue;

					foreach (EndpointInfo endpointToRemove in oldSourceEndpoints)
						changed |= typeCache.Remove(endpointToRemove);
				}

				return changed;
			}
			finally
			{
				m_CacheSection.Leave();
			}
		}

		/// <summary>
		/// Adds each source endpoint to the cache for each destination endpoint.
		/// </summary>
		/// <param name="newSourceEndpoints"></param>
		/// <param name="destinations"></param>
		/// <param name="type"></param>
		/// <returns></returns>
		private bool AddNewValuesToDestinationCache(IcdHashSet<EndpointInfo> newSourceEndpoints,
													IEnumerable<EndpointInfo> destinations,
													eConnectionType type)
		{
			if (newSourceEndpoints == null)
				throw new ArgumentNullException("newSourceEndpoints");

			if (destinations == null)
				throw new ArgumentNullException("destinations");

			if (!EnumUtils.HasSingleFlag(type))
				throw new ArgumentException("Type has multiple flags", "type");

			m_CacheSection.Enter();

			try
			{
				bool changed = false;

				foreach (EndpointInfo destination in destinations)
				{
					Dictionary<eConnectionType, IcdHashSet<EndpointInfo>> destinationCache;
					if (!m_DestinationEndpointToSourceEndpointCache.TryGetValue(destination, out destinationCache))
					{
						destinationCache = new Dictionary<eConnectionType, IcdHashSet<EndpointInfo>>();
						m_DestinationEndpointToSourceEndpointCache.Add(destination, destinationCache);
					}

					IcdHashSet<EndpointInfo> typeCache;
					if (!destinationCache.TryGetValue(type, out typeCache))
					{
						typeCache = new IcdHashSet<EndpointInfo>();
						destinationCache.Add(type, typeCache);
					}

					foreach (EndpointInfo endpointToAdd in newSourceEndpoints)
						changed |= typeCache.Add(endpointToAdd);
				}

				return changed;
			}
			finally
			{
				m_CacheSection.Leave();
			}
		}

		/// <summary>
		/// Removes each destination endpoint from the cache for each source endpoint.
		/// </summary>
		/// <param name="oldSourceEndpoints"></param>
		/// <param name="destinations"></param>
		/// <param name="type"></param>
		/// <returns></returns>
		private bool RemoveOldValuesFromSourceCache(IEnumerable<EndpointInfo> oldSourceEndpoints,
													IcdHashSet<EndpointInfo> destinations,
													eConnectionType type)
		{
			if (oldSourceEndpoints == null)
				throw new ArgumentNullException("oldSourceEndpoints");

			if (destinations == null)
				throw new ArgumentNullException("destinations");

			if (!EnumUtils.HasSingleFlag(type))
				throw new ArgumentException("Type has multiple flags", "type");

			m_CacheSection.Enter();

			try
			{
				bool changed = false;

				foreach (EndpointInfo source in oldSourceEndpoints)
				{
					Dictionary<eConnectionType, IcdHashSet<EndpointInfo>> sourceCache;
					if (!m_SourceEndpointToDestinationEndpointCache.TryGetValue(source, out sourceCache))
						continue;

					IcdHashSet<EndpointInfo> typeCache;
					if (!sourceCache.TryGetValue(type, out typeCache))
						continue;

					foreach (EndpointInfo endpointToRemove in destinations)
						changed |= typeCache.Remove(endpointToRemove);
				}

				return changed;
			}
			finally
			{
				m_CacheSection.Leave();
			}
		}

		/// <summary>
		/// Adds each destination endpoint to the cache for each source endpoint.
		/// </summary>
		/// <param name="newSourceEndpoints"></param>
		/// <param name="destinations"></param>
		/// <param name="type"></param>
		/// <returns></returns>
		private bool AddNewValuesToSourceCache(IEnumerable<EndpointInfo> newSourceEndpoints,
											   IcdHashSet<EndpointInfo> destinations,
											   eConnectionType type)
		{
			if (!EnumUtils.HasSingleFlag(type))
				throw new ArgumentException("Type has multiple flags", "type");

			if (newSourceEndpoints == null)
				throw new ArgumentNullException("newSourceEndpoints");

			if (destinations == null)
				throw new ArgumentNullException("destinations");

			m_CacheSection.Enter();

			try
			{
				bool changed = false;

				foreach (EndpointInfo source in newSourceEndpoints)
				{
					Dictionary<eConnectionType, IcdHashSet<EndpointInfo>> sourceCache;
					if (!m_SourceEndpointToDestinationEndpointCache.TryGetValue(source, out sourceCache))
					{
						sourceCache = new Dictionary<eConnectionType, IcdHashSet<EndpointInfo>>();
						m_SourceEndpointToDestinationEndpointCache.Add(source, sourceCache);
					}

					IcdHashSet<EndpointInfo> typeCache;
					if (!sourceCache.TryGetValue(type, out typeCache))
					{
						typeCache = new IcdHashSet<EndpointInfo>();
						sourceCache.Add(type, typeCache);
					}

					foreach (EndpointInfo endpointToAdd in destinations)
						changed |= typeCache.Add(endpointToAdd);
				}

				return changed;
			}
			finally
			{
				m_CacheSection.Leave();
			}
		}

		#endregion

		#region Routing Graph Callbacks

		/// <summary>
		/// Subscribe to the routing graph events.
		/// </summary>
		/// <param name="routingGraph"></param>
		private void Subscribe(IRoutingGraph routingGraph)
		{
			routingGraph.OnSourceTransmissionStateChanged += RoutingGraphOnSourceTransmissionStateChanged;
			routingGraph.OnSourceDetectionStateChanged += RoutingGraphOnSourceDetectionStateChanged;
			routingGraph.OnRouteChanged += RoutingGraphOnRouteChanged;
			routingGraph.OnDestinationInputActiveStateChanged += RoutingGraphOnDestinationInputActiveStateChanged;
		}

		/// <summary>
		/// Unsubscribe from the routing graph.
		/// </summary>
		/// <param name="routingGraph"></param>
		private void Unsubscribe(IRoutingGraph routingGraph)
		{
			routingGraph.OnSourceTransmissionStateChanged -= RoutingGraphOnSourceTransmissionStateChanged;
			routingGraph.OnSourceDetectionStateChanged -= RoutingGraphOnSourceDetectionStateChanged;
			routingGraph.OnRouteChanged -= RoutingGraphOnRouteChanged;
			routingGraph.OnDestinationInputActiveStateChanged -= RoutingGraphOnDestinationInputActiveStateChanged;
		}

		private void RoutingGraphOnRouteChanged(object sender, SwitcherRouteChangeEventArgs args)
		{
			if (args == null)
				throw new ArgumentNullException("args");

			UpdateInputForOutput(args.Control, args.OldInput, args.NewInput, args.Output, args.Type);
		}

		/// <summary>
		/// Updates the routing cache to match the new state for the given switcher.
		/// </summary>
		/// <param name="control"></param>
		/// <param name="oldInput"></param>
		/// <param name="newInput"></param>
		/// <param name="output"></param>
		/// <param name="type"></param>
		private void UpdateInputForOutput(IRouteMidpointControl control, int? oldInput, int? newInput, int output, eConnectionType type)
		{
			if (control == null)
				throw new ArgumentNullException("control");

			// No change
			if (oldInput == newInput)
				return;

			eConnectionType typeChange = eConnectionType.None;

			m_CacheSection.Enter();

			try
			{
				foreach (eConnectionType flag in EnumUtils.GetFlagsExceptNone(type))
				{
					// Update the midpoint input/output mapping
					if (!m_MidpointCache.SetCachedInputForOutput(control, oldInput, newInput, output, flag))
						continue;

					IcdHashSet<EndpointInfo> destinations =
						GetDestinationEndpoints(control.GetOutputEndpointInfo(output), flag).ToIcdHashSet();

					// Don't care about a route change if there are no destinations
					if (destinations.Count == 0)
						continue;

					IcdHashSet<EndpointInfo> oldSources = new IcdHashSet<EndpointInfo>();
					IcdHashSet<EndpointInfo> newSources = new IcdHashSet<EndpointInfo>();

					if (oldInput.HasValue)
					{
						IEnumerable<EndpointInfo> sources =
							GetSourceEndpointsRecursive(control.GetInputEndpointInfo(oldInput.Value), flag)
								.Where(e => control.Parent.Id != e.Device);
						oldSources.AddRange(sources);
					}

					if (newInput.HasValue)
					{
						IEnumerable<EndpointInfo> sources =
							GetSourceEndpointsRecursive(control.GetInputEndpointInfo(newInput.Value), flag)
								.Where(e => control.Parent.Id != e.Device);
						newSources.AddRange(sources);
					}

					// No change
					if (oldSources.SetEquals(newSources))
						continue;

					bool change = false;

					if (oldSources.Count > 0)
					{
						change |= RemoveOldValuesFromSourceCache(oldSources, destinations, flag);
						change |= RemoveOldValuesFromDestinationCache(oldSources, destinations, flag);
					}

					if (newSources.Count > 0)
					{
						change |= AddNewValuesToSourceCache(newSources, destinations, flag);
						change |= AddNewValuesToDestinationCache(newSources, destinations, flag);
					}

					if (m_DebugEnabled)
						PrintRouteMaps();

					if (change)
						typeChange |= flag;
				}
			}
			finally
			{
				m_CacheSection.Leave();
			}

			if (typeChange == eConnectionType.None)
				return;

			OnEndpointRouteChanged.Raise(this, new EndpointRouteChangedEventArgs(typeChange));
			OnSourceDestinationRouteChanged.Raise(this, new SourceDestinationRouteChangedEventArgs(typeChange));
		}

		/// <summary>
		/// Walks forward from the given output endpoint and returns all of the input endpoints.
		/// </summary>
		/// <param name="outputEndpointInfo"></param>
		/// <param name="flag"></param>
		/// <returns></returns>
		private IEnumerable<EndpointInfo> GetDestinationEndpoints(EndpointInfo outputEndpointInfo, eConnectionType flag)
		{
			if (!EnumUtils.HasSingleFlag(flag))
				throw new ArgumentException("Connection type must be a single flag", "flag");

			m_CacheSection.Enter();

			try
			{
				IcdHashSet<EndpointInfo> destinations = new IcdHashSet<EndpointInfo>();

				Queue<EndpointInfo> process = new Queue<EndpointInfo>();
				process.Enqueue(outputEndpointInfo);

				while (process.Count > 0)
				{
					EndpointInfo current = process.Dequeue();

					// Grab the immediate destination for this source and add it to the hashset
					Connection connection = m_RoutingGraph.Connections.GetOutputConnection(current, flag);
					if (connection == null)
						continue;

					// Fix for infinite loop
					if (destinations.Contains(connection.Destination))
						continue;

					destinations.Add(connection.Destination);

					IEnumerable<EndpointInfo> outputs = m_MidpointCache.GetCachedOutputsForInput(connection.Destination, flag);
					process.EnqueueRange(outputs);
				}

				return destinations;
			}
			finally
			{
				m_CacheSection.Leave();
			}
		}

		private IEnumerable<EndpointInfo> GetSourceEndpointsRecursive(EndpointInfo inputEndpointInfo, eConnectionType flag)
		{
			if (!EnumUtils.HasSingleFlag(flag))
				throw new ArgumentException("Connection type must be a single flag", "flag");

			m_CacheSection.Enter();

			try
			{
				IcdHashSet<Connection> visited = new IcdHashSet<Connection>();

				while (true)
				{
					Connection connection = m_RoutingGraph.Connections.GetInputConnection(inputEndpointInfo, flag);
					if (connection == null)
						yield break;

					// Fix for infinite loop
					if (!visited.Add(connection))
						yield break;

					yield return connection.Source;

					EndpointInfo? inputForOutput = m_MidpointCache.GetCachedInputForOutput(connection.Source, flag);
					if (inputForOutput == null)
						yield break;

					inputEndpointInfo = inputForOutput.Value;
				}
			}
			finally
			{
				m_CacheSection.Leave();
			}
		}

		private void RoutingGraphOnSourceTransmissionStateChanged(object sender, EndpointStateEventArgs args)
		{
			CacheStateChangedEventArgs cache;

			m_CacheSection.Enter();

			try
			{
				// We don't care about transmission state changes unless it's a source
				if (!m_EndpointToSources.ContainsKey(args.Endpoint))
					return;

				cache = UpdateSourceEndpointTransmissionState(args.Endpoint, args.Type, args.State)
					        ? new CacheStateChangedEventArgs(new[] {args.Endpoint}, args.Type, args.State)
					        : null;
			}
			finally
			{
				m_CacheSection.Leave();
			}

			if (cache != null)
				OnTransmissionStateChanged.Raise(this, cache);
		}

		private void RoutingGraphOnSourceDetectionStateChanged(object sender, EndpointStateEventArgs args)
		{
			CacheStateChangedEventArgs cache;

			m_CacheSection.Enter();

			try
			{
				// We don't care about detection state changes unless it's a source
				if (!m_EndpointToSources.ContainsKey(args.Endpoint))
					return;

				cache = UpdateSourceEndpointDetectionState(args.Endpoint, args.Type, args.State)
					        ? new CacheStateChangedEventArgs(new[] {args.Endpoint}, args.Type, args.State)
					        : null;
			}
			finally
			{
				m_CacheSection.Leave();
			}

			if (cache != null)
				OnDetectionStateChanged.Raise(this, cache);
		}

		private void RoutingGraphOnDestinationInputActiveStateChanged(object sender, EndpointStateEventArgs args)
		{
			m_CacheSection.Enter();

			try
			{
				// We don't care about active state changes unless it's a destination
				if (!m_EndpointToDestinations.ContainsKey(args.Endpoint))
					return;

				if (!UpdateDestinationEndpointInputActiveState(args.Endpoint, args.Type, args.State))
					return;
			}
			finally
			{
				m_CacheSection.Leave();
			}

			OnDestinationEndpointActiveChanged.Raise(this, new CacheStateChangedEventArgs(new[] {args.Endpoint}, args.Type, args.State));
		}

		#endregion

		#region Debug Stuff

		private void PrintAllCacheStates()
		{
			m_CacheSection.Enter();

			try
			{
				PrintEndpointMaps();
				PrintTransmittingMaps();
				PrintDetectedMaps();
				PrintDestinationEndpointActiveMap();
				PrintRouteMaps();
			}
			finally
			{
				m_CacheSection.Leave();
			}
		}

		private void PrintEndpointMaps()
		{
			m_CacheSection.Enter();

			try
			{
				PrintSourceToEndpoints();
				PrintEndpointToSources();
				PrintDestinationToEndpoints();
				PrintEndpointToDestinations();
			}
			finally
			{
				m_CacheSection.Leave();
			}
		}

		private void PrintSourceToEndpoints()
		{
			IcdConsole.PrintLine(eConsoleColor.Magenta, "Source To Endpoints");

			TableBuilder builder = new TableBuilder("Source Name", "Source ID", "Endpoint");

			foreach (var mapping in m_SourceToEndpoints)
			{
				if (mapping.Value.Count == 0)
				{
					builder.AddRow(mapping.Key.Name, mapping.Key.Id, "NO ENTRIES");
					builder.AddSeparator();
					continue;
				}

				builder.AddRow(mapping.Key.Name, mapping.Key.Id, mapping.Value.First());

				if (mapping.Value.Count == 1)
				{
					builder.AddSeparator();
					continue;
				}

				foreach (var value in mapping.Value.Skip(1))
				{
					builder.AddRow("", "", value);
				}
				builder.AddSeparator();
			}

			IcdConsole.PrintLine(eConsoleColor.White, builder.ToString());
		}

		private void PrintEndpointToSources()
		{
			IcdConsole.PrintLine(eConsoleColor.Magenta, "Endpoint To Sources");

			TableBuilder builder = new TableBuilder("Endpoint", "Source Name", "Source Id");

			foreach (var mapping in m_EndpointToSources)
			{
				if (mapping.Value.Count == 0)
				{
					builder.AddRow(mapping.Key, "NO ENTRIES", "NO ENTRIES");
					builder.AddSeparator();
					continue;
				}

				builder.AddRow(mapping.Key, mapping.Value.First().Name, mapping.Value.First().Id);

				if (mapping.Value.Count == 1)
				{
					builder.AddSeparator();
					continue;
				}

				foreach (var value in mapping.Value.Skip(1))
				{
					builder.AddRow("", value.Name, value.Id);
				}
				builder.AddSeparator();
			}

			IcdConsole.PrintLine(eConsoleColor.White, builder.ToString());
		}

		private void PrintDestinationToEndpoints()
		{
			IcdConsole.PrintLine(eConsoleColor.Magenta, "Destination To Endpoints");

			TableBuilder builder = new TableBuilder("Destination Name", "Destination ID", "Endpoint");

			foreach (var mapping in m_DestinationToEndpoints)
			{
				if (mapping.Value.Count == 0)
				{
					builder.AddRow(mapping.Key.Name, mapping.Key.Id, "NO ENTRIES");
					builder.AddSeparator();
					continue;
				}

				builder.AddRow(mapping.Key.Name, mapping.Key.Id, mapping.Value.First());

				if (mapping.Value.Count == 1)
				{
					builder.AddSeparator();
					continue;
				}

				foreach (var value in mapping.Value.Skip(1))
				{
					builder.AddRow("", "", value);
				}
				builder.AddSeparator();
			}

			IcdConsole.PrintLine(eConsoleColor.White, builder.ToString());
		}

		private void PrintEndpointToDestinations()
		{
			IcdConsole.PrintLine(eConsoleColor.Magenta, "Endpoint To Destinations");

			TableBuilder builder = new TableBuilder("Endpoint", "Destination Name", "Destination Id");

			foreach (var mapping in m_EndpointToDestinations)
			{
				if (mapping.Value.Count == 0)
				{
					builder.AddRow(mapping.Key, "NO ENTRIES", "NO ENTRIES");
					builder.AddSeparator();
					continue;
				}

				builder.AddRow(mapping.Key, mapping.Value.First().Name, mapping.Value.First().Id);

				if (mapping.Value.Count == 1)
				{
					builder.AddSeparator();
					continue;
				}

				foreach (var value in mapping.Value.Skip(1))
				{
					builder.AddRow("", value.Name, value.Id);
				}
				builder.AddSeparator();
			}

			IcdConsole.PrintLine(eConsoleColor.White, builder.ToString());
		}

		private void PrintTransmittingMaps()
		{
			PrintSourceTransmitting();
			PrintEndpointTransmitting();
		}

		private void PrintSourceTransmitting()
		{
			IcdConsole.PrintLine(eConsoleColor.Magenta, "Source Transmitting");

			TableBuilder builder = new TableBuilder("Source Name", "Source Id", "Transmitting Types");

			foreach (var mapping in m_SourceTransmitting)
			{

				builder.AddRow(mapping.Key.Name, mapping.Key.Id, mapping.Value.ToString());
			}

			IcdConsole.PrintLine(eConsoleColor.White, builder.ToString());
		}

		private void PrintEndpointTransmitting()
		{
			IcdConsole.PrintLine(eConsoleColor.Magenta, "Source Endpoint Transmitting");

			TableBuilder builder = new TableBuilder("Endpoint", "Transmitting Types");

			foreach (var mapping in m_SourceEndpointTransmitting)
			{

				builder.AddRow(mapping.Key, mapping.Value.ToString());
			}

			IcdConsole.PrintLine(eConsoleColor.White, builder.ToString());
		}

		private void PrintDetectedMaps()
		{
			PrintSourceDetectedMap();
			PrintEndpointDetectedMap();
		}

		private void PrintSourceDetectedMap()
		{
			IcdConsole.PrintLine(eConsoleColor.Magenta, "Source Detected");

			TableBuilder builder = new TableBuilder("Source Name", "Source Id", "Detected Types");

			foreach (var mapping in m_SourceDetected)
			{
				builder.AddRow(mapping.Key.Name, mapping.Key.Id, mapping.Value.ToString());
			}

			IcdConsole.PrintLine(eConsoleColor.White, builder.ToString());
		}

		private void PrintEndpointDetectedMap()
		{
			IcdConsole.PrintLine(eConsoleColor.Magenta, "Source Endpoint Detected");

			TableBuilder builder = new TableBuilder("Endpoint", "Detected Types");

			foreach (var mapping in m_SourceEndpointDetected)
			{

				builder.AddRow(mapping.Key, mapping.Value.ToString());
			}

			IcdConsole.PrintLine(eConsoleColor.White, builder.ToString());
		}

		private void PrintDestinationEndpointActiveMap()
		{
			IcdConsole.PrintLine(eConsoleColor.Magenta, "Destination Endpoint Active");

			TableBuilder builder = new TableBuilder("Endpoint", "Active Types");

			foreach (var mapping in m_DestinationEndpointActive)
			{

				builder.AddRow(mapping.Key, mapping.Value.ToString());
			}

			IcdConsole.PrintLine(eConsoleColor.White, builder.ToString());
		}

		private void PrintRouteMaps()
		{
			PrintDestinationToSourceMap();
			PrintSourceToDestinationMap();
		}

		private void PrintDestinationToSourceMap()
		{
			IcdConsole.PrintLine(eConsoleColor.Magenta, "Destination To Source");

			TableBuilder builder = new TableBuilder("ConnectionType", "Route");

			foreach (var mapping in m_DestinationEndpointToSourceEndpointCache)
			{
				builder.AddHeader("Final Destination: ", mapping.Key.ToString());

				var dictionary = mapping.Value;

				foreach (eConnectionType type in mapping.Value.Keys)
				{
					if (dictionary[type].Count == 0)
					{
						builder.AddRow(type.ToString(), "NO ROUTE");
						builder.AddSeparator();
						continue;
					}

					builder.AddRow(type.ToString(), dictionary[type].First());
					foreach (EndpointInfo endpoint in dictionary[type].Skip(1))
					{
						builder.AddRow("", endpoint);
					}
					builder.AddSeparator();
				}
			}

			IcdConsole.PrintLine(eConsoleColor.White, builder.ToString());
		}

		private void PrintSourceToDestinationMap()
		{
			IcdConsole.PrintLine(eConsoleColor.Magenta, "Source To Destination");

			TableBuilder builder = new TableBuilder("ConnectionType", "Route");

			foreach (var mapping in m_SourceEndpointToDestinationEndpointCache)
			{
				builder.AddHeader("Initial Source: ", mapping.Key.ToString());

				var dictionary = mapping.Value;

				foreach (eConnectionType type in mapping.Value.Keys)
				{
					if (dictionary[type].Count == 0)
					{
						builder.AddRow(type.ToString(), "NO ROUTE");
						builder.AddSeparator();
						continue;
					}

					builder.AddRow(type.ToString(), dictionary[type].First());
					foreach (EndpointInfo endpoint in dictionary[type].Skip(1))
					{
						builder.AddRow("", endpoint);
					}
					builder.AddSeparator();
				}
			}

			IcdConsole.PrintLine(eConsoleColor.White, builder.ToString());
		}

		#endregion

		#region Console

		/// <summary>
		/// Gets the name of the node.
		/// </summary>
		public string ConsoleName { get { return "RoutingCache"; } }

		/// <summary>
		/// Gets the help information for the node.
		/// </summary>
		public string ConsoleHelp { get { return "The Routing Cache"; } }

		/// <summary>
		/// Gets the child console nodes.
		/// </summary>
		/// <returns></returns>
		public IEnumerable<IConsoleNodeBase> GetConsoleNodes()
		{
			return Enumerable.Empty<IConsoleNodeBase>();
		}

		/// <summary>
		/// Calls the delegate for each console status item.
		/// </summary>
		/// <param name="addRow"></param>
		public void BuildConsoleStatus(AddStatusRowDelegate addRow)
		{
			addRow.Invoke("Debug Mode:", m_DebugEnabled ? "Enabled" : "Disabled");
		}

		/// <summary>
		/// Gets the child console commands.
		/// </summary>
		/// <returns></returns>
		public IEnumerable<IConsoleCommand> GetConsoleCommands()
		{
			yield return new ConsoleCommand("PrintAllCaches", "Prints the contents of every cache", () => PrintAllCacheStates());
			yield return new ConsoleCommand("Rebuild", "Clears and Rebuilds the caches", () => RebuildCache());
			yield return new GenericConsoleCommand<bool>("Enable Debug", "Enables or disables cache printouts when they change", a => m_DebugEnabled = a);
		}

		#endregion
	}
}