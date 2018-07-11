using System;
using System.Collections.Generic;
using System.Linq;
using ICD.Common.Utils;
using ICD.Common.Utils.Collections;
using ICD.Common.Utils.Extensions;
using ICD.Connect.Routing.Connections;
using ICD.Connect.Routing.Controls;
using ICD.Connect.Routing.Endpoints;
using ICD.Connect.Routing.Endpoints.Destinations;
using ICD.Connect.Routing.Endpoints.Sources;
using ICD.Connect.Routing.EventArguments;
using ICD.Connect.Routing.RoutingGraphs;

namespace ICD.Connect.Routing
{
	public sealed class RoutingCache : IDisposable
	{
		#region Events

		public event EventHandler<SourceStateChangedEventArgs> OnSourceTransmissionStateChanged;
		public event EventHandler<SourceStateChangedEventArgs> OnSourceDetectionStateChanged;
		public event EventHandler<EndpointStateChangedEventArgs> OnEndpointTransmissionStateChanged;
		public event EventHandler<EndpointStateChangedEventArgs> OnEndpointDetectionStateChanged;
		public event EventHandler<EndpointRouteChangedEventArgs> OnEndpointRouteChanged;
		public event EventHandler<SourceDestinationRouteChangedEventArgs> OnSourceDestinationRouteChanged;

		/// <summary>
		/// Raised when a destination input becomes active/inactive.
		/// </summary>
		public event EventHandler<EndpointStateChangedEventArgs> OnDestinationEndpointActiveChanged; 

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

		private readonly Dictionary<EndpointInfo, Dictionary<eConnectionType, IcdHashSet<EndpointInfo>>> m_DestinationToSourceCache;
		private readonly Dictionary<EndpointInfo, Dictionary<eConnectionType, IcdHashSet<EndpointInfo>>> m_SourceToDestinationCache;

		#endregion

		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="routingGraph"></param>
		public RoutingCache(IRoutingGraph routingGraph)
		{
			m_RoutingGraph = routingGraph;
			Subscribe(m_RoutingGraph);

			m_SourceToEndpoints = new Dictionary<ISource, IcdHashSet<EndpointInfo>>();
			m_EndpointToSources = new Dictionary<EndpointInfo, IcdHashSet<ISource>>();

			m_DestinationToEndpoints = new Dictionary<IDestination, IcdHashSet<EndpointInfo>>();
			m_EndpointToDestinations = new Dictionary<EndpointInfo, IcdHashSet<IDestination>>();

			m_SourceTransmitting = new Dictionary<ISource, eConnectionType>();
			m_SourceDetected = new Dictionary<ISource, eConnectionType>();
			m_SourceEndpointTransmitting = new Dictionary<EndpointInfo, eConnectionType>();
			m_SourceEndpointDetected = new Dictionary<EndpointInfo, eConnectionType>();
			m_DestinationEndpointActive = new Dictionary<EndpointInfo, eConnectionType>();

			m_DestinationToSourceCache = new Dictionary<EndpointInfo, Dictionary<eConnectionType, IcdHashSet<EndpointInfo>>>();
			m_SourceToDestinationCache = new Dictionary<EndpointInfo, Dictionary<eConnectionType, IcdHashSet<EndpointInfo>>>();

			RebuildCache();
		}

		/// <summary>
		/// Release resources.
		/// </summary>
		public void Dispose()
		{
			OnSourceTransmissionStateChanged = null;
			OnSourceDetectionStateChanged = null;
			OnEndpointTransmissionStateChanged = null;
			OnEndpointDetectionStateChanged = null;
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
			ClearCache();

			InitializeSourceCaches();
			InitializeDestinationCaches();
			InitializeRoutes();
		}

		/// <summary>
		/// Clears all of the cached states.
		/// </summary>
		public void ClearCache()
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

			m_DestinationToSourceCache.Clear();
			m_SourceToDestinationCache.Clear();
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

			return m_SourceDetected.ContainsKey(source) && m_SourceDetected[source].HasFlags(flag);
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

			return m_SourceTransmitting.ContainsKey(source) && m_SourceTransmitting[source].HasFlags(flag);
		}

		#endregion

		#region Destination Sources

		/// <summary>
		/// Returns all of the sources actively routed to the given destitation endpoint for the given connection type.
		/// </summary>
		/// <param name="destinationEndpoint"></param>
		/// <param name="flag"></param>
		/// <returns></returns>
		public IEnumerable<ISource> GetSourcesForDestination(EndpointInfo destinationEndpoint, eConnectionType flag)
		{
			if (!EnumUtils.HasSingleFlag(flag))
				throw new ArgumentException("type cannot have multiple flags", "flag");

			if (!m_DestinationToSourceCache.ContainsKey(destinationEndpoint))
				return Enumerable.Empty<ISource>();

			IcdHashSet<ISource> sources = new IcdHashSet<ISource>();

			foreach (EndpointInfo endpoint in m_DestinationToSourceCache[destinationEndpoint][flag])
				sources.AddRange(m_EndpointToSources[endpoint]);

			return sources;
		}

		/// <summary>
		/// Returns all of the source endpoints actively routed to the given destitation endpoint for the given connection type.
		/// </summary>
		/// <param name="destinationEndpoint"></param>
		/// <param name="flag"></param>
		/// <returns></returns>
		public IEnumerable<EndpointInfo> GetSourceEndpointsForDestination(EndpointInfo destinationEndpoint,
		                                                                  eConnectionType flag)
		{
			if (!EnumUtils.HasSingleFlag(flag))
				throw new ArgumentException("type cannot have multiple flags", "flag");

			Dictionary<eConnectionType, IcdHashSet<EndpointInfo>> cache;
			if (!m_DestinationToSourceCache.TryGetValue(destinationEndpoint, out cache))
				return Enumerable.Empty<EndpointInfo>();

			IcdHashSet<EndpointInfo> result;
			if (!cache.TryGetValue(flag, out result))
				return Enumerable.Empty<EndpointInfo>();

			return result;
		}

		/// <summary>
		/// Gets all of the source endpoints currently routed to the given destination for the given flag.
		/// </summary>
		/// <param name="destination"></param>
		/// <param name="flag"></param>
		/// <param name="signalDetected">When true only return where the source is detected.</param>
		/// <param name="inputActive">When true only return for active inputs.</param>
		/// <returns></returns>
		public IEnumerable<EndpointInfo> GetSourceEndpointsForDestination(IDestination destination, eConnectionType flag,
		                                                                  bool signalDetected, bool inputActive)
		{
			if (destination == null)
				throw new ArgumentNullException("destination");

			if (!EnumUtils.HasSingleFlag(flag))
				throw new ArgumentException("type cannot have multiple flags", "flag");

			return destination.GetEndpoints()
			                  .Where(d => !inputActive || m_DestinationEndpointActive.GetDefault(d).HasFlag(flag))
			                  .SelectMany(d => GetSourceEndpointsForDestination(d, flag))
							  .Distinct()
							  .Where(s => !signalDetected || m_SourceEndpointDetected.GetDefault(s).HasFlag(flag));
		}

		/// <summary>
		/// Returns all of the destination endpoints actively routed from the given source endpoint for the given connection type.
		/// </summary>
		/// <param name="sourceEndpoint"></param>
		/// <param name="flag"></param>
		/// <returns></returns>
		public IEnumerable<EndpointInfo> GetDestinationEndpointsForSource(EndpointInfo sourceEndpoint,
																		  eConnectionType flag)
		{
			if (!EnumUtils.HasSingleFlag(flag))
				throw new ArgumentException("type cannot have multiple flags", "flag");

			if (!m_SourceToDestinationCache.ContainsKey(sourceEndpoint))
				return Enumerable.Empty<EndpointInfo>();

			return m_SourceToDestinationCache[sourceEndpoint][flag];
		}

		/// <summary>
		/// Gets all of the destination endpoints currently routed from the given source for the given flag.
		/// </summary>
		/// <param name="source"></param>
		/// <param name="flag"></param>
		/// <param name="signalDetected">When true only return where the source is detected.</param>
		/// <param name="inputActive">When true only return for active inputs.</param>
		/// <returns></returns>
		public IEnumerable<EndpointInfo> GetDestinationEndpointsForSource(ISource source, eConnectionType flag,
																		  bool signalDetected, bool inputActive)
		{
			if (source == null)
				throw new ArgumentNullException("source");

			if (!EnumUtils.HasSingleFlag(flag))
				throw new ArgumentException("type cannot have multiple flags", "flag");

			return source.GetEndpoints()
							  .Where(s => !inputActive || m_SourceEndpointTransmitting.GetDefault(s).HasFlag(flag))
							  .SelectMany(s => GetDestinationEndpointsForSource(s, flag))
							  .Distinct()
							  .Where(s => !signalDetected || m_SourceEndpointDetected.GetDefault(s).HasFlag(flag));
		}

		public IEnumerable<IDestination> GetDestinationsForSource(ISource source, eConnectionType flag, bool signalDetected,
		                                                         bool inputActive)
		{
			return
				GetDestinationEndpointsForSource(source, flag, signalDetected, inputActive)
					.SelectMany(e => m_EndpointToDestinations[e]);
		}

		#endregion

		#endregion

		#region Private Methods

		/// <summary>
		/// Initializes the source caches.
		/// </summary>
		private void InitializeSourceCaches()
		{
			IcdHashSet<ISource> sources = m_RoutingGraph.Sources.ToIcdHashSet();
			IcdHashSet<EndpointInfo> sourceEndpoints =
				sources.SelectMany(s => s.GetEndpoints()).ToIcdHashSet();

			foreach (EndpointInfo endpoint in sourceEndpoints)
			{
				EndpointInfo endpoint1 = endpoint;
				m_EndpointToSources.Add(endpoint, sources.Where(s => s.Contains(endpoint1)).ToIcdHashSet());
				m_SourceToDestinationCache.Add(endpoint, new Dictionary<eConnectionType, IcdHashSet<EndpointInfo>>());
				foreach (var flag in EnumUtils.GetFlagsExceptNone<eConnectionType>())
				{
					m_SourceToDestinationCache[endpoint].Add(flag, new IcdHashSet<EndpointInfo>());
				}
			}

			foreach (ISource source in sources)
				m_SourceToEndpoints.Add(source, source.GetEndpoints().ToIcdHashSet());

			foreach (EndpointInfo endpoint in sourceEndpoints)
				UpdateSourceEndpoint(endpoint);
		}

		/// <summary>
		/// Initializes the destination caches.
		/// </summary>
		private void InitializeDestinationCaches()
		{
			IcdHashSet<IDestination> destinations = m_RoutingGraph.Destinations.ToIcdHashSet();
			IcdHashSet<EndpointInfo> destinationEndpoints =
				destinations.SelectMany(s => s.GetEndpoints()).ToIcdHashSet();

			foreach (EndpointInfo endpoint in destinationEndpoints)
			{
				EndpointInfo endpoint1 = endpoint;
				m_EndpointToDestinations.Add(endpoint, destinations.Where(d => d.Contains(endpoint1)).ToIcdHashSet());
				m_DestinationToSourceCache.Add(endpoint, new Dictionary<eConnectionType, IcdHashSet<EndpointInfo>>());
				foreach (var flag in EnumUtils.GetFlagsExceptNone<eConnectionType>())
				{
					m_DestinationToSourceCache[endpoint].Add(flag, new IcdHashSet<EndpointInfo>());
				}
			}

			foreach (IDestination destination in destinations)
				m_DestinationToEndpoints.Add(destination, destination.GetEndpoints().ToIcdHashSet());

			foreach (EndpointInfo endpoint in destinationEndpoints)
				UpdateDestinationEndpoint(endpoint);
		}

		/// <summary>
		/// Initializes the route caches.
		/// </summary>
		private void InitializeRoutes()
		{
			InitializeDirectConnectionRoutes();
			InitializeExtendedRoutes();
		}

		private void InitializeDirectConnectionRoutes()
		{
			foreach (Connection connection in m_RoutingGraph.Connections)
			{
				foreach (var flag in EnumUtils.GetFlagsExceptNone(connection.ConnectionType))
				{
					if (!m_DestinationToSourceCache.ContainsKey(connection.Destination))
						m_DestinationToSourceCache[connection.Destination] = new Dictionary<eConnectionType, IcdHashSet<EndpointInfo>>();

					if(!m_DestinationToSourceCache[connection.Destination].ContainsKey(flag))
						m_DestinationToSourceCache[connection.Destination].Add(flag, new IcdHashSet<EndpointInfo>());

					m_DestinationToSourceCache[connection.Destination][flag].Add(connection.Source);

					if (!m_SourceToDestinationCache.ContainsKey(connection.Source))
						m_SourceToDestinationCache[connection.Source] = new Dictionary<eConnectionType, IcdHashSet<EndpointInfo>>();

					if(!m_SourceToDestinationCache[connection.Source].ContainsKey(flag))
						m_SourceToDestinationCache[connection.Source].Add(flag, new IcdHashSet<EndpointInfo>());

					m_SourceToDestinationCache[connection.Source][flag].Add(connection.Destination);
				}
			}
		}

		private void InitializeExtendedRoutes()
		{
			foreach(EndpointInfo destinationEndpoint in m_RoutingGraph.Destinations.SelectMany(d => d.GetEndpoints()).Distinct())
			{
				IRouteDestinationControl finalDestinationControl = 
					m_RoutingGraph.GetDestinationControl(destinationEndpoint.Device, destinationEndpoint.Control);

				if (!finalDestinationControl.ContainsInput(destinationEndpoint.Address))
					continue;

				ConnectorInfo connector = finalDestinationControl.GetInput(destinationEndpoint.Address);
				
				foreach (eConnectionType type in EnumUtils.GetFlagsExceptNone(connector.ConnectionType))
				{
					IcdHashSet<EndpointInfo> activeRouteSourceEndpoints = new IcdHashSet<EndpointInfo>();
					IcdHashSet<EndpointInfo> activeRouteDestinationEndpoints = new IcdHashSet<EndpointInfo>();
					Queue<EndpointInfo> process = new Queue<EndpointInfo>();
					process.Enqueue(destinationEndpoint);

					while (process.Count > 0)
					{
						EndpointInfo currentDestinationEndpoint = process.Dequeue();

						if(!m_RoutingGraph.InputActive(currentDestinationEndpoint, type))
							continue;

						Connection connection = m_RoutingGraph.Connections.GetInputConnection(currentDestinationEndpoint);

						if(connection == null)
							continue;

						EndpointInfo currentSourceEndpoint = connection.Source;
						activeRouteSourceEndpoints.Add(currentSourceEndpoint);
						activeRouteDestinationEndpoints.Add(currentDestinationEndpoint);

						IRouteControl control = m_RoutingGraph.GetControl<IRouteControl>(currentSourceEndpoint);

						IRouteMidpointControl midControl = control as IRouteMidpointControl;
						if(midControl == null)
							continue;

						foreach (ConnectorInfo activeInput in midControl.GetInputs(currentSourceEndpoint.Address, type))
						{
							process.Enqueue(new EndpointInfo(currentSourceEndpoint.Device, currentSourceEndpoint.Control, activeInput.Address));
						}
					}

					if (!activeRouteSourceEndpoints.Any() || !activeRouteDestinationEndpoints.Any())
						return;

					m_DestinationToSourceCache[destinationEndpoint][type].AddRange(activeRouteSourceEndpoints);
					m_SourceToDestinationCache[activeRouteSourceEndpoints.Last()][type].AddRange(activeRouteDestinationEndpoints);
				}
			}
		}

		private void UpdateSourceEndpoint(EndpointInfo endpoint)
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

		private void UpdateDestinationEndpoint(EndpointInfo endpoint)
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

		private void AddDestinationToDestinationToSourceCache(EndpointInfo destinationEndpoint)
		{
			if (m_DestinationToSourceCache.ContainsKey(destinationEndpoint))
				return;

			m_DestinationToSourceCache.Add(destinationEndpoint, new Dictionary<eConnectionType, IcdHashSet<EndpointInfo>>());

			foreach (eConnectionType flag in EnumUtils.GetFlagsExceptNone<eConnectionType>())
				m_DestinationToSourceCache[destinationEndpoint].Add(flag, new IcdHashSet<EndpointInfo>());
		}

		private void AddSourceToSourceToDestinationCache(EndpointInfo sourceEndpoint)
		{
			if (m_SourceToDestinationCache.ContainsKey(sourceEndpoint))
				return;

			m_SourceToDestinationCache.Add(sourceEndpoint, new Dictionary<eConnectionType, IcdHashSet<EndpointInfo>>());

			foreach (eConnectionType flag in EnumUtils.GetFlagsExceptNone<eConnectionType>())
				m_SourceToDestinationCache[sourceEndpoint].Add(flag, new IcdHashSet<EndpointInfo>());
		}

		private void UpdateSourceEndpointTransmissionState(EndpointInfo endpoint, eConnectionType type, bool state)
		{
			eConnectionType oldFlags = m_SourceEndpointTransmitting.GetDefault(endpoint);
			eConnectionType newFlags = oldFlags;

			if (state)
				newFlags |= type;
			else
				newFlags &= ~type;

			if (newFlags == oldFlags)
				return;

			m_SourceEndpointTransmitting[endpoint] = newFlags;

			if (m_EndpointToSources.ContainsKey(endpoint))
			{
				foreach (ISource source in m_EndpointToSources[endpoint])
					UpdateSourceTransmissionState(source);
			}

			OnEndpointTransmissionStateChanged.Raise(this, new EndpointStateChangedEventArgs(endpoint, type, state));
		}

		private void UpdateSourceTransmissionState(ISource source)
		{
			eConnectionType flags = m_SourceToEndpoints.GetDefault(source)
				.Aggregate(eConnectionType.None,
				           (current, endpoint) => current | m_SourceEndpointTransmitting.GetDefault(endpoint));

			eConnectionType oldFlags = m_SourceTransmitting.GetDefault(source);
			
			if (flags == oldFlags)
				return;

			m_SourceTransmitting[source] = flags;

			foreach (eConnectionType flag in EnumUtils.GetFlagsExceptNone<eConnectionType>())
			{
				if (oldFlags.HasFlag(flag) && !flags.HasFlag(flag))
					OnSourceTransmissionStateChanged.Raise(this, new SourceStateChangedEventArgs(source, flag, false));

				else if (!oldFlags.HasFlag(flag) && flags.HasFlag(flag))
					OnSourceTransmissionStateChanged.Raise(this, new SourceStateChangedEventArgs(source, flag, true));
			}
		}

		private void UpdateSourceEndpointDetectionState(EndpointInfo endpoint, eConnectionType type, bool state)
		{
			eConnectionType oldFlags = m_SourceEndpointDetected.GetDefault(endpoint);
			eConnectionType newFlags = oldFlags;

			if (state)
				newFlags |= type;
			else
				newFlags &= ~type;

			if (newFlags == oldFlags)
				return;

			m_SourceEndpointDetected[endpoint] = newFlags;

			if (m_EndpointToSources.ContainsKey(endpoint))
			{
				foreach (ISource source in m_EndpointToSources[endpoint])
					UpdateSourceDetectionState(source);
			}

			OnEndpointDetectionStateChanged.Raise(this, new EndpointStateChangedEventArgs(endpoint, type, state));
		}

		private void UpdateSourceDetectionState(ISource source)
		{
			eConnectionType flags = m_SourceToEndpoints.GetDefault(source)
				.Aggregate(eConnectionType.None,
						   (current, endpoint) => current | m_SourceEndpointDetected.GetDefault(endpoint));

			eConnectionType oldFlags = m_SourceDetected.GetDefault(source);

			if (flags == oldFlags)
				return;

			m_SourceDetected[source] = flags;

			foreach (eConnectionType flag in EnumUtils.GetFlagsExceptNone<eConnectionType>())
			{
				if (oldFlags.HasFlag(flag) && !flags.HasFlag(flag))
					OnSourceDetectionStateChanged.Raise(this, new SourceStateChangedEventArgs(source, flag, false));
				else if (!oldFlags.HasFlag(flag) && flags.HasFlag(flag))
					OnSourceDetectionStateChanged.Raise(this, new SourceStateChangedEventArgs(source, flag, true));
			}
		}

		private void UpdateDestinationEndpointInputActiveState(EndpointInfo endpoint, eConnectionType type, bool state)
		{
			eConnectionType oldFlags = m_DestinationEndpointActive.GetDefault(endpoint);
			eConnectionType newFlags = oldFlags;

			if (state)
				newFlags |= type;
			else
				newFlags &= ~type;

			if (newFlags == oldFlags)
				return;

			m_DestinationEndpointActive[endpoint] = newFlags;

			OnDestinationEndpointActiveChanged.Raise(this, new EndpointStateChangedEventArgs(endpoint, type, state));
		}

		private void RemoveOldValuesFromDestinationCache(IcdHashSet<EndpointInfo> oldSourceEndpoints,
														 IEnumerable<EndpointInfo> destinations,
														 eConnectionType type)
		{
			if (!EnumUtils.HasSingleFlag(type))
				throw new ArgumentException("Type has multiple flags", "type");

			foreach (EndpointInfo destination in destinations.Where(destination => m_DestinationToSourceCache.ContainsKey(destination)))
			{
				foreach (EndpointInfo endpointToRemove in oldSourceEndpoints)
					m_DestinationToSourceCache[destination][type].Remove(endpointToRemove);
			}
		}

		private void AddNewValuesToDestinationCache(IcdHashSet<EndpointInfo> newSourceEndpoints,
													IEnumerable<EndpointInfo> destinations,
													eConnectionType type)
		{
			if (newSourceEndpoints == null)
				throw new ArgumentNullException("newSourceEndpoints");

			if (destinations == null)
				throw new ArgumentNullException("destinations");

			if (!EnumUtils.HasSingleFlag(type))
				throw new ArgumentException("Type has multiple flags", "type");

			foreach (EndpointInfo destination in destinations)
			{
				if (!m_DestinationToSourceCache.ContainsKey(destination))
					AddDestinationToDestinationToSourceCache(destination);

				foreach (EndpointInfo endpointToAdd in newSourceEndpoints)
					m_DestinationToSourceCache[destination][type].Add(endpointToAdd);
			}
		}

		private void RemoveOldValuesFromSourceCache(IEnumerable<EndpointInfo> oldSourceEndpoints,
													IcdHashSet<EndpointInfo> destinations,
													eConnectionType type)
		{
			if (oldSourceEndpoints == null)
				throw new ArgumentNullException("oldSourceEndpoints");

			if (destinations == null)
				throw new ArgumentNullException("destinations");

			if (!EnumUtils.HasSingleFlag(type))
				throw new ArgumentException("Type has multiple flags", "type");

			foreach (EndpointInfo source in oldSourceEndpoints)
			{
				if (!m_SourceToDestinationCache.ContainsKey(source))
					continue;

				if (!m_SourceToDestinationCache[source].ContainsKey(type))
					continue;

				foreach (EndpointInfo endpointToRemove in destinations)
					m_SourceToDestinationCache[source][type].Remove(endpointToRemove);
			}
		}

		private void AddNewValuesToSourceCache(IEnumerable<EndpointInfo> newSourceEndpoints,
											   IcdHashSet<EndpointInfo> destinations,
											   eConnectionType type)
		{
			if (!EnumUtils.HasSingleFlag(type))
				throw new ArgumentException("Type has multiple flags", "type");

			foreach (EndpointInfo source in newSourceEndpoints)
			{
				if (!m_SourceToDestinationCache.ContainsKey(source))
					AddSourceToSourceToDestinationCache(source);

				if (!m_SourceToDestinationCache[source].ContainsKey(type))
					m_SourceToDestinationCache[source][type] = new IcdHashSet<EndpointInfo>();

				foreach (EndpointInfo endpointToAdd in destinations)
					m_SourceToDestinationCache[source][type].Add(endpointToAdd);
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
			IcdHashSet<EndpointInfo> oldSourceEndpoints = args.OldSourceEndpoints.ToIcdHashSet();
			IcdHashSet<EndpointInfo> newSourceEndpoints = args.NewSourceEndpoints.ToIcdHashSet();
			IcdHashSet<EndpointInfo> destinationEndpoints = args.DestinationEndpoints.ToIcdHashSet();

			foreach (eConnectionType flag in EnumUtils.GetFlagsExceptNone(args.Type))
			{
				RemoveOldValuesFromSourceCache(oldSourceEndpoints, destinationEndpoints, flag);
				AddNewValuesToSourceCache(newSourceEndpoints, destinationEndpoints, flag);

				if (oldSourceEndpoints.SetEquals(newSourceEndpoints))
					continue;

				RemoveOldValuesFromDestinationCache(oldSourceEndpoints, destinationEndpoints, flag);
				AddNewValuesToDestinationCache(newSourceEndpoints, destinationEndpoints, flag);

				IcdHashSet<IDestination> newRouteDestinations = new IcdHashSet<IDestination>();
				IcdHashSet<ISource> newRouteSources = new IcdHashSet<ISource>();

				foreach (EndpointInfo destinationEndpoint in destinationEndpoints)
				{
					if (m_EndpointToDestinations.ContainsKey(destinationEndpoint))
						newRouteDestinations.AddRange(m_EndpointToDestinations[destinationEndpoint]);
					
					OnEndpointRouteChanged.Raise(this,
					                                          new EndpointRouteChangedEventArgs(
						                                          destinationEndpoint,
						                                          args.Type,
						                                          m_DestinationToSourceCache[destinationEndpoint][flag]));
				}

				foreach (EndpointInfo sourceEndpoint in newSourceEndpoints.Where(sourceEndpoint => m_EndpointToSources.ContainsKey(sourceEndpoint)))
					newRouteSources.AddRange(m_EndpointToSources[sourceEndpoint]);

				OnSourceDestinationRouteChanged.Raise(this, new SourceDestinationRouteChangedEventArgs(newRouteSources, newRouteDestinations, flag));
			}
		}

		private void RoutingGraphOnSourceTransmissionStateChanged(object sender, EndpointStateEventArgs args)
		{
			if (!m_EndpointToSources.ContainsKey(args.Endpoint))
				UpdateSourceEndpoint(args.Endpoint);

			UpdateSourceEndpointTransmissionState(args.Endpoint, args.Type, args.State);
		}

		private void RoutingGraphOnSourceDetectionStateChanged(object sender, EndpointStateEventArgs args)
		{
			if (!m_EndpointToSources.ContainsKey(args.Endpoint))
				UpdateSourceEndpoint(args.Endpoint);

			UpdateSourceEndpointDetectionState(args.Endpoint, args.Type, args.State);
		}

		private void RoutingGraphOnDestinationInputActiveStateChanged(object sender, EndpointStateEventArgs args)
		{
			if (!m_EndpointToDestinations.ContainsKey(args.Endpoint))
				UpdateDestinationEndpoint(args.Endpoint);

			UpdateDestinationEndpointInputActiveState(args.Endpoint, args.Type, args.State);
		}

		#endregion

		#region Debug Stuff



		#endregion
	}

	public sealed class SourceStateChangedEventArgs : EventArgs
	{
		public ISource Source { get; private set; }

		public eConnectionType Type { get; private set; }

		public bool State { get; private set; }

		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="source"></param>
		/// <param name="type"></param>
		/// <param name="state"></param>
		public SourceStateChangedEventArgs(ISource source, eConnectionType type, bool state)
		{
			Source = source;
			Type = type;
			State = state;
		}

		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="args"></param>
		public SourceStateChangedEventArgs(SourceStateChangedEventArgs args)
			: this(args.Source, args.Type, args.State)
		{
		}
	}

	public sealed class EndpointStateChangedEventArgs : EventArgs
	{
		public EndpointInfo Endpoint { get; private set; }

		public eConnectionType Type { get; private set; }

		public bool State { get; private set; }

		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="endpoint"></param>
		/// <param name="type"></param>
		/// <param name="state"></param>
		public EndpointStateChangedEventArgs(EndpointInfo endpoint, eConnectionType type, bool state)
		{
			Endpoint = endpoint;
			Type = type;
			State = state;
		}

		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="args"></param>
		public EndpointStateChangedEventArgs(EndpointStateChangedEventArgs args)
			: this(args.Endpoint, args.Type, args.State)
		{
		}
	}

	public sealed class EndpointRouteChangedEventArgs : EventArgs
	{
		public EndpointInfo Destination { get; private set; }

		public eConnectionType ConnectionType { get; private set; }

		public IEnumerable<EndpointInfo> Endpoints { get; private set; }

		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="destination"></param>
		/// <param name="type"></param>
		/// <param name="endpoints"></param>
		public EndpointRouteChangedEventArgs(EndpointInfo destination, eConnectionType type,
												  IEnumerable<EndpointInfo> endpoints)
		{
			Destination = destination;
			ConnectionType = type;
			Endpoints = endpoints;
		}

		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="args"></param>
		public EndpointRouteChangedEventArgs(EndpointRouteChangedEventArgs args)
			: this(args.Destination, args.ConnectionType, args.Endpoints)
		{
		}
	}

	public sealed class SourceDestinationRouteChangedEventArgs : EventArgs
	{
		public IEnumerable<ISource> Sources { get; private set; }

		public IEnumerable<IDestination> Destinations { get; private set; }
 
		public eConnectionType Type { get; private set; }

		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="sources"></param>
		/// <param name="destinations"></param>
		/// <param name="type"></param>
		public SourceDestinationRouteChangedEventArgs(IEnumerable<ISource> sources, 
													  IEnumerable<IDestination> destinations,
		                                              eConnectionType type)
		{
			Sources = sources;
			Destinations = destinations;
			Type = type;
		}

		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="args"></param>
		public SourceDestinationRouteChangedEventArgs(SourceDestinationRouteChangedEventArgs args)
			: this(args.Sources, args.Destinations, args.Type)
		{
		}
	}
}