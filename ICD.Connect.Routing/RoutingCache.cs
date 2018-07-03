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

		#endregion

		#region Private Members

		private readonly IRoutingGraph m_RoutingGraph;

		private readonly Dictionary<ISource, IcdHashSet<EndpointInfo>> m_SourceToEndpoints;
		private readonly Dictionary<EndpointInfo, IcdHashSet<ISource>> m_EndpointToSources;

		private readonly Dictionary<ISource, eConnectionType> m_SourceTransmitting;
		private readonly Dictionary<ISource, eConnectionType> m_SourceDetected;
		private readonly Dictionary<EndpointInfo, eConnectionType> m_EndpointTransmitting;
		private readonly Dictionary<EndpointInfo, eConnectionType> m_EndpointDetected;

		private readonly Dictionary<EndpointInfo, Dictionary<eConnectionType, IcdHashSet<EndpointInfo>>> m_DestinationToSourceCache;
		private readonly Dictionary<EndpointInfo, Dictionary<eConnectionType, IcdHashSet<EndpointInfo>>> m_SourceToDestinationCache;

		#endregion

		public RoutingCache(IRoutingGraph routingGraph)
		{
			m_RoutingGraph = routingGraph;
			SubscribeToRoutingGraphEvents();

			m_SourceToEndpoints = new Dictionary<ISource, IcdHashSet<EndpointInfo>>();
			m_EndpointToSources = new Dictionary<EndpointInfo, IcdHashSet<ISource>>();

			m_SourceTransmitting = new Dictionary<ISource, eConnectionType>();
			m_SourceDetected = new Dictionary<ISource, eConnectionType>();
			m_EndpointTransmitting = new Dictionary<EndpointInfo, eConnectionType>();
			m_EndpointDetected = new Dictionary<EndpointInfo, eConnectionType>();

			m_DestinationToSourceCache = new Dictionary<EndpointInfo, Dictionary<eConnectionType, IcdHashSet<EndpointInfo>>>();
			m_SourceToDestinationCache = new Dictionary<EndpointInfo, Dictionary<eConnectionType, IcdHashSet<EndpointInfo>>>();

			InitializeCache();
		}

		private void InitializeCache()
		{
			IcdHashSet<ISource> sources = m_RoutingGraph.Sources.ToIcdHashSet();
			IcdHashSet<EndpointInfo> sourceEndpoints =
				sources.SelectMany(s => s.GetEndpoints()).ToIcdHashSet();

			foreach (EndpointInfo endpoint in sourceEndpoints)
				m_EndpointToSources.Add(endpoint, sources.Where(s => s.Contains(endpoint)).ToIcdHashSet());

			foreach (ISource source in sources)
				m_SourceToEndpoints.Add(source, source.GetEndpoints().ToIcdHashSet());

			// Initializes m_SourceToEndpoints and m_EndpointToSources
			foreach (var endpoint in m_RoutingGraph.Sources.SelectMany(s => s.GetEndpoints()).Distinct())
			{
				UpdateSourceEndpoint(endpoint);
			}
		}

		public void Dispose()
		{
			UnsubscribeFromRoutingGraphEvents();

			m_SourceToEndpoints.Clear();
			m_EndpointToSources.Clear();

			m_SourceTransmitting.Clear();
			m_SourceDetected.Clear();
			m_EndpointTransmitting.Clear();
			m_EndpointDetected.Clear();

			m_DestinationToSourceCache.Clear();
			m_SourceToDestinationCache.Clear();
		}

		#region Public Methods

		public bool GetSourceDetected(ISource source, eConnectionType type)
		{
			return m_SourceDetected.ContainsKey(source) && m_SourceDetected[source].HasFlags(type);
		}

		public bool GetSourceTransmitting(ISource source, eConnectionType type)
		{
			return m_SourceTransmitting.ContainsKey(source) && m_SourceTransmitting[source].HasFlags(type);
		}

		public IEnumerable<ISource> GetSourcesForDestination(EndpointInfo destinationEndpoint, eConnectionType type)
		{
			if(!EnumUtils.HasSingleFlag(type))
				throw new ArgumentException("type cannot have multiple flags", "type");

			if (!m_DestinationToSourceCache.ContainsKey(destinationEndpoint))
				return Enumerable.Empty<ISource>();

			IcdHashSet<ISource> sources = new IcdHashSet<ISource>();

			foreach (var endpoint in m_DestinationToSourceCache[destinationEndpoint][type])
			{
				sources.AddRange(m_EndpointToSources[endpoint]);
			}

			return sources;
		}

		public IEnumerable<EndpointInfo> GetSourceEndpointsForDestination(EndpointInfo destinationEndpoint,
		                                                                  eConnectionType type)
		{
			if (!EnumUtils.HasSingleFlag(type))
				throw new ArgumentException("type cannot have multiple flags", "type");

			if (!m_DestinationToSourceCache.ContainsKey(destinationEndpoint))
				return Enumerable.Empty<EndpointInfo>();

			return m_DestinationToSourceCache[destinationEndpoint][type];
		}

		#endregion

		#region Private Methods

		private void UpdateSourceEndpoint(EndpointInfo endpoint)
		{
			IRouteSourceControl control = m_RoutingGraph.GetSourceControl(endpoint);

			foreach (eConnectionType flag in EnumUtils.GetValuesExceptNone<eConnectionType>())
			{
				bool transmission = control.GetActiveTransmissionState(endpoint.Address, flag);
				UpdateEndpointTransmissionState(endpoint, flag, transmission);

				bool detection = m_RoutingGraph.SourceDetected(endpoint, flag);
				UpdateEndpointDetectionState(endpoint, flag, detection);
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

		private void UpdateEndpointTransmissionState(EndpointInfo endpoint, eConnectionType type, bool state)
		{
			eConnectionType flags = m_EndpointTransmitting[endpoint];

			if (state)
				flags |= type;
			else
				flags &= ~type;

			if (flags == m_EndpointTransmitting[endpoint])
				return;

			m_EndpointTransmitting[endpoint] = flags;
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
				           (current, endpoint) => current | m_EndpointTransmitting[endpoint]);

			eConnectionType oldFlags = m_SourceTransmitting[source];
			
			if(flags == oldFlags)
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

		private void UpdateEndpointDetectionState(EndpointInfo endpoint, eConnectionType type, bool state)
		{
			eConnectionType flags = m_EndpointDetected[endpoint];

			if (state)
				flags |= type;
			else
				flags &= ~type;

			if (flags == m_EndpointDetected[endpoint])
				return;

			m_EndpointDetected[endpoint] = flags;
			OnEndpointDetectionStateChanged.Raise(this, new EndpointStateChangedEventArgs(endpoint, type, state));

			foreach (var source in m_EndpointToSources[endpoint])
				UpdateSourceDetectionState(source);
		}

		private void UpdateSourceDetectionState(ISource source)
		{
			eConnectionType flags = m_SourceToEndpoints[source]
				.Aggregate(eConnectionType.None,
						   (current, endpoint) => current | m_EndpointDetected[endpoint]);

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

		private void SubscribeToRoutingGraphEvents()
		{
			m_RoutingGraph.OnSourceTransmissionStateChanged += RoutingGraphOnSourceTransmissionStateChanged;
			m_RoutingGraph.OnSourceDetectionStateChanged += RoutingGraphOnSourceDetectionStateChanged;
			m_RoutingGraph.OnRouteChanged += RoutingGraphOnRouteChanged;
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

		private void UnsubscribeFromRoutingGraphEvents()
		{
			m_RoutingGraph.OnSourceTransmissionStateChanged -= RoutingGraphOnSourceTransmissionStateChanged;
			m_RoutingGraph.OnSourceDetectionStateChanged -= RoutingGraphOnSourceDetectionStateChanged;
		}

		private void RoutingGraphOnSourceTransmissionStateChanged(object sender, EndpointStateEventArgs args)
		{
			if (!m_EndpointToSources.ContainsKey(args.Endpoint))
				UpdateSourceEndpoint(args.Endpoint);

			UpdateEndpointTransmissionState(args.Endpoint, args.Type, args.State);
		}

		private void RoutingGraphOnSourceDetectionStateChanged(object sender, EndpointStateEventArgs args)
		{
			if (!m_EndpointToSources.ContainsKey(args.Endpoint))
				UpdateSourceEndpoint(args.Endpoint);

			UpdateEndpointDetectionState(args.Endpoint, args.Type, args.State);
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