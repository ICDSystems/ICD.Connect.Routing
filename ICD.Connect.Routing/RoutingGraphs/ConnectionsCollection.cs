using System;
using System.Collections.Generic;
using System.Linq;
using ICD.Common.Properties;
using ICD.Common.Utils;
using ICD.Common.Utils.Extensions;
using ICD.Connect.Devices.Controls;
using ICD.Connect.Routing.Connections;
using ICD.Connect.Routing.Controls;
using ICD.Connect.Routing.Endpoints;
using ICD.Connect.Settings;

namespace ICD.Connect.Routing.RoutingGraphs
{
	public sealed class ConnectionsCollection : AbstractOriginatorCollection<Connection>, IConnectionsCollection
	{
		private readonly SafeCriticalSection m_ConnectionsSection;

		/// <summary>
		/// Maps Device -> Control -> Address -> outgoing connections.
		/// </summary>
		private readonly Dictionary<DeviceControlInfo, Dictionary<int, Connection>> m_OutputConnectionLookup;

		/// <summary>
		/// Maps Device -> Control -> Address -> incoming connections.
		/// </summary>
		private readonly Dictionary<DeviceControlInfo, Dictionary<int, Connection>> m_InputConnectionLookup;

		/// <summary>
		/// Maps Source -> Final Destination -> Type -> Connection.
		/// </summary>
		private readonly Dictionary<EndpointInfo,
			Dictionary<EndpointInfo,
				Dictionary<eConnectionType, Connection>>> m_FilteredConnectionLookup;

		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="routingGraph"></param>
		public ConnectionsCollection(RoutingGraph routingGraph)
		{
			m_OutputConnectionLookup = new Dictionary<DeviceControlInfo, Dictionary<int, Connection>>();
			m_InputConnectionLookup = new Dictionary<DeviceControlInfo, Dictionary<int, Connection>>();
			m_FilteredConnectionLookup =
				new Dictionary<EndpointInfo,
					Dictionary<EndpointInfo,
						Dictionary<eConnectionType, Connection>>>();

			m_ConnectionsSection = new SafeCriticalSection();
		}

		#region Methods

		/// <summary>
		/// Gets the connection for the given endpoint.
		/// </summary>
		/// <param name="destination"></param>
		/// <returns></returns>
		[CanBeNull]
		public Connection GetInputConnection(EndpointInfo destination)
		{
			DeviceControlInfo key = new DeviceControlInfo(destination.Device, destination.Control);

			m_ConnectionsSection.Enter();

			try
			{
				Dictionary<int, Connection> map;
				return m_InputConnectionLookup.TryGetValue(key, out map)
					       ? map.GetDefault(destination.Address, null)
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
		/// <param name="destinationControl"></param>
		/// <param name="input"></param>
		/// <returns></returns>
		[CanBeNull]
		public Connection GetInputConnection(IRouteDestinationControl destinationControl, int input)
		{
			if (destinationControl == null)
				throw new ArgumentNullException("destinationControl");

			return GetInputConnection(destinationControl.GetInputEndpointInfo(input));
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
				Dictionary<int, Connection> map;
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
		/// Gets the input connections for the device matching any of the given type flags.
		/// </summary>
		/// <param name="destinationDeviceId"></param>
		/// <param name="destinationControlId"></param>
		/// <param name="type"></param>
		/// <returns></returns>
		public IEnumerable<Connection> GetInputConnectionsAny(int destinationDeviceId, int destinationControlId,
		                                                      eConnectionType type)
		{
			DeviceControlInfo info = new DeviceControlInfo(destinationDeviceId, destinationControlId);

			m_ConnectionsSection.Enter();

			try
			{
				Dictionary<int, Connection> map;
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
		/// <param name="source"></param>
		/// <returns></returns>
		[CanBeNull]
		public Connection GetOutputConnection(EndpointInfo source)
		{
			DeviceControlInfo key = new DeviceControlInfo(source.Device, source.Control);

			m_ConnectionsSection.Enter();

			try
			{
				Dictionary<int, Connection> map;
				return m_OutputConnectionLookup.TryGetValue(key, out map)
					       ? map.GetDefault(source.Address, null)
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
		/// <param name="sourceControl"></param>
		/// <param name="output"></param>
		/// <returns></returns>
		[CanBeNull]
		public Connection GetOutputConnection(IRouteSourceControl sourceControl, int output)
		{
			if (sourceControl == null)
				throw new ArgumentNullException("sourceControl");

			return GetOutputConnection(sourceControl.GetOutputEndpointInfo(output));
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
				Dictionary<int, Connection> map;
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
		/// <param name="type"></param>
		/// <returns></returns>
		public IEnumerable<Connection> GetOutputConnections(int sourceDeviceId, int sourceControlId, eConnectionType type)
		{
			DeviceControlInfo info = new DeviceControlInfo(sourceDeviceId, sourceControlId);

			m_ConnectionsSection.Enter();

			try
			{
				Dictionary<int, Connection> map;
				return m_OutputConnectionLookup.TryGetValue(info, out map)
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
		/// Gets the output connections for the given source device matching any of the given type flags.
		/// </summary>
		/// <param name="sourceDeviceId"></param>
		/// <param name="sourceControlId"></param>
		/// <param name="type"></param>
		/// <returns></returns>
		public IEnumerable<Connection> GetOutputConnectionsAny(int sourceDeviceId, int sourceControlId, eConnectionType type)
		{
			DeviceControlInfo info = new DeviceControlInfo(sourceDeviceId, sourceControlId);

			m_ConnectionsSection.Enter();

			try
			{
				Dictionary<int, Connection> map;
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

			m_ConnectionsSection.Enter();

			try
			{
				Dictionary<EndpointInfo, Dictionary<eConnectionType, Connection>> destinationMap;
				if (!m_FilteredConnectionLookup.TryGetValue(source, out destinationMap))
					return null;

				Dictionary<eConnectionType, Connection> connectionTypeMap;
				if (!destinationMap.TryGetValue(finalDestination, out connectionTypeMap))
					return null;

				return connectionTypeMap.GetDefault(flag, null);
			}
			finally
			{
				m_ConnectionsSection.Leave();
			}
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
			m_ConnectionsSection.Enter();

			try
			{
				foreach (Connection child in children)
				{
					DeviceControlInfo sourceInfo = new DeviceControlInfo(child.Source.Device, child.Source.Control);
					DeviceControlInfo destinationInfo = new DeviceControlInfo(child.Destination.Device,
					                                                          child.Destination.Control);

					// Add device controls to the maps
					if (!m_OutputConnectionLookup.ContainsKey(sourceInfo))
						m_OutputConnectionLookup.Add(sourceInfo, new Dictionary<int, Connection>());
					if (!m_InputConnectionLookup.ContainsKey(destinationInfo))
						m_InputConnectionLookup.Add(destinationInfo, new Dictionary<int, Connection>());

					// Add connections to the maps
					m_OutputConnectionLookup[sourceInfo][child.Source.Address] = child;
					m_InputConnectionLookup[destinationInfo][child.Destination.Address] = child;
				}

				RebuildFilteredConnectionsMap();
			}
			finally
			{
				m_ConnectionsSection.Leave();
			}
		}

		/// <summary>
		/// Called when children are removed from the collection before any events are raised.
		/// </summary>
		/// <param name="children"></param>
		protected override void ChildrenRemoved(IEnumerable<Connection> children)
		{
			m_ConnectionsSection.Enter();

			try
			{
				foreach (Connection child in children)
				{
					foreach (KeyValuePair<DeviceControlInfo, Dictionary<int, Connection>> kvp in m_OutputConnectionLookup)
						kvp.Value.RemoveAllValues(child);

					foreach (KeyValuePair<DeviceControlInfo, Dictionary<int, Connection>> kvp in m_InputConnectionLookup)
						kvp.Value.RemoveAllValues(child);
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

				// Add the source as a primary key
				foreach (Connection connection in connections)
				{
					if (!m_FilteredConnectionLookup.ContainsKey(connection.Source))
						m_FilteredConnectionLookup.Add(connection.Source,
						                               new Dictionary<EndpointInfo, Dictionary<eConnectionType, Connection>>());
				}

				// Add the destination to all of the existing keys
				foreach (EndpointInfo source in m_FilteredConnectionLookup.Keys)
				{
					foreach (Connection connection in connections)
					{
						if (!m_FilteredConnectionLookup[source].ContainsKey(connection.Destination))
							m_FilteredConnectionLookup[source].Add(connection.Destination, new Dictionary<eConnectionType, Connection>());
					}
				}

				// Perform the pathfinding
				foreach (EndpointInfo source in m_FilteredConnectionLookup.Keys.OrderBy(s => s.Device).ThenBy(s => s.Control).ThenBy(s => s.Address))
				{
					Connection outputConnection = GetOutputConnection(source);
					if (outputConnection == null)
						continue;

					foreach (EndpointInfo destination in m_FilteredConnectionLookup[source].Keys.OrderBy(d => d.Device).ThenBy(d => d.Control).ThenBy(d => d.Address))
					{
						Connection inputConnection = GetInputConnection(destination);
						if (inputConnection == null)
							continue;

						eConnectionType type =
							EnumUtils.GetFlagsIntersection(outputConnection.ConnectionType, inputConnection.ConnectionType);

						foreach (eConnectionType flag in EnumUtils.GetFlagsExceptNone(type))
						{
							// TODO - This is extremely lazy and inefficient
							if (!HasAnyPath(source, destination, flag))
								continue;

							m_FilteredConnectionLookup[source][destination].Add(flag, outputConnection);
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
		/// <param name="source"></param>
		/// <param name="destination"></param>
		/// <param name="flag"></param>
		/// <returns></returns>
		private bool HasAnyPath(EndpointInfo source, EndpointInfo destination, eConnectionType flag)
		{
			if (EnumUtils.HasMultipleFlags(flag))
				throw new ArgumentException("Connection type has multiple flags", "flag");

			Connection outputConnection = GetOutputConnection(source);
			if (outputConnection == null)
				return false;

			Connection inputConnection = GetInputConnection(destination);
			if (inputConnection == null)
				return false;

			return RecursionUtils.BreadthFirstSearch(outputConnection, inputConnection, c => GetChildren(c, flag));
		}

		/// <summary>
		/// Given an input connection returns the connected output connections.
		/// </summary>
		/// <param name="inputConnection"></param>
		/// <param name="flag"></param>
		/// <returns></returns>
		private IEnumerable<Connection> GetChildren(Connection inputConnection, eConnectionType flag)
		{
			if (inputConnection == null)
				throw new ArgumentNullException("inputConnection");

			if (EnumUtils.HasMultipleFlags(flag))
				throw new ArgumentException("Connection type has multiple flags", "flag");

			return GetOutputConnections(inputConnection.Destination.Device, inputConnection.Destination.Control, flag);
		}

		#endregion
	}
}
