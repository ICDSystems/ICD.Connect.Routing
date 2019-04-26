﻿using System.Collections.Generic;
using System.Linq;
using ICD.Common.Utils;
using ICD.Common.Utils.Collections;
using ICD.Common.Utils.Extensions;
using ICD.Connect.Routing.Connections;
using ICD.Connect.Routing.Controls;
using ICD.Connect.Routing.StaticRoutes;
using ICD.Connect.Settings;

namespace ICD.Connect.Routing.RoutingGraphs
{
	public sealed class StaticRoutesCollection : AbstractOriginatorCollection<StaticRoute>, IStaticRoutesCollection
	{
		private readonly SafeCriticalSection m_StaticRoutesSection;

		/// <summary>
		/// Mapping each switcher to the static routes they are used in.
		/// </summary>
		private readonly Dictionary<IRouteMidpointControl, IcdHashSet<StaticRoute>> m_MidpointStaticRoutes;

		private readonly RoutingGraph m_RoutingGraph;

		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="routingGraph"></param>
		public StaticRoutesCollection(RoutingGraph routingGraph)
		{
			m_RoutingGraph = routingGraph;

			m_StaticRoutesSection = new SafeCriticalSection();
			m_MidpointStaticRoutes = new Dictionary<IRouteMidpointControl, IcdHashSet<StaticRoute>>();
		}

		#region Methods

		/// <summary>
		/// Re-applies the static routes that include the given midpoint.
		/// </summary>
		/// <param name="midpoint"></param>
		public void ReApplyStaticRoutesForMidpoint(IRouteMidpointControl midpoint)
		{
			m_StaticRoutesSection.Enter();

			try
			{
				IcdHashSet<StaticRoute> staticRoutes;
				if (m_MidpointStaticRoutes.TryGetValue(midpoint, out staticRoutes))
					Route(staticRoutes);
			}
			finally
			{
				m_StaticRoutesSection.Leave();
			}
		}

		#endregion

		#region Private Methods

		/// <summary>
		/// Routes each of the static routes in the given sequence.
		/// </summary>
		/// <param name="staticRoutes"></param>
		private void Route(IEnumerable<StaticRoute> staticRoutes)
		{
			staticRoutes.ForEach(Route);
		}

		/// <summary>
		/// Routes the path described by the given static route.
		/// </summary>
		/// <param name="staticRoute"></param>
		private void Route(StaticRoute staticRoute)
		{
			IEnumerable<Connection> connectionsEnumerable = GetConnections(staticRoute);
			Connection[] connections = connectionsEnumerable as Connection[] ?? connectionsEnumerable.ToArray();

			IcdHashSet<Connection> visited = new IcdHashSet<Connection>();

			// Looping over the connections to see where they share switchers
			foreach (Connection current in connections)
			{
				visited.Add(current);

				foreach (Connection other in connections.Where(c => !visited.Contains(c)))
				{
					IRouteSwitcherControl switcher;
					int input;
					int output;

					// Connections meet at destination
					if (current.Destination.Device == other.Source.Device)
					{
						switcher = m_RoutingGraph.GetDestinationControl(current) as IRouteSwitcherControl;
						input = current.Destination.Address;
						output = other.Source.Address;
					}
						// Connections meet at source
					else if (current.Source.Device == other.Destination.Device)
					{
						switcher = m_RoutingGraph.GetSourceControl(current) as IRouteSwitcherControl;
						output = current.Source.Address;
						input = other.Destination.Address;
					}
					else
					{
						continue;
					}

					// Force this route. Static routes don't care about ownership.
					if (switcher != null)
						switcher.Route(input, output, staticRoute.ConnectionType);
				}
			}
		}

		/// <summary>
		/// Gets the switchers for the given static route.
		/// </summary>
		/// <param name="staticRoute"></param>
		/// <returns></returns>
		private IEnumerable<IRouteSwitcherControl> GetSwitcherDevices(StaticRoute staticRoute)
		{
			return GetDevices(staticRoute).OfType<IRouteSwitcherControl>();
		}

		/// <summary>
		/// Gets the devices for the given static route.
		/// </summary>
		/// <param name="staticRoute"></param>
		/// <returns></returns>
		private IEnumerable<IRouteControl> GetDevices(StaticRoute staticRoute)
		{
			return GetConnections(staticRoute).SelectMany(c => m_RoutingGraph.GetControls(c))
			                                  .Distinct();
		}

		/// <summary>
		/// Gets the connections for the given static route.
		/// </summary>
		/// <param name="staticRoute"></param>
		/// <returns></returns>
		private IEnumerable<Connection> GetConnections(StaticRoute staticRoute)
		{
			m_StaticRoutesSection.Enter();

			try
			{
				return staticRoute.GetConnections()
				                  .Select(c => m_RoutingGraph.Connections.GetChild(c))
				                  .ToArray();
			}
			finally
			{
				m_StaticRoutesSection.Leave();
			}
		}

		#endregion

		/// <summary>
		/// Called when children are added to the collection before any events are raised.
		/// </summary>
		/// <param name="children"></param>
		protected override void ChildrenAdded(IEnumerable<StaticRoute> children)
		{
			UpdateStaticRoutes();
		}

		/// <summary>
		/// Called when children are removed from the collection before any events are raised.
		/// </summary>
		/// <param name="children"></param>
		protected override void ChildrenRemoved(IEnumerable<StaticRoute> children)
		{
			UpdateStaticRoutes();
		}

		/// <summary>
		/// Rebuilds the mapping of switchers to static routes, subscribes to switcher feedback and
		/// establishes the static routes.
		/// </summary>
		public void UpdateStaticRoutes()
		{
			m_StaticRoutesSection.Enter();

			try
			{
				// Clear the old lookup
				m_MidpointStaticRoutes.Clear();

				// Build the new lookup
				foreach (StaticRoute staticRoute in GetChildren())
				{
					foreach (IRouteSwitcherControl switcher in GetSwitcherDevices(staticRoute))
					{
						if (!m_MidpointStaticRoutes.ContainsKey(switcher))
							m_MidpointStaticRoutes[switcher] = new IcdHashSet<StaticRoute>();
						m_MidpointStaticRoutes[switcher].Add(staticRoute);
					}

					// Initialize this static route
					Route(staticRoute);
				}
			}
			finally
			{
				m_StaticRoutesSection.Leave();
			}
		}
	}
}
