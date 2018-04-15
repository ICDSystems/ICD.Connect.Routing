using System;
using System.Collections.Generic;
using System.Linq;
using ICD.Common.Utils;
using ICD.Connect.Routing.Connections;
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

			m_RoutingMap =
				new Dictionary<IDestination,
					Dictionary<eConnectionType,
						Dictionary<int,
							Dictionary<ISource, RoutingGraphCacheSourceState>>>>();

			m_RoutingGraph = routingGraph;
			Subscribe(m_RoutingGraph);

			UpdateRoutingCache();
		}

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
		/// <param name="endpointStateEventArgs"></param>
		private void RoutingGraphOnDestinationInputActiveStateChanged(object sender, EndpointStateEventArgs endpointStateEventArgs)
		{
			throw new NotImplementedException();
		}

		/// <summary>
		/// Called when a source becomes detected/undetected.
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="endpointStateEventArgs"></param>
		private void RoutingGraphOnSourceDetectionStateChanged(object sender, EndpointStateEventArgs endpointStateEventArgs)
		{
			throw new NotImplementedException();
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
