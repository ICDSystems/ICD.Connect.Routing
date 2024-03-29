﻿using System;
using System.Collections.Generic;
using System.Linq;
using ICD.Common.Properties;
using ICD.Common.Utils;
using ICD.Common.Utils.Collections;
using ICD.Common.Utils.Extensions;
using ICD.Connect.Devices.Controls;
using ICD.Connect.Routing.Connections;
using ICD.Connect.Routing.Controls;
using ICD.Connect.Routing.Endpoints;
using ICD.Connect.Settings.Originators;
using ICD.Connect.Routing.Endpoints.Destinations;
using ICD.Connect.Routing.Endpoints.Sources;

namespace ICD.Connect.Routing.RoutingGraphs
{
	public sealed class ConnectionsCollection : AbstractOriginatorCollection<Connection>, IConnectionsCollection
	{
		private readonly SafeCriticalSection m_ConnectionsSection;

		/// <summary>
		/// Maps Control -> Address -> outgoing connections.
		/// </summary>
		private readonly Dictionary<DeviceControlInfo, IcdSortedDictionary<int, Connection>> m_OutputConnectionLookup;

		/// <summary>
		/// Maps Control -> Address -> incoming connections.
		/// </summary>
		private readonly Dictionary<DeviceControlInfo, IcdSortedDictionary<int, Connection>> m_InputConnectionLookup;

		/// <summary>
		/// Maps Source + Final Destination + Type -> Connection.
		/// </summary>
		private readonly Dictionary<FilteredConnectionLookupKey, Connection> m_FilteredConnectionLookup;

		private readonly RoutingGraph m_RoutingGraph;

		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="routingGraph"></param>
		public ConnectionsCollection(RoutingGraph routingGraph)
		{
			m_RoutingGraph = routingGraph;

			m_OutputConnectionLookup = new Dictionary<DeviceControlInfo, IcdSortedDictionary<int, Connection>>();
			m_InputConnectionLookup = new Dictionary<DeviceControlInfo, IcdSortedDictionary<int, Connection>>();
			m_FilteredConnectionLookup = new Dictionary<FilteredConnectionLookupKey, Connection>();

			m_ConnectionsSection = new SafeCriticalSection();
		}

		#region Methods

		/// <summary>
		/// Gets the connection for the given endpoint.
		/// </summary>
		/// <param name="destination">The destination endpoint for the target connection.</param>
		/// <returns></returns>
		[CanBeNull]
		public Connection GetInputConnection(EndpointInfo destination)
		{
			DeviceControlInfo key = new DeviceControlInfo(destination.Device, destination.Control);

			m_ConnectionsSection.Enter();

			try
			{
				IcdSortedDictionary<int, Connection> map;
				return m_InputConnectionLookup.TryGetValue(key, out map)
					       ? map.GetDefault(destination.Address)
					       : null;
			}
			finally
			{
				m_ConnectionsSection.Leave();
			}
		}

		/// <summary>
		/// Gets the connection for the given endpoint.
		/// </summary>
		/// <param name="destination"></param>
		/// <param name="type"></param>
		/// <returns></returns>
		public Connection GetInputConnection(EndpointInfo destination, eConnectionType type)
		{
			Connection connection = GetInputConnection(destination);
			return connection == null
				       ? null
				       : connection.ConnectionType.HasFlags(type)
					         ? connection
					         : null;
		}

		/// <summary>
		/// Gets the connection for the given endpoint.
		/// </summary>
		/// <param name="destinationControl"></param>
		/// <param name="input"></param>
		/// <returns></returns>
		[CanBeNull]
		public Connection GetInputConnection(IRouteDestinationControl destinationControl, int input)
		{
			if (destinationControl == null)
				throw new ArgumentNullException("destinationControl");

			EndpointInfo endpoint = destinationControl.GetInputEndpointInfo(input);
			return GetInputConnection(endpoint);
		}

		/// <summary>
		/// Gets the input connections for the device with the given type.
		/// </summary>
		/// <param name="destinationDeviceId"></param>
		/// <param name="destinationControlId"></param>
		/// <param name="type"></param>
		/// <returns></returns>
		public IEnumerable<Connection> GetInputConnections(int destinationDeviceId, int destinationControlId,
		                                                   eConnectionType type)
		{
			DeviceControlInfo info = new DeviceControlInfo(destinationDeviceId, destinationControlId);

			m_ConnectionsSection.Enter();

			try
			{
				IcdSortedDictionary<int, Connection> map;
				return m_InputConnectionLookup.TryGetValue(info, out map)
					       ? map.Values
					            .Where(c => EnumUtils.HasFlags(c.ConnectionType, type))
					            .ToArray()
					       : Enumerable.Empty<Connection>();
			}
			finally
			{
				m_ConnectionsSection.Leave();
			}
		}

		/// <summary>
		/// Gets the input connections for the device.
		/// </summary>
		/// <param name="destinationDeviceId"></param>
		/// <param name="destinationControlId"></param>
		/// <returns></returns>
		public IEnumerable<Connection> GetInputConnections(int destinationDeviceId, int destinationControlId)
		{
			DeviceControlInfo info = new DeviceControlInfo(destinationDeviceId, destinationControlId);

			m_ConnectionsSection.Enter();

			try
			{
				IcdSortedDictionary<int, Connection> map;
				return m_InputConnectionLookup.TryGetValue(info, out map)
					       ? map.Values.ToArray(map.Count)
					       : Enumerable.Empty<Connection>();
			}
			finally
			{
				m_ConnectionsSection.Leave();
			}
		}

		/// <summary>
		/// Gets the input connections for the device matching any of the given type flags.
		/// </summary>
		/// <param name="destinationDeviceId"></param>
		/// <param name="destinationControlId"></param>
		/// <param name="type"></param>
		/// <returns></returns>
		private IEnumerable<Connection> GetInputConnectionsAny(int destinationDeviceId, int destinationControlId,
		                                                       eConnectionType type)
		{
			DeviceControlInfo info = new DeviceControlInfo(destinationDeviceId, destinationControlId);

			m_ConnectionsSection.Enter();

			try
			{
				IcdSortedDictionary<int, Connection> map;
				return m_InputConnectionLookup.TryGetValue(info, out map)
					       ? map.Values
					            .Where(c => EnumUtils.HasAnyFlags(c.ConnectionType, type))
					            .ToArray()
					       : Enumerable.Empty<Connection>();
			}
			finally
			{
				m_ConnectionsSection.Leave();
			}
		}

		/// <summary>
		/// Gets the connection for the given endpoint.
		/// </summary>
		/// <param name="source">The source endpoint for the target connection</param>
		/// <returns></returns>
		[CanBeNull]
		public Connection GetOutputConnection(EndpointInfo source)
		{
			DeviceControlInfo key = new DeviceControlInfo(source.Device, source.Control);

			m_ConnectionsSection.Enter();

			try
			{
				IcdSortedDictionary<int, Connection> map;
				return m_OutputConnectionLookup.TryGetValue(key, out map)
					       ? map.GetDefault(source.Address)
					       : null;
			}
			finally
			{
				m_ConnectionsSection.Leave();
			}
		}

		/// <summary>
		/// Gets the connection for the given endpoint.
		/// </summary>
		/// <param name="source">The source endpoint for the target connection</param>
		/// <param name="type"></param>
		/// <returns></returns>
		[CanBeNull]
		public Connection GetOutputConnection(EndpointInfo source, eConnectionType type)
		{
			Connection connection = GetOutputConnection(source);
			return connection == null
				       ? null
				       : connection.ConnectionType.HasFlags(type)
					         ? connection
					         : null;
		}

		/// <summary>
		/// Gets the connection for the given endpoint.
		/// </summary>
		/// <param name="sourceControl"></param>
		/// <param name="output"></param>
		/// <returns></returns>
		[CanBeNull]
		public Connection GetOutputConnection(IRouteSourceControl sourceControl, int output)
		{
			if (sourceControl == null)
				throw new ArgumentNullException("sourceControl");

			EndpointInfo endpoint = sourceControl.GetOutputEndpointInfo(output);
			return GetOutputConnection(endpoint);
		}

		/// <summary>
		/// Gets the output connections for the given source device.
		/// </summary>
		/// <param name="sourceDeviceId"></param>
		/// <param name="sourceControlId"></param>
		/// <returns></returns>
		public IEnumerable<Connection> GetOutputConnections(int sourceDeviceId, int sourceControlId)
		{
			DeviceControlInfo info = new DeviceControlInfo(sourceDeviceId, sourceControlId);

			m_ConnectionsSection.Enter();

			try
			{
				IcdSortedDictionary<int, Connection> map;
				return m_OutputConnectionLookup.TryGetValue(info, out map)
					       ? map.Values.ToArray(map.Count)
					       : Enumerable.Empty<Connection>();
			}
			finally
			{
				m_ConnectionsSection.Leave();
			}
		}

		/// <summary>
		/// Gets the output connections for the given source device.
		/// </summary>
		/// <param name="sourceDeviceId"></param>
		/// <param name="sourceControlId"></param>
		/// <param name="flag"></param>
		/// <returns></returns>
		public IEnumerable<Connection> GetOutputConnections(int sourceDeviceId, int sourceControlId, eConnectionType flag)
		{
			DeviceControlInfo info = new DeviceControlInfo(sourceDeviceId, sourceControlId);

			m_ConnectionsSection.Enter();

			try
			{
				IcdSortedDictionary<int, Connection> map;
				return m_OutputConnectionLookup.TryGetValue(info, out map)
					       ? map.Values
					            .Where(c => c.ConnectionType.HasFlag(flag))
					            .ToArray()
					       : Enumerable.Empty<Connection>();
			}
			finally
			{
				m_ConnectionsSection.Leave();
			}
		}

		/// <summary>
		/// Gets the output connections for the given source device matching any of the given type flags.
		/// </summary>
		/// <param name="sourceDeviceId"></param>
		/// <param name="sourceControlId"></param>
		/// <param name="type"></param>
		/// <returns></returns>
		private IEnumerable<Connection> GetOutputConnectionsAny(int sourceDeviceId, int sourceControlId, eConnectionType type)
		{
			DeviceControlInfo info = new DeviceControlInfo(sourceDeviceId, sourceControlId);

			m_ConnectionsSection.Enter();

			try
			{
				IcdSortedDictionary<int, Connection> map;
				return m_OutputConnectionLookup.TryGetValue(info, out map)
					       ? map.Values
					            .Where(c => EnumUtils.HasAnyFlags(c.ConnectionType, type))
					            .ToArray()
					       : Enumerable.Empty<Connection>();
			}
			finally
			{
				m_ConnectionsSection.Leave();
			}
		}

		/// <summary>
		/// Given a source endpoint and a final destination endpoint,
		/// returns the possible output connection from the source to reach the destination.
		/// </summary>
		/// <param name="source"></param>
		/// <param name="finalDestination"></param>
		/// <param name="flag"></param>
		/// <returns></returns>
		[CanBeNull]
		public Connection GetOutputConnection(EndpointInfo source, EndpointInfo finalDestination, eConnectionType flag)
		{
			if (EnumUtils.HasMultipleFlags(flag))
				throw new ArgumentException("Connection type has multiple flags", "flag");

			FilteredConnectionLookupKey key = new FilteredConnectionLookupKey(source, finalDestination, flag);

			return m_ConnectionsSection.Execute(() => m_FilteredConnectionLookup.GetDefault(key));
		}

		/// <summary>
		/// Gets the output connections from the given source control in order to reach the given destination endpoints.
		/// </summary>
		/// <param name="sourceControl"></param>
		/// <param name="finalDestinations"></param>
		/// <param name="flag"></param>
		/// <returns></returns>
		public IEnumerable<Connection> GetOutputConnections(DeviceControlInfo sourceControl,
		                                                    IEnumerable<EndpointInfo> finalDestinations, eConnectionType flag)
		{
			if (finalDestinations == null)
				throw new ArgumentNullException("finalDestinations");

			if (EnumUtils.HasMultipleFlags(flag))
				throw new ArgumentException("ConnectionType has multiple flags", "flag");

			IList<EndpointInfo> destinationEndpoints =
				finalDestinations as IList<EndpointInfo> ?? finalDestinations.ToArray();

			m_ConnectionsSection.Enter();

			try
			{
				return
					GetOutputConnections(sourceControl.DeviceId, sourceControl.ControlId, flag)
						.Where(c => HasPathAny(c.Source, destinationEndpoints, flag));
			}
			finally
			{
				m_ConnectionsSection.Leave();
			}
		}

		/// <summary>
		/// Returns true if there is a path from the given source endpoint to any of the given destination endpoints.
		/// </summary>
		/// <param name="sourceEndpoint"></param>
		/// <param name="destinationEndpoints"></param>
		/// <param name="flag"></param>
		/// <returns></returns>
		private bool HasPathAny(EndpointInfo sourceEndpoint, IEnumerable<EndpointInfo> destinationEndpoints, eConnectionType flag)
		{
			if (EnumUtils.HasMultipleFlags(flag))
				throw new ArgumentException("ConnectionType has multiple flags", "flag");

			m_ConnectionsSection.Enter();

			try
			{
				return destinationEndpoints.Select(destination => GetOutputConnection(sourceEndpoint, destination, flag))
				                           .Any(connection => connection != null);
			}
			finally
			{
				m_ConnectionsSection.Leave();
			}
		}

		/// <summary>
		/// Gets filtered endpoints for the given destination.
		/// </summary>
		/// <param name="destination"></param>
		/// <param name="flag"></param>
		/// <returns></returns>
		public IEnumerable<EndpointInfo> FilterEndpoints(IDestination destination, eConnectionType flag)
		{
			if (destination == null)
				throw new ArgumentNullException("destination");

			if (EnumUtils.HasMultipleFlags(flag))
				throw new ArgumentException("Connection type has multiple flags", "flag");

			IEnumerable<EndpointInfo> endpoints =
				GetInputConnections(destination.Device, destination.Control, flag).Select(c => c.Destination);

			return destination.FilterEndpoints(endpoints);
		}

		/// <summary>
		/// Gets filtered endpoints for the given source.
		/// </summary>
		/// <param name="source"></param>
		/// <param name="flag"></param>
		/// <returns></returns>
		public IEnumerable<EndpointInfo> FilterEndpoints(ISource source, eConnectionType flag)
		{
			if (source == null)
				throw new ArgumentNullException("source");

			if (EnumUtils.HasMultipleFlags(flag))
				throw new ArgumentException("Connection type has multiple flags", "flag");

			IEnumerable<EndpointInfo> endpoints = GetOutputConnections(source.Device, source.Control, flag).Select(c => c.Source);

			return source.FilterEndpoints(endpoints);
		}

		/// <summary>
		/// Gets filtered endpoints matching any of the given connection flags for the given destination.
		/// </summary>
		/// <param name="destination"></param>
		/// <param name="type"></param>
		/// <returns></returns>
		public IEnumerable<EndpointInfo> FilterEndpointsAny(IDestination destination, eConnectionType type)
		{
			if (destination == null)
				throw new ArgumentNullException("destination");

			IEnumerable<EndpointInfo> endpoints =
				GetInputConnectionsAny(destination.Device, destination.Control, type).Select(c => c.Destination);

			return destination.FilterEndpoints(endpoints);
		}

		/// <summary>
		/// Gets filtered endpoints matching any of the given connection flags for the given source.
		/// </summary>
		/// <param name="source"></param>
		/// <param name="type"></param>
		/// <returns></returns>
		public IEnumerable<EndpointInfo> FilterEndpointsAny(ISource source, eConnectionType type)
		{
			if (source == null)
				throw new ArgumentNullException("source");

			IEnumerable<EndpointInfo> endpoints =
				GetOutputConnectionsAny(source.Device, source.Control, type).Select(c => c.Source);

			return source.FilterEndpoints(endpoints);
		}

		#endregion

		#region Adjacency

		/// <summary>
		/// Returns the destination input addresses where source and destination are directly connected.
		/// </summary>
		/// <param name="sourceControl"></param>
		/// <param name="destinationControl"></param>
		/// <param name="type"></param>
		/// <returns></returns>
		public IEnumerable<int> GetInputs(IRouteSourceControl sourceControl, IRouteDestinationControl destinationControl,
		                                  eConnectionType type)
		{
			if (sourceControl == null)
				throw new ArgumentNullException("sourceControl");

			if (destinationControl == null)
				throw new ArgumentNullException("destinationControl");

			m_ConnectionsSection.Enter();

			try
			{
				return GetInputConnections(destinationControl.Parent.Id, destinationControl.Id, type)
					.Where(c => c.Source.Device == sourceControl.Parent.Id && c.Source.Control == sourceControl.Id)
					.Select(c => c.Destination.Address)
					.ToArray();
			}
			finally
			{
				m_ConnectionsSection.Leave();
			}
		}

		/// <summary>
		/// Returns the destination input addresses.
		/// </summary>
		/// <param name="destinationControl"></param>
		/// <param name="type"></param>
		/// <returns></returns>
		public IEnumerable<int> GetInputs(IRouteDestinationControl destinationControl, eConnectionType type)
		{
			if (destinationControl == null)
				throw new ArgumentNullException("destinationControl");

			return GetInputConnections(destinationControl.Parent.Id, destinationControl.Id, type)
				.Select(c => c.Destination.Address);
		}

		/// <summary>
		/// Gets the outputs that match any of the given type flags.
		/// </summary>
		/// <param name="destinationControl"></param>
		/// <param name="type"></param>
		public IEnumerable<int> GetInputsAny(IRouteDestinationControl destinationControl, eConnectionType type)
		{
			if (destinationControl == null)
				throw new ArgumentNullException("destinationControl");

			return GetInputsAny(destinationControl.Parent.Id, destinationControl.Id, type);
		}

		/// <summary>
		/// Gets the mapped input addresses for the given destination control matching any of the given type flags.
		/// </summary>
		/// <param name="destinationDeviceId"></param>
		/// <param name="destinationControlId"></param>
		/// <param name="type"></param>
		/// <returns></returns>
		public IEnumerable<int> GetInputsAny(int destinationDeviceId, int destinationControlId, eConnectionType type)
		{
			return GetInputConnectionsAny(destinationDeviceId, destinationControlId, type)
				.Select(c => c.Destination.Address);
		}

		/// <summary>
		/// Gets the mapped output addresses for the given source control.
		/// </summary>
		/// <param name="sourceControl"></param>
		/// <param name="type"></param>
		/// <returns></returns>
		public IEnumerable<int> GetOutputs(IRouteSourceControl sourceControl, eConnectionType type)
		{
			if (sourceControl == null)
				throw new ArgumentNullException("sourceControl");

			return GetOutputs(sourceControl.Parent.Id, sourceControl.Id, type);
		}

		/// <summary>
		/// Gets the mapped output addresses for the given source control.
		/// </summary>
		/// <param name="sourceDeviceId"></param>
		/// <param name="sourceControlId"></param>
		/// <param name="type"></param>
		/// <returns></returns>
		public IEnumerable<int> GetOutputs(int sourceDeviceId, int sourceControlId, eConnectionType type)
		{
			return GetOutputConnections(sourceDeviceId, sourceControlId, type)
				.Select(c => c.Source.Address);
		}

		/// <summary>
		/// Returns the source output addresses where source and destination are directly connected.
		/// </summary>
		/// <param name="sourceControl"></param>
		/// <param name="destinationControl"></param>
		/// <param name="type"></param>
		/// <returns></returns>
		public IEnumerable<int> GetOutputs(IRouteSourceControl sourceControl, IRouteDestinationControl destinationControl,
		                                   eConnectionType type)
		{
			if (sourceControl == null)
				throw new ArgumentNullException("sourceControl");

			if (destinationControl == null)
				throw new ArgumentNullException("destinationControl");

			return GetOutputConnections(sourceControl.Parent.Id, sourceControl.Id, type)
				.Where(c => c.Destination.Device == destinationControl.Parent.Id &&
				            c.Destination.Control == destinationControl.Id)
				.Select(c => c.Source.Address);
		}

		/// <summary>
		/// Gets the outputs that match any of the given type flags.
		/// </summary>
		/// <param name="sourceControl"></param>
		/// <param name="type"></param>
		/// <returns></returns>
		public IEnumerable<int> GetOutputsAny(IRouteSourceControl sourceControl, eConnectionType type)
		{
			if (sourceControl == null)
				throw new ArgumentNullException("sourceControl");

			return GetOutputsAny(sourceControl.Parent.Id, sourceControl.Id, type);
		}

		/// <summary>
		/// Gets the mapped output addresses for the given source control matching any of the given type flags.
		/// </summary>
		/// <param name="sourceDeviceId"></param>
		/// <param name="sourceControlId"></param>
		/// <param name="type"></param>
		/// <returns></returns>
		public IEnumerable<int> GetOutputsAny(int sourceDeviceId, int sourceControlId, eConnectionType type)
		{
			return GetOutputConnectionsAny(sourceDeviceId, sourceControlId, type)
				.Select(c => c.Source.Address);
		}

		#endregion

		#region Caching

		/// <summary>
		/// Called when children are added to the collection before any events are raised.
		/// </summary>
		/// <param name="children"></param>
		protected override void ChildrenAdded(IEnumerable<Connection> children)
		{
			base.ChildrenAdded(children);

			RebuildCache();
		}

		/// <summary>
		/// Called when children are removed from the collection before any events are raised.
		/// </summary>
		/// <param name="children"></param>
		protected override void ChildrenRemoved(IEnumerable<Connection> children)
		{
			base.ChildrenRemoved(children);

			RebuildCache();
		}

		private void RebuildCache()
		{
			m_ConnectionsSection.Enter();

			try
			{
				m_OutputConnectionLookup.Clear();
				m_InputConnectionLookup.Clear();
				m_FilteredConnectionLookup.Clear();

				foreach (Connection child in GetChildren())
				{
					// Build the source cache
					DeviceControlInfo sourceInfo = new DeviceControlInfo(child.Source.Device, child.Source.Control);
					m_OutputConnectionLookup.GetOrAddNew(sourceInfo).Add(child.Source.Address, child);

					// Build the destination cache
					DeviceControlInfo destinationInfo = new DeviceControlInfo(child.Destination.Device, child.Destination.Control);
					m_InputConnectionLookup.GetOrAddNew(destinationInfo).Add(child.Destination.Address, child);
				}

				RebuildFilteredConnectionsMap();
			}
			finally
			{
				m_ConnectionsSection.Leave();
			}
		}

		/// <summary>
		/// Clears and rebuilds the filtered connections table.
		/// </summary>
		private void RebuildFilteredConnectionsMap()
		{
			m_ConnectionsSection.Enter();

			try
			{
				m_FilteredConnectionLookup.Clear();

				Connection[] connections = GetChildren().ToArray();

				IcdHashSet<DeviceControlInfo> midpoints =
					connections.Select(c => c.Destination.GetDeviceControlInfo())
					           .Distinct()
					           .Where(c => m_RoutingGraph.GetControl<IRouteDestinationControl>(c.DeviceId, c.ControlId)
						                  is IRouteMidpointControl)
					           .ToIcdHashSet();

				foreach (Connection outputConnection in connections)
				{
					// Optimization - We can skip terminal output connections
					// E.g. A -> B -> C we can't path from C to A
					bool terminal = !m_OutputConnectionLookup.ContainsKey(outputConnection.Destination.GetDeviceControlInfo());

					foreach (Connection inputConnection in connections)
					{
						eConnectionType type =
							EnumUtils.GetFlagsIntersection(outputConnection.ConnectionType, inputConnection.ConnectionType);

						foreach (eConnectionType flag in EnumUtils.GetFlagsExceptNone(type))
						{
							FilteredConnectionLookupKey key = new FilteredConnectionLookupKey(outputConnection.Source, inputConnection.Destination, flag);
							if (m_FilteredConnectionLookup.ContainsKey(key))
								continue;

							// Easy case - Output and Input are the same
							if (outputConnection == inputConnection)
							{
								m_FilteredConnectionLookup.Add(key, outputConnection);
								continue;
							}

							if (terminal)
							{
								m_FilteredConnectionLookup.Add(key, null);
								continue;
							}

							// If a path is found it's added to the cache as part of the pathfinding
							if (RebuildFilteredConnectionsMapHasAnyPath(midpoints, outputConnection, inputConnection, flag))
								continue;

							// Add the negative case so we can bail early for other paths
							m_FilteredConnectionLookup.Add(key, null);
						}
					}
				}
			}
			finally
			{
				m_ConnectionsSection.Leave();
			}
		}

		/// <summary>
		/// Returns true if there is a connection path from the given source to the given destination.
		/// </summary>
		/// <param name="midpoints"></param>
		/// <param name="outputConnection"></param>
		/// <param name="inputConnection"></param>
		/// <param name="flag"></param>
		/// <returns></returns>
		private bool RebuildFilteredConnectionsMapHasAnyPath(IcdHashSet<DeviceControlInfo> midpoints, Connection outputConnection, Connection inputConnection,
		                                                     eConnectionType flag)
		{
			return RecursionUtils.BreadthFirstSearch(outputConnection, inputConnection,
			                                         c => RebuildFilteredConnectionsMapGetChildren(midpoints, c, outputConnection, inputConnection, flag));
		}

		/// <summary>
		/// Given an input connection returns the connected output connections.
		/// </summary>
		/// <param name="midpoints"></param>
		/// <param name="inputConnection"></param>
		/// <param name="startOutputConnection"></param>
		/// <param name="finalInputConnection"></param>
		/// <param name="flag"></param>
		/// <returns></returns>
		private IEnumerable<Connection> RebuildFilteredConnectionsMapGetChildren(
			IcdHashSet<DeviceControlInfo> midpoints, Connection inputConnection, Connection startOutputConnection,
			Connection finalInputConnection, eConnectionType flag)
		{
			// Can only route through midpoints
			if (!midpoints.Contains(inputConnection.Destination.GetDeviceControlInfo()))
				yield break;

			IEnumerable<Connection> outputConnections =
				GetOutputConnections(inputConnection.Destination.Device, inputConnection.Destination.Control, flag);

			foreach (Connection outputConnection in outputConnections)
			{
				// Bail early if we already know the result for the output connection to the final connection
				FilteredConnectionLookupKey key = new FilteredConnectionLookupKey(outputConnection.Source, finalInputConnection.Destination, flag);
				Connection pathableConnection;
				if (m_FilteredConnectionLookup.TryGetValue(key, out pathableConnection) && pathableConnection == null)
					continue;

				// Add this sub-path from start to output connection
				key = new FilteredConnectionLookupKey(startOutputConnection.Source, outputConnection.Destination, flag);
				if (!m_FilteredConnectionLookup.ContainsKey(key))
					m_FilteredConnectionLookup.Add(key, startOutputConnection);

				// Add this sub-path from input connection to output connection
				key = new FilteredConnectionLookupKey(inputConnection.Source, outputConnection.Destination, flag);
				if (!m_FilteredConnectionLookup.ContainsKey(key))
					m_FilteredConnectionLookup.Add(key, inputConnection);

				yield return outputConnection;
			}
		}

		#endregion
	}
}
