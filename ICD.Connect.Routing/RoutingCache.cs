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
		public event EventHandler<RouteToDestinationChangedEventArgs> OnRouteToDestinationChanged;

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

		private readonly Dictionary<EndpointInfo, Dictionary<eConnectionType, IcdHashSet<EndpointInfo>>> m_DestinationToSourceCache;
		private readonly Dictionary<EndpointInfo, Dictionary<eConnectionType, IcdHashSet<EndpointInfo>>> m_SourceToDestinationCache;

		private readonly Dictionary<EndpointInfo, eConnectionType> m_DestinationEndpointActive;

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

			m_DestinationToSourceCache = new Dictionary<EndpointInfo, Dictionary<eConnectionType, IcdHashSet<EndpointInfo>>>();
			m_SourceToDestinationCache = new Dictionary<EndpointInfo, Dictionary<eConnectionType, IcdHashSet<EndpointInfo>>>();

			m_DestinationEndpointActive = new Dictionary<EndpointInfo, eConnectionType>();

			RebuildCache();
		}

		/// <summary>
		/// Release resources.
		/// </summary>
		public void Dispose()
		{
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

			m_DestinationToSourceCache.Clear();
			m_SourceToDestinationCache.Clear();

			m_DestinationEndpointActive.Clear();
		}

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

			if (!m_DestinationToSourceCache.ContainsKey(destinationEndpoint))
				return Enumerable.Empty<EndpointInfo>();

			return m_DestinationToSourceCache[destinationEndpoint][flag];
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
			}

			foreach (IDestination destination in destinations)
				m_DestinationToEndpoints.Add(destination, destination.GetEndpoints().ToIcdHashSet());

			foreach (EndpointInfo endpoint in destinationEndpoints)
				UpdateDestinationEndpoint(endpoint);
		}

		private void UpdateSourceEndpoint(EndpointInfo endpoint)
		{
			IRouteSourceControl control = m_RoutingGraph.GetSourceControl(endpoint);

			foreach (eConnectionType flag in EnumUtils.GetValuesExceptNone<eConnectionType>())
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

			foreach (eConnectionType flag in EnumUtils.GetValuesExceptNone<eConnectionType>())
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

			foreach (var flag in EnumUtils.GetFlagsExceptNone<eConnectionType>())
				m_DestinationToSourceCache[destinationEndpoint].Add(flag, new IcdHashSet<EndpointInfo>());
		}

		private void AddSourceToSourceToDestinationCache(EndpointInfo sourceEndpoint)
		{
			if (m_SourceToDestinationCache.ContainsKey(sourceEndpoint))
				return;

			m_SourceToDestinationCache.Add(sourceEndpoint, new Dictionary<eConnectionType, IcdHashSet<EndpointInfo>>());

			foreach (var flag in EnumUtils.GetFlagsExceptNone<eConnectionType>())
				m_SourceToDestinationCache[sourceEndpoint].Add(flag, new IcdHashSet<EndpointInfo>());
		}

		private void UpdateSourceEndpointTransmissionState(EndpointInfo endpoint, eConnectionType type, bool state)
		{
			eConnectionType flags = m_SourceEndpointTransmitting[endpoint];

			if (state)
				flags |= type;
			else
				flags &= ~type;

			if (flags == m_SourceEndpointTransmitting[endpoint])
				return;

			m_SourceEndpointTransmitting[endpoint] = flags;
			OnEndpointTransmissionStateChanged.Raise(this, new EndpointStateChangedEventArgs(endpoint, type, state));

			foreach (var source in m_EndpointToSources[endpoint])
			{
				UpdateSourceTransmissionState(source);
			}
		}

		private void UpdateSourceTransmissionState(ISource source)
		{
			eConnectionType flags = m_SourceToEndpoints[source]
				.Aggregate(eConnectionType.None,
				           (current, endpoint) => current | m_SourceEndpointTransmitting[endpoint]);

			eConnectionType oldFlags = m_SourceTransmitting[source];
			
			if (flags == oldFlags)
				return;

			m_SourceTransmitting[source] = flags;

			foreach (var flag in EnumUtils.GetFlagsExceptNone<eConnectionType>())
			{
				if(oldFlags.HasFlag(flag) && !flags.HasFlag(flag))
					OnSourceTransmissionStateChanged.Raise(this, new SourceStateChangedEventArgs(source, flag, false));
				else if(!oldFlags.HasFlag(flag) && flags.HasFlag(flag))
					OnSourceTransmissionStateChanged.Raise(this, new SourceStateChangedEventArgs(source, flag, true));
			}
		}

		private void UpdateSourceEndpointDetectionState(EndpointInfo endpoint, eConnectionType type, bool state)
		{
			eConnectionType flags = m_SourceEndpointDetected[endpoint];

			if (state)
				flags |= type;
			else
				flags &= ~type;

			if (flags == m_SourceEndpointDetected[endpoint])
				return;

			m_SourceEndpointDetected[endpoint] = flags;
			OnEndpointDetectionStateChanged.Raise(this, new EndpointStateChangedEventArgs(endpoint, type, state));

			foreach (var source in m_EndpointToSources[endpoint])
				UpdateSourceDetectionState(source);
		}

		private void UpdateSourceDetectionState(ISource source)
		{
			eConnectionType flags = m_SourceToEndpoints[source]
				.Aggregate(eConnectionType.None,
						   (current, endpoint) => current | m_SourceEndpointDetected[endpoint]);

			eConnectionType oldFlags = m_SourceDetected[source];

			if (flags == oldFlags)
				return;

			m_SourceDetected[source] = flags;

			foreach (var flag in EnumUtils.GetFlagsExceptNone<eConnectionType>())
			{
				if (oldFlags.HasFlag(flag) && !flags.HasFlag(flag))
					OnSourceDetectionStateChanged.Raise(this, new SourceStateChangedEventArgs(source, flag, false));
				else if (!oldFlags.HasFlag(flag) && flags.HasFlag(flag))
					OnSourceDetectionStateChanged.Raise(this, new SourceStateChangedEventArgs(source, flag, true));
			}
		}

		private void UpdateDestinationEndpointInputActiveState(EndpointInfo endpoint, eConnectionType type, bool state)
		{
			eConnectionType flags = m_DestinationEndpointActive[endpoint];

			if (state)
				flags |= type;
			else
				flags &= ~type;

			if (flags == m_DestinationEndpointActive[endpoint])
				return;

			m_DestinationEndpointActive[endpoint] = flags;

			OnDestinationEndpointActiveChanged.Raise(this, new EndpointStateChangedEventArgs(endpoint, type, state));
		}

		private void RemoveOldValuesFromDestinationCache(IcdHashSet<EndpointInfo> oldSourceEndpoints,
														 IEnumerable<EndpointInfo> destinations,
														 eConnectionType type)
		{
			if (!EnumUtils.HasSingleFlag(type))
				throw new ArgumentException("Type has multiple flags", "type");

			foreach (var destination in destinations.Where(destination => m_DestinationToSourceCache.ContainsKey(destination)))
			{
				foreach (var endpointToRemove in oldSourceEndpoints)
					m_DestinationToSourceCache[destination][type].Remove(endpointToRemove);
			}
		}

		private void AddNewValuesToDestinationCache(IcdHashSet<EndpointInfo> newSourceEndpoints,
													IEnumerable<EndpointInfo> destinations,
													eConnectionType type)
		{
			if (!EnumUtils.HasSingleFlag(type))
				throw new ArgumentException("Type has multiple flags", "type");

			foreach (var destination in destinations)
			{
				if (!m_DestinationToSourceCache.ContainsKey(destination))
					AddDestinationToDestinationToSourceCache(destination);

				foreach (var endpointToAdd in newSourceEndpoints)
					m_DestinationToSourceCache[destination][type].Add(endpointToAdd);
			}
		}

		private void RemoveOldValuesFromSourceCache(IEnumerable<EndpointInfo> oldSourceEndpoints,
													IcdHashSet<EndpointInfo> destinations,
													eConnectionType type)
		{
			if (!EnumUtils.HasSingleFlag(type))
				throw new ArgumentException("Type has multiple flags", "type");

			foreach (var source in oldSourceEndpoints.Where(source => m_SourceToDestinationCache.ContainsKey(source)))
			{
				foreach (var endpointToRemove in destinations)
					m_SourceToDestinationCache[source][type].Remove(endpointToRemove);
			}
		}

		private void AddNewValuesToSourceCache(IEnumerable<EndpointInfo> newSourceEndpoints,
											   IcdHashSet<EndpointInfo> destinations,
											   eConnectionType type)
		{
			if (!EnumUtils.HasSingleFlag(type))
				throw new ArgumentException("Type has multiple flags", "type");

			foreach (var source in newSourceEndpoints)
			{
				if(!m_SourceToDestinationCache.ContainsKey(source))
					AddSourceToSourceToDestinationCache(source);

				foreach (var endpointToAdd in destinations)
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
			var oldSourceEndpoints = args.OldSourceEndpoints.ToIcdHashSet();
			var newSourceEndpoints = args.NewSourceEndpoints.ToIcdHashSet();
			var destinationEndpoints = args.DestinationEndpoints.ToIcdHashSet();

			foreach (var flag in EnumUtils.GetFlagsExceptNone(args.Type))
			{
				RemoveOldValuesFromSourceCache(oldSourceEndpoints, destinationEndpoints, flag);
				AddNewValuesToSourceCache(newSourceEndpoints, destinationEndpoints, flag);

				if (oldSourceEndpoints.SetEquals(newSourceEndpoints))
					continue;

				RemoveOldValuesFromDestinationCache(oldSourceEndpoints, destinationEndpoints, flag);
				AddNewValuesToDestinationCache(newSourceEndpoints, destinationEndpoints, flag);

				foreach (var destination in destinationEndpoints)
				{
					OnRouteToDestinationChanged.Raise(this,
					                                  new RouteToDestinationChangedEventArgs(
						                                  destination,
						                                  args.Type,
						                                  m_DestinationToSourceCache[destination][flag]));
				}
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
	}

	public sealed class SourceStateChangedEventArgs : EventArgs
	{
		public ISource Source { get; private set; }

		public eConnectionType Type { get; private set; }

		public bool State { get; private set; }

		public SourceStateChangedEventArgs(ISource source, eConnectionType type, bool state)
		{
			Source = source;
			Type = type;
			State = state;
		}

		public SourceStateChangedEventArgs(SourceStateChangedEventArgs args)
		{
			Source = args.Source;
			Type = args.Type;
			State = args.State;
		}
	}

	public sealed class EndpointStateChangedEventArgs : EventArgs
	{
		public EndpointInfo Endpoint { get; private set; }

		public eConnectionType Type { get; private set; }

		public bool State { get; private set; }

		public EndpointStateChangedEventArgs(EndpointInfo endpoint, eConnectionType type, bool state)
		{
			Endpoint = endpoint;
			Type = type;
			State = state;
		}

		public EndpointStateChangedEventArgs(EndpointStateChangedEventArgs args)
		{
			Endpoint = args.Endpoint;
			Type = args.Type;
			State = args.State;
		}
	}

	public sealed class RouteToDestinationChangedEventArgs : EventArgs
	{
		public EndpointInfo Destination { get; private set; }

		public eConnectionType ConnectionType { get; private set; }

		public IEnumerable<EndpointInfo> Endpoints { get; private set; }

		public RouteToDestinationChangedEventArgs(EndpointInfo destination, eConnectionType type,
												  IEnumerable<EndpointInfo> endpoints)
		{
			Destination = destination;
			ConnectionType = type;
			Endpoints = endpoints;
		}

		public RouteToDestinationChangedEventArgs(RouteToDestinationChangedEventArgs args)
		{
			Destination = args.Destination;
			ConnectionType = args.ConnectionType;
			Endpoints = args.Endpoints;
		}
	}
}