using System;
using System.Collections.Generic;
using System.Linq;
using ICD.Common.Utils;
using ICD.Common.Utils.Extensions;
using ICD.Connect.Routing.Connections;
using ICD.Connect.Routing.Endpoints;
using ICD.Connect.Routing.Endpoints.Destinations;
using ICD.Connect.Routing.Endpoints.Sources;
using ICD.Connect.Routing.EventArguments;

namespace ICD.Connect.Routing.RoutingGraphs
{
	public sealed class RoutingGraphCacheSourceState
	{
		public bool Active { get; set; }
		public bool Detected { get; set; }
	}

	/// <summary>
	/// The RoutingGraphCache acts as a filter between the noisy switcher events and
	/// useful routing changes such as active source routing.
	/// </summary>
	public sealed class RoutingGraphCache
	{
		private readonly IRoutingGraph m_RoutingGraph;

		private readonly Dictionary<EndpointInfo, eConnectionType> m_DestinationEndpointActiveState;
		private readonly Dictionary<EndpointInfo, eConnectionType> m_SourceEndpointDetectedState;

		private readonly
			Dictionary<IDestination,
				Dictionary<eConnectionType,
					Dictionary<int,
						Dictionary<ISource, RoutingGraphCacheSourceState>>>> m_RoutingMap;

		private readonly SafeCriticalSection m_CacheSection;

		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="routingGraph"></param>
		public RoutingGraphCache(IRoutingGraph routingGraph)
		{
			if (routingGraph == null)
				throw new ArgumentNullException("routingGraph");

			m_DestinationEndpointActiveState = new Dictionary<EndpointInfo, eConnectionType>();
			m_SourceEndpointDetectedState = new Dictionary<EndpointInfo, eConnectionType>();

			m_RoutingMap =
				new Dictionary<IDestination,
					Dictionary<eConnectionType,
						Dictionary<int,
							Dictionary<ISource, RoutingGraphCacheSourceState>>>>();

			m_RoutingGraph = routingGraph;
			Subscribe(m_RoutingGraph);

			UpdateRoutingCache();
		}

		#region Methods

		/// <summary>
		/// Gets the sources currently routed to the destination.
		/// </summary>
		/// <param name="destination"></param>
		/// <param name="type"></param>
		/// <param name="address"></param>
		/// <param name="active"></param>
		/// <param name="detected"></param>
		/// <returns></returns>
		public IEnumerable<ISource> GetSourcesForDestination(IDestination destination, eConnectionType type, int address,
		                                                     bool active, bool detected)
		{
			if (destination == null)
				throw new ArgumentNullException();

			m_CacheSection.Enter();

			try
			{
				Dictionary<eConnectionType, Dictionary<int, Dictionary<ISource, RoutingGraphCacheSourceState>>> types;
				if (!m_RoutingMap.TryGetValue(destination, out types))
					return Enumerable.Empty<ISource>();

				Dictionary<int, Dictionary<ISource, RoutingGraphCacheSourceState>> addresses;
				if (!types.TryGetValue(type, out addresses))
					return Enumerable.Empty<ISource>();

				Dictionary<ISource, RoutingGraphCacheSourceState> sources;
				if (!addresses.TryGetValue(address, out sources))
					return Enumerable.Empty<ISource>();

				return sources.Where(kvp =>
				                     {
					                     if (active && !kvp.Value.Active)
						                     return false;

					                     return !detected || kvp.Value.Detected;
				                     })
				              .Select(kvp => kvp.Key)
				              .ToArray();
			}
			finally
			{
				m_CacheSection.Leave();
			}
		}

		/// <summary>
		/// Gets the input active state for the given destination endpoint.
		/// </summary>
		/// <param name="endpoint"></param>
		/// <param name="type"></param>
		/// <returns>True if all of the flags in the connection type are active.</returns>
		public bool GetDestinationEndpointActiveState(EndpointInfo endpoint, eConnectionType type)
		{
			m_CacheSection.Enter();

			try
			{
				eConnectionType active = m_DestinationEndpointActiveState.GetDefault(endpoint);
				return EnumUtils.HasFlags(active, type);
			}
			finally
			{
				m_CacheSection.Leave();
			}
		}

		/// <summary>
		/// Gets the output detected state for the given source endpoint.
		/// </summary>
		/// <param name="endpoint"></param>
		/// <param name="type"></param>
		/// <returns>True if all of the flags in the connection type are detected.</returns>
		public bool GetSourceEndpointDetectedState(EndpointInfo endpoint, eConnectionType type)
		{
			m_CacheSection.Enter();

			try
			{
				eConnectionType active = m_SourceEndpointDetectedState.GetDefault(endpoint);
				return EnumUtils.HasFlags(active, type);
			}
			finally
			{
				m_CacheSection.Leave();
			}
		}

		#endregion

		#region Private Methods

		private void UpdateRoutingCache()
		{
			throw new NotImplementedException();
		}

		#endregion

		#region Routing Graph Callbacks

		/// <summary>
		/// Subscribe to the routing graph events.
		/// </summary>
		/// <param name="routingGraph"></param>
		private void Subscribe(IRoutingGraph routingGraph)
		{
			routingGraph.OnRouteChanged += RoutingGraphOnRouteChanged;
			routingGraph.OnSourceDetectionStateChanged += RoutingGraphOnSourceDetectionStateChanged;
			routingGraph.OnDestinationInputActiveStateChanged += RoutingGraphOnDestinationInputActiveStateChanged;
		}

		/// <summary>
		/// Called when a destination input becomes active/inactive.
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="eventArgs"></param>
		private void RoutingGraphOnDestinationInputActiveStateChanged(object sender, EndpointStateEventArgs eventArgs)
		{
			m_CacheSection.Enter();

			try
			{
				eConnectionType oldValue = m_DestinationEndpointActiveState.GetDefault(eventArgs.Endpoint);
				eConnectionType newValue = oldValue;

				foreach (eConnectionType flag in EnumUtils.GetFlagsExceptNone(eventArgs.Type))
				{
					bool detected = m_RoutingGraph.InputActive(eventArgs.Endpoint, flag);

					if (detected)
						newValue = newValue | flag;
					else
						newValue = newValue & ~flag;
				}

				if (eventArgs.State)
					newValue = oldValue | eventArgs.Type;
				else
					newValue = oldValue & ~eventArgs.Type;

				if (newValue == oldValue)
					return;

				m_DestinationEndpointActiveState[eventArgs.Endpoint] = newValue;
			}
			finally
			{
				m_CacheSection.Leave();
			}
		}

		/// <summary>
		/// Called when a source becomes detected/undetected.
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="eventArgs"></param>
		private void RoutingGraphOnSourceDetectionStateChanged(object sender, EndpointStateEventArgs eventArgs)
		{
			m_CacheSection.Enter();

			try
			{
				eConnectionType oldValue = m_SourceEndpointDetectedState.GetDefault(eventArgs.Endpoint);
				eConnectionType newValue = oldValue;

				foreach (eConnectionType flag in EnumUtils.GetFlagsExceptNone(eventArgs.Type))
				{
					bool detected = m_RoutingGraph.SourceDetected(eventArgs.Endpoint, flag);

					if (detected)
						newValue = newValue | flag;
					else
						newValue = newValue & ~flag;
				}

				if (newValue == oldValue)
					return;

				m_SourceEndpointDetectedState[eventArgs.Endpoint] = newValue;
			}
			finally
			{
				m_CacheSection.Leave();
			}
		}

		/// <summary>
		/// Called when a switcher changes routing.
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="eventArgs"></param>
		private void RoutingGraphOnRouteChanged(object sender, SwitcherRouteChangeEventArgs eventArgs)
		{
			throw new NotImplementedException();
		}

		#endregion
	}
}
