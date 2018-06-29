using System;
using System.Collections.Generic;
using System.Linq;
using ICD.Common.Properties;
using ICD.Common.Utils;
using ICD.Common.Utils.Extensions;
using ICD.Connect.Routing.Connections;
using ICD.Connect.Routing.Endpoints;
using ICD.Connect.Routing.EventArguments;

namespace ICD.Connect.Routing.RoutingGraphs
{
	/// <summary>
	/// The RoutingGraphCache acts as a filter between the noisy switcher events and
	/// useful routing changes such as active source routing.
	/// </summary>
	public sealed class RoutingGraphCache
	{
		/// <summary>
		/// Raised when a source device is connected or disconnected.
		/// </summary>
		public event EventHandler<EndpointStateEventArgs> OnSourceDetectionStateChanged;

		/// <summary>
		/// Raised when a destination device changes active input state.
		/// </summary>
		public event EventHandler<EndpointStateEventArgs> OnDestinationInputActiveStateChanged;

		private readonly IRoutingGraph m_RoutingGraph;

		/// <summary>
		/// Maps destination to active input types.
		/// </summary>
		private readonly Dictionary<EndpointInfo, eConnectionType> m_DestinationEndpointActiveState;

		/// <summary>
		/// Maps source to detected types.
		/// </summary>
		private readonly Dictionary<EndpointInfo, eConnectionType> m_SourceEndpointDetectedState;

		/// <summary>
		/// Maps destination to source to routed types.
		/// </summary>
		private readonly Dictionary<EndpointInfo, Dictionary<EndpointInfo, eConnectionType>> m_RoutingMap;

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
			m_RoutingMap = new Dictionary<EndpointInfo, Dictionary<EndpointInfo, eConnectionType>>();

			m_CacheSection = new SafeCriticalSection();

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
		/// <returns></returns>
		[PublicAPI]
		public IEnumerable<EndpointInfo> GetSourcesForDestination(EndpointInfo destination, eConnectionType type)
		{
			if (destination == null)
				throw new ArgumentNullException();

			m_CacheSection.Enter();

			try
			{
				Dictionary<EndpointInfo, eConnectionType> sources;
				if (!m_RoutingMap.TryGetValue(destination, out sources))
					return Enumerable.Empty<EndpointInfo>();

				return sources.Where(kvp => kvp.Value.HasFlags(type))
				              .Select(kvp => kvp.Key);
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
		[PublicAPI]
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
		[PublicAPI]
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

		/// <summary>
		/// Updates the input active cache for the given destination and type.
		/// </summary>
		/// <param name="destination"></param>
		/// <param name="type"></param>
		private void UpdateDestinationInputActiveCache(EndpointInfo destination, eConnectionType type)
		{
			m_CacheSection.Enter();

			try
			{
				eConnectionType oldValue = m_DestinationEndpointActiveState.GetDefault(destination);
				eConnectionType newValue = oldValue;

				foreach (eConnectionType flag in EnumUtils.GetFlagsExceptNone(type))
				{
					bool detected = m_RoutingGraph.InputActive(destination, flag);

					if (detected)
						newValue = newValue | flag;
					else
						newValue = newValue & ~flag;
				}

				if (newValue == oldValue)
					return;

				m_DestinationEndpointActiveState[destination] = newValue;

				OnDestinationInputActiveStateChanged.Raise(this, new EndpointStateEventArgs(destination, newValue, true));
			}
			finally
			{
				m_CacheSection.Leave();
			}
		}

		/// <summary>
		/// Updates the source detection cache for the given source and type.
		/// </summary>
		/// <param name="source"></param>
		/// <param name="type"></param>
		private void UpdateSourceDetectionCache(EndpointInfo source, eConnectionType type)
		{
			m_CacheSection.Enter();

			try
			{
				eConnectionType oldValue = m_SourceEndpointDetectedState.GetDefault(source);
				eConnectionType newValue = oldValue;

				foreach (eConnectionType flag in EnumUtils.GetFlagsExceptNone(type))
				{
					bool detected = m_RoutingGraph.SourceDetected(source, flag);

					if (detected)
						newValue = newValue | flag;
					else
						newValue = newValue & ~flag;
				}

				if (newValue == oldValue)
					return;

				m_SourceEndpointDetectedState[source] = newValue;

				OnSourceDetectionStateChanged.Raise(this, new EndpointStateEventArgs(source, newValue, true));
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
			UpdateDestinationInputActiveCache(eventArgs.Endpoint, eventArgs.Type);
		}

		/// <summary>
		/// Called when a source becomes detected/undetected.
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="eventArgs"></param>
		private void RoutingGraphOnSourceDetectionStateChanged(object sender, EndpointStateEventArgs eventArgs)
		{
			UpdateSourceDetectionCache(eventArgs.Endpoint, eventArgs.Type);
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
