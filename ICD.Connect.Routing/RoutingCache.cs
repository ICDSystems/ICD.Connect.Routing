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

namespace ICD.Connect.Routing
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

		private readonly SafeCriticalSection m_CacheSection;

		private bool m_DebugEnabled;

		#endregion

		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="routingGraph"></param>
		public RoutingCache(IRoutingGraph routingGraph)
		{
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
			if (!EnumUtils.HasSingleFlag(flag))
				throw new ArgumentException("type cannot have multiple flags", "flag");

			m_CacheSection.Enter();

			try
			{
				eConnectionType detected;
				return m_SourceDetected.TryGetValue(source, out detected) && detected.HasFlags(flag);
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
			if (!EnumUtils.HasSingleFlag(flag))
				throw new ArgumentException("type cannot have multiple flags", "flag");

			m_CacheSection.Enter();

			try
			{
				eConnectionType transmitting;
				return m_SourceTransmitting.TryGetValue(source, out transmitting) && transmitting.HasFlags(flag);
			}
			finally
			{
				m_CacheSection.Leave();
			}
		}

		#endregion

		#region Destination Sources

		/// <summary>
		/// Returns all of the SOURCES routed to the given DESTINATION ENDPOINT for the given connection type.
		/// </summary>
		[PublicAPI]
		public IEnumerable<ISource> GetSourcesForDestinationEndpoint(EndpointInfo destinationEndpoint, eConnectionType flag)
		{
			if (!EnumUtils.HasSingleFlag(flag))
				throw new ArgumentException("type cannot have multiple flags", "flag");

			m_CacheSection.Enter();

			try
			{
				Dictionary<eConnectionType, IcdHashSet<EndpointInfo>> cache;
				if (!m_DestinationEndpointToSourceEndpointCache.TryGetValue(destinationEndpoint, out cache))
					return Enumerable.Empty<ISource>();

				IcdHashSet<EndpointInfo> endpoints;
				if (!cache.TryGetValue(flag, out endpoints))
					return Enumerable.Empty<ISource>();

				return endpoints.SelectMany(endpoint =>
				{
					IcdHashSet<ISource> sources;
					return m_EndpointToSources.TryGetValue(endpoint, out sources)
							   ? sources
							   : Enumerable.Empty<ISource>();
				})
								.Distinct()
								.ToArray();
			}
			finally
			{
				m_CacheSection.Leave();
			}
		}

		/// <summary>
		/// Returns all of the SOURCES routed to the given DESTINATION for the given connection type.
		/// </summary>
		[PublicAPI]
		public IEnumerable<ISource> GetSourcesForDestination(IDestination destination, eConnectionType flag)
		{
			if (!EnumUtils.HasSingleFlag(flag))
				throw new ArgumentException("type cannot have multiple flags", "flag");

			m_CacheSection.Enter();

			try
			{
				IcdHashSet<EndpointInfo> endpoints;
				if (!m_DestinationToEndpoints.TryGetValue(destination, out endpoints))
					throw new ArgumentException("unknown or uncached destination", "destination");

				return endpoints.SelectMany(e => GetSourcesForDestinationEndpoint(e, flag))
								.Distinct()
								.ToArray();
			}
			finally
			{
				m_CacheSection.Leave();
			}
		}

		/// <summary>
		/// Returns all of the DESTINATIONS routed from the given SOURCE ENDPOINT for the given connection type.
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

				return endpoints.SelectMany(endpoint =>
				                            {
					                            IcdHashSet<IDestination> destinations;
					                            return m_EndpointToDestinations.TryGetValue(endpoint, out destinations)
						                                   ? destinations
						                                   : Enumerable.Empty<IDestination>();
				                            })
				                .Distinct()
				                .ToArray();
			}
			finally
			{
				m_CacheSection.Leave();
			}
		}

		/// <summary>
		/// Returns all of the DESTINATIONS routed from the given SOURCE for the given connection type.
		/// </summary>
		[PublicAPI]
		public IEnumerable<IDestination> GetDestinationsForSource(ISource source, eConnectionType flag)
		{
			if (!EnumUtils.HasSingleFlag(flag))
				throw new ArgumentException("type cannot have multiple flags", "flag");

			m_CacheSection.Enter();

			try
			{
				IcdHashSet<EndpointInfo> endpoints;
				if (!m_SourceToEndpoints.TryGetValue(source, out endpoints))
					return Enumerable.Empty<IDestination>();

				return endpoints.SelectMany(e => GetDestinationsForSourceEndpoint(e, flag))
								.Distinct()
								.ToArray();
			}
			finally
			{
				m_CacheSection.Leave();
			}
		}

		/// <summary>
		/// Returns all of the SOURCE ENDPOINTS routed to the given DESTINATION ENDPOINT for the given connection type.
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
				if (!cache.TryGetValue(flag, out result))
					return Enumerable.Empty<EndpointInfo>();

				return result;
			}
			finally
			{
				m_CacheSection.Leave();
			}
		}

		/// <summary>
		/// Returns all of the DESTINATION ENDPOINTS routed from the given SOURCE ENDPOINT for the given connection type.
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

				IcdHashSet<EndpointInfo> destinationEndpoints;
				if (!types.TryGetValue(flag, out destinationEndpoints))
					return Enumerable.Empty<EndpointInfo>();

				return destinationEndpoints;
			}
			finally
			{
				m_CacheSection.Leave();
			}
		}

		/// <summary>
		/// Returns all of the SOURCE ENDPOINTS routed from the given DESTINATION for the given connection type.
		/// </summary>
		[PublicAPI]
		public IEnumerable<EndpointInfo> GetSourceEndpointsForDestination(IDestination destination, eConnectionType flag)
		{
			if (!EnumUtils.HasSingleFlag(flag))
				throw new ArgumentException("type cannot have multiple flags", "flag");

			m_CacheSection.Enter();

			try
			{
				IcdHashSet<EndpointInfo> endpoints;
				if (!m_DestinationToEndpoints.TryGetValue(destination, out endpoints))
					return Enumerable.Empty<EndpointInfo>();

				return endpoints.SelectMany(e => GetSourceEndpointsForDestinationEndpoint(e, flag))
								.Distinct()
								.ToArray();
			}
			finally
			{
				m_CacheSection.Leave();
			}
		}

		/// <summary>
		/// Returns all of the DESTINATION ENDPOINTS routed from the given SOURCE for the given connection type.
		/// </summary>
		[PublicAPI]
		public IEnumerable<EndpointInfo> GetDestinationEndpointsForSource(ISource source, eConnectionType flag)
		{
			if (!EnumUtils.HasSingleFlag(flag))
				throw new ArgumentException("type cannot have multiple flags", "flag");

			m_CacheSection.Enter();

			try
			{
				IcdHashSet<EndpointInfo> endpoints;
				if (!m_SourceToEndpoints.TryGetValue(source, out endpoints))
					return Enumerable.Empty<EndpointInfo>();

				return endpoints.SelectMany(e => GetDestinationEndpointsForSourceEndpoint(e, flag))
				                .Distinct()
				                .ToArray();
			}
			finally
			{
				m_CacheSection.Leave();
			}
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

				return cache.Where(d => !inputActive || m_DestinationEndpointActive.GetDefault(d).HasFlag(flag))
				            .SelectMany(d => GetSourceEndpointsForDestinationEndpoint(d, flag))
				            .Distinct()
				            .Where(s => !signalDetected || m_SourceEndpointDetected.GetDefault(s).HasFlag(flag))
				            .ToArray();
			}
			finally
			{
				m_CacheSection.Leave();
			}
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

				return cache.Where(s => !inputActive || m_SourceEndpointTransmitting.GetDefault(s).HasFlag(flag))
				            .SelectMany(s => GetDestinationEndpointsForSourceEndpoint(s, flag))
				            .Distinct()
				            .Where(s => !signalDetected || m_SourceEndpointDetected.GetDefault(s).HasFlag(flag))
				            .ToArray();
			}
			finally
			{
				m_CacheSection.Leave();
			}
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
			m_CacheSection.Enter();

			try
			{
				return GetSourceEndpointsForDestination(destination, flag, signalDetected, inputActive)
					.SelectMany(e =>
					            {
						            IcdHashSet<ISource> sources;
						            return m_EndpointToSources.TryGetValue(e, out sources) ? sources : Enumerable.Empty<ISource>();
					            })
					.Distinct()
					.ToArray();
			}
			finally
			{
				m_CacheSection.Leave();
			}
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
			m_CacheSection.Enter();

			try
			{
				return GetDestinationEndpointsForSource(source, flag, signalDetected, inputActive)
					.SelectMany(e =>
					            {
						            IcdHashSet<IDestination> destinations;
						            return m_EndpointToDestinations.TryGetValue(e, out destinations)
							                   ? destinations
							                   : Enumerable.Empty<IDestination>();
					            })
					.Distinct()
					.ToArray();
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
		/// Initializes the source caches.
		/// </summary>
		private void InitializeSourceCaches()
		{
			m_CacheSection.Enter();

			try
			{
				IcdHashSet<ISource> sources = m_RoutingGraph.Sources.ToIcdHashSet();
				IcdHashSet<EndpointInfo> sourceEndpoints =
					sources.SelectMany(s => s.GetEndpoints()).ToIcdHashSet();

				foreach (EndpointInfo endpoint in sourceEndpoints)
				{
					EndpointInfo endpoint1 = endpoint;
					m_EndpointToSources.Add(endpoint, sources.Where(s => s.Contains(endpoint1)).ToIcdHashSet());
					m_SourceEndpointToDestinationEndpointCache.Add(endpoint,
					                                               new Dictionary<eConnectionType, IcdHashSet<EndpointInfo>>());

					foreach (eConnectionType flag in EnumUtils.GetFlagsExceptNone<eConnectionType>())
						m_SourceEndpointToDestinationEndpointCache[endpoint].Add(flag, new IcdHashSet<EndpointInfo>());
				}

				foreach (ISource source in sources)
					m_SourceToEndpoints.Add(source, source.GetEndpoints().ToIcdHashSet());

				foreach (EndpointInfo endpoint in sourceEndpoints)
					UpdateSourceEndpoint(endpoint);
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
				IcdHashSet<IDestination> destinations = m_RoutingGraph.Destinations.ToIcdHashSet();
				IcdHashSet<EndpointInfo> destinationEndpoints =
					destinations.SelectMany(s => s.GetEndpoints()).ToIcdHashSet();

				foreach (EndpointInfo endpoint in destinationEndpoints)
				{
					EndpointInfo endpoint1 = endpoint;
					m_EndpointToDestinations.Add(endpoint, destinations.Where(d => d.Contains(endpoint1)).ToIcdHashSet());
					m_DestinationEndpointToSourceEndpointCache.Add(endpoint,
					                                               new Dictionary<eConnectionType, IcdHashSet<EndpointInfo>>());

					foreach (eConnectionType flag in EnumUtils.GetFlagsExceptNone<eConnectionType>())
						m_DestinationEndpointToSourceEndpointCache[endpoint].Add(flag, new IcdHashSet<EndpointInfo>());
				}

				foreach (IDestination destination in destinations)
					m_DestinationToEndpoints.Add(destination, destination.GetEndpoints().ToIcdHashSet());

				foreach (EndpointInfo endpoint in destinationEndpoints)
					UpdateDestinationEndpoint(endpoint);
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
				InitializeDirectConnectionRoutes();
				InitializeExtendedRoutes();
			}
			finally
			{
				m_CacheSection.Leave();
			}
		}

		private void InitializeDirectConnectionRoutes()
		{
			m_CacheSection.Enter();

			try
			{
				foreach (Connection connection in m_RoutingGraph.Connections)
				{
					// Destination
					Dictionary<eConnectionType, IcdHashSet<EndpointInfo>> destinationCache;
					if (!m_DestinationEndpointToSourceEndpointCache.TryGetValue(connection.Destination, out destinationCache))
					{
						destinationCache = new Dictionary<eConnectionType, IcdHashSet<EndpointInfo>>();
						m_DestinationEndpointToSourceEndpointCache.Add(connection.Destination, destinationCache);
					}

					// Source
					Dictionary<eConnectionType, IcdHashSet<EndpointInfo>> sourceCache;
					if (!m_SourceEndpointToDestinationEndpointCache.TryGetValue(connection.Source, out sourceCache))
					{
						sourceCache = new Dictionary<eConnectionType, IcdHashSet<EndpointInfo>>();
						m_SourceEndpointToDestinationEndpointCache.Add(connection.Source, sourceCache);
					}

					foreach (eConnectionType flag in EnumUtils.GetFlagsExceptNone(connection.ConnectionType))
					{
						// Destination
						IcdHashSet<EndpointInfo> destinationEndpoints;
						if (!destinationCache.TryGetValue(flag, out destinationEndpoints))
						{
							destinationEndpoints = new IcdHashSet<EndpointInfo>();
							destinationCache.Add(flag, destinationEndpoints);
						}
						destinationEndpoints.Add(connection.Source);

						// Source
						IcdHashSet<EndpointInfo> sourceEndpoints;
						if (!sourceCache.TryGetValue(flag, out sourceEndpoints))
						{
							sourceEndpoints = new IcdHashSet<EndpointInfo>();
							sourceCache.Add(flag, sourceEndpoints);
						}
						sourceEndpoints.Add(connection.Destination);
					}
				}
			}
			finally
			{
				m_CacheSection.Leave();
			}
		}

		private void InitializeExtendedRoutes()
		{
			m_CacheSection.Enter();

			try
			{
				foreach (
					EndpointInfo destinationEndpoint in m_RoutingGraph.Destinations.SelectMany(d => d.GetEndpoints()).Distinct())
				{
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

						while (process.Count > 0)
						{
							EndpointInfo currentDestinationEndpoint = process.Dequeue();

							if (!m_RoutingGraph.InputActive(currentDestinationEndpoint, flag))
								continue;

							Connection connection = m_RoutingGraph.Connections.GetInputConnection(currentDestinationEndpoint);
							if (connection == null)
								continue;

							EndpointInfo currentSourceEndpoint = connection.Source;
							activeRouteSourceEndpoints.Add(currentSourceEndpoint);
							activeRouteDestinationEndpoints.Add(currentDestinationEndpoint);

							IRouteControl control = m_RoutingGraph.GetControl<IRouteControl>(currentSourceEndpoint);

							IRouteMidpointControl midControl = control as IRouteMidpointControl;
							if (midControl == null)
								continue;

							foreach (ConnectorInfo activeInput in midControl.GetInputs(currentSourceEndpoint.Address, flag))
							{
								process.Enqueue(new EndpointInfo(currentSourceEndpoint.Device, currentSourceEndpoint.Control,
								                                 activeInput.Address));
							}
						}

						if (!activeRouteSourceEndpoints.Any() || !activeRouteDestinationEndpoints.Any())
							return;

						if (!m_DestinationEndpointToSourceEndpointCache.ContainsKey(destinationEndpoint))
							m_DestinationEndpointToSourceEndpointCache[destinationEndpoint] =
								new Dictionary<eConnectionType, IcdHashSet<EndpointInfo>>();

						if (!m_DestinationEndpointToSourceEndpointCache[destinationEndpoint].ContainsKey(flag))
							m_DestinationEndpointToSourceEndpointCache[destinationEndpoint].Add(flag, new IcdHashSet<EndpointInfo>());

						EndpointInfo sourceKey = activeRouteSourceEndpoints.Last();

						if (!m_SourceEndpointToDestinationEndpointCache.ContainsKey(sourceKey))
							m_SourceEndpointToDestinationEndpointCache[sourceKey] =
								new Dictionary<eConnectionType, IcdHashSet<EndpointInfo>>();

						if (!m_SourceEndpointToDestinationEndpointCache[sourceKey].ContainsKey(flag))
							m_SourceEndpointToDestinationEndpointCache[sourceKey].Add(flag, new IcdHashSet<EndpointInfo>());

						m_DestinationEndpointToSourceEndpointCache[destinationEndpoint][flag].AddRange(activeRouteSourceEndpoints);
						m_SourceEndpointToDestinationEndpointCache[activeRouteSourceEndpoints.Last()][flag].AddRange(
						                                                                                             activeRouteDestinationEndpoints);
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

		private void UpdateSourceTransmissionState(ISource source)
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
					return;

				m_SourceTransmitting[source] = flags;

				if (m_DebugEnabled)
					PrintSourceTransmitting();
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

				if (m_EndpointToSources.ContainsKey(endpoint))
				{
					foreach (ISource source in m_EndpointToSources[endpoint])
						UpdateSourceDetectionState(source);
				}

				return true;
			}
			finally
			{
				m_CacheSection.Leave();
			}
		}

		private void UpdateSourceDetectionState(ISource source)
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
					return;

				m_SourceDetected[source] = flags;

				if (m_DebugEnabled)
					PrintSourceDetectedMap();
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

			IcdHashSet<EndpointInfo> oldSourceEndpoints =
				args.OldSourceEndpoints as IcdHashSet<EndpointInfo> ?? args.OldSourceEndpoints.ToIcdHashSet();
			IcdHashSet<EndpointInfo> newSourceEndpoints =
				args.NewSourceEndpoints as IcdHashSet<EndpointInfo> ?? args.NewSourceEndpoints.ToIcdHashSet();
			IcdHashSet<EndpointInfo> destinationEndpoints =
				args.DestinationEndpoints as IcdHashSet<EndpointInfo> ?? args.DestinationEndpoints.ToIcdHashSet();

			// No change
			if (destinationEndpoints.Count == 0)
				return;

			// No change
			if (oldSourceEndpoints.SetEquals(newSourceEndpoints))
				return;

			eConnectionType typeChange = eConnectionType.None;

			m_CacheSection.Enter();

			try
			{
				foreach (eConnectionType flag in EnumUtils.GetFlagsExceptNone(args.Type))
				{
					bool change = false;

					if (oldSourceEndpoints.Count > 0)
					{
						change |= RemoveOldValuesFromSourceCache(oldSourceEndpoints, destinationEndpoints, flag);
						change |= RemoveOldValuesFromDestinationCache(oldSourceEndpoints, destinationEndpoints, flag);
					}

					if (newSourceEndpoints.Count > 0)
					{
						change |= AddNewValuesToSourceCache(newSourceEndpoints, destinationEndpoints, flag);
						change |= AddNewValuesToDestinationCache(newSourceEndpoints, destinationEndpoints, flag);
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

			OnEndpointRouteChanged.Raise(this, new EndpointRouteChangedEventArgs());
			OnSourceDestinationRouteChanged.Raise(this, new SourceDestinationRouteChangedEventArgs(typeChange));
		}

		private void RoutingGraphOnSourceTransmissionStateChanged(object sender, EndpointStateEventArgs args)
		{
			m_CacheSection.Enter();

			try
			{
				// We don't care about transmission state changes unless it's a source
				if (!m_EndpointToSources.ContainsKey(args.Endpoint))
					return;

				bool endpointChanged = UpdateSourceEndpointTransmissionState(args.Endpoint, args.Type, args.State);
				if (endpointChanged)
					OnTransmissionStateChanged.Raise(this, new CacheStateChangedEventArgs(new[] { args.Endpoint },
																						  args.Type,
																						  args.State));
			}
			finally
			{
				m_CacheSection.Leave();
			}
		}

		private void RoutingGraphOnSourceDetectionStateChanged(object sender, EndpointStateEventArgs args)
		{
			m_CacheSection.Enter();

			try
			{
				// We don't care about detection state changes unless it's a source
				if (!m_EndpointToSources.ContainsKey(args.Endpoint))
					return;

				bool endpointChanged = UpdateSourceEndpointDetectionState(args.Endpoint, args.Type, args.State);
				if (endpointChanged)
					OnDetectionStateChanged.Raise(this, new CacheStateChangedEventArgs(new[] { args.Endpoint },
																					   args.Type,
																					   args.State));
			}
			finally
			{
				m_CacheSection.Leave();
			}
		}

		private void RoutingGraphOnDestinationInputActiveStateChanged(object sender, EndpointStateEventArgs args)
		{
			m_CacheSection.Enter();

			try
			{
				// We don't care about active state changes unless it's a destination
				if (!m_EndpointToDestinations.ContainsKey(args.Endpoint))
					return;

				bool endpointChanged = UpdateDestinationEndpointInputActiveState(args.Endpoint, args.Type, args.State);
				if (endpointChanged)
					OnDestinationEndpointActiveChanged.Raise(this, new CacheStateChangedEventArgs(new[] { args.Endpoint },
																								  args.Type,
																								  args.State));
			}
			finally
			{
				m_CacheSection.Leave();
			}
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