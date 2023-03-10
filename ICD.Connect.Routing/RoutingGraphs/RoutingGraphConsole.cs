using System;
using System.Collections.Generic;
using System.Linq;
using ICD.Common.Utils;
using ICD.Connect.API.Commands;
using ICD.Connect.API.Nodes;
using ICD.Connect.Routing.Connections;
using ICD.Connect.Routing.Controls;
using ICD.Connect.Routing.Endpoints;
using ICD.Connect.Routing.Endpoints.Destinations;
using ICD.Connect.Routing.Endpoints.Sources;
using ICD.Connect.Routing.PathFinding;
using ICD.Connect.Routing.Utils;
using ICD.Connect.Settings.Originators;

namespace ICD.Connect.Routing.RoutingGraphs
{
	public static class RoutingGraphConsole
	{
		/// <summary>
		/// Gets the child console nodes.
		/// </summary>
		/// <param name="instance"></param>
		/// <returns></returns>
		public static IEnumerable<IConsoleNodeBase> GetConsoleNodes(IRoutingGraph instance)
		{
			if (instance == null)
				throw new ArgumentNullException("instance");

			yield return instance.RoutingCache;
			yield return ConsoleNodeGroup.KeyNodeMap("Connections", instance.Connections, c => (uint)c.Id);
			yield return ConsoleNodeGroup.KeyNodeMap("Sources", instance.Sources, s => (uint)s.Id);
			yield return ConsoleNodeGroup.KeyNodeMap("Destinations", instance.Destinations, d => (uint)d.Id);
			yield return ConsoleNodeGroup.KeyNodeMap("SourceGroups", instance.SourceGroups, s => (uint)s.Id);
			yield return ConsoleNodeGroup.KeyNodeMap("DestinationGroups", instance.DestinationGroups, d => (uint)d.Id);
		}

		/// <summary>
		/// Calls the delegate for each console status item.
		/// </summary>
		/// <param name="instance"></param>
		/// <param name="addRow"></param>
		public static void BuildConsoleStatus(IRoutingGraph instance, AddStatusRowDelegate addRow)
		{
			if (instance == null)
				throw new ArgumentNullException("instance");
		}

		/// <summary>
		/// Gets the child console commands.
		/// </summary>
		/// <param name="instance"></param>
		/// <returns></returns>
		public static IEnumerable<IConsoleCommand> GetConsoleCommands(IRoutingGraph instance)
		{
			if (instance == null)
				throw new ArgumentNullException("instance");

			yield return
				new ConsoleCommand("PrintTable", "Prints a table of the routed devices and their input/output information.",
				                   () => PrintTable(instance));

			yield return
				new ConsoleCommand("PrintPathable", "Prints a table of sources to routable destinations",
				                   () => PrintPathable(instance));

			yield return new GenericConsoleCommand<int>("PrintDestinationPaths", "PrintDestinationPaths <DESTINATION>",
											id => PrintDestinationPaths(instance, id));

			yield return new GenericConsoleCommand<int, int, eConnectionType, int>("Route",
			                                                                       "Routes source to destination. Usage: Route <sourceId> <destId> <connType> <roomId>",
			                                                                       (a, b, c, d) =>
			                                                                       RouteConsoleCommand(instance, a, b, c, d));

			yield return new GenericConsoleCommand<int, int, eConnectionType, int>("TestRoute",
																							   "Prints route path, but doesn't execute it. Usage: TestRoute <sourceId> <destId> <connType> <roomId>",
																							   (a, b, c, d) =>
																							   TestRouteConsoleCommand(instance, a, b, c, d));


			yield return new GenericConsoleCommand<int, int, int, int>("AB",
																	   "AB <SourceDeviceId> <SourceAddress> <DestinationDeviceId> <DestinationAddress>",
			                                                           (a, b, c, d) => RouteDevicesConsoleCommand(instance, a, b, c, d));
		}

		/// <summary>
		/// Loop over the devices, build a table of inputs, outputs, and their statuses.
		/// </summary>
		private static string PrintTable(IRoutingGraph instance)
		{
			if (instance == null)
				throw new ArgumentNullException("instance");

			return new RoutingGraphTableBuilder(instance).ToString();
		}

		private static string PrintPathable(IRoutingGraph instance)
		{
			if (instance == null)
				throw new ArgumentNullException("instance");

			TableBuilder builder = new TableBuilder("Source", "Type", "Destination");

			ISource[] sources = instance.Sources.ToArray();

			for (int index = 0; index < sources.Length; index++)
			{
				ISource source = sources[index];

				bool writeSource = true;

				foreach (eConnectionType flag in EnumUtils.GetFlagsExceptNone<eConnectionType>())
				{
					bool writeFlag = true;

					foreach (IDestination destination in instance.Destinations)
					{
						IDestination destinationCopy = destination;

						bool canRoute = source.GetEndpoints()
						                      .Any(sourceEndpoint =>
						                           destinationCopy.GetEndpoints()
						                                          .Any(destinationEndpoint =>
						                                               instance.Connections.GetOutputConnection(sourceEndpoint,
						                                                                                        destinationEndpoint, flag) !=
						                                               null)
							);

						if (!canRoute)
							continue;

						string sourceLabel = writeSource ? source.ToStringShorthand() : null;
						string flagLabel = writeFlag ? flag.ToString() : null;
						string destinationLabel = destination.ToStringShorthand();

						builder.AddRow(sourceLabel, flagLabel, destinationLabel);

						writeSource = false;
						writeFlag = false;
					}
				}

				// Put a separator between the sources
				if (!writeSource && index < sources.Length - 1)
					builder.AddSeparator();
			}

			return builder.ToString();
		}

		private static string PrintDestinationPaths(IRoutingGraph instance, int id)
		{
			if (instance == null)
				throw new ArgumentNullException("instance");

			TableBuilder builder = new TableBuilder("Input", "Type", "Active", "Path");

			IDestination destination = instance.Destinations.GetChild(id);
			IRouteDestinationControl control = instance.GetDestinationControl(destination.Device, destination.Control);

			foreach (int input in destination.GetAddresses())
			{
				string inputString = input.ToString();

				foreach (eConnectionType flag in EnumUtils.GetFlagsExceptNone(destination.ConnectionType))
				{
					string flagString = flag.ToString();
					string activeString = control.GetInputActiveState(input, flag).ToString();

					EndpointInfo destinationEndpoint = control.GetInputEndpointInfo(input);

					Connection[] connections = GetInputConnectionsRecursive(instance, destinationEndpoint, flag).ToArray();

					if (connections.Length == 0)
					{
						builder.AddRow(inputString, flagString, activeString, null);
						inputString = null;
						continue;
					}

					foreach (Connection connection in connections)
					{
						builder.AddRow(inputString, flagString, activeString, connection);

						inputString = null;
						flagString = null;
						activeString = null;
					}
				}
			}

			return builder.ToString();
		}

		private static IEnumerable<Connection> GetInputConnectionsRecursive(IRoutingGraph instance,
		                                                                    EndpointInfo destinationEndpoint,
		                                                                    eConnectionType flag)
		{
			while (true)
			{
				Connection connection = instance.Connections.GetInputConnection(destinationEndpoint);
				if (connection == null)
					yield break;

				yield return connection;

				IRouteMidpointControl midpoint = instance.GetSourceControl(connection) as IRouteMidpointControl;
				if (midpoint == null)
					yield break;

				ConnectorInfo? input = midpoint.GetInput(connection.Source.Address, flag);
				if (!input.HasValue)
					yield break;

				destinationEndpoint = new EndpointInfo(midpoint.Parent.Id, midpoint.Id, input.Value.Address);
			}
		}

		private static string TestRouteConsoleCommand(IRoutingGraph instance, int source, int destination,
		                                              eConnectionType connectionType, int roomId)
		{
			if (instance == null)
				throw new ArgumentNullException("instance");

			if (!instance.Sources.ContainsChild(source) || !instance.Destinations.ContainsChild(destination))
				return "There is no source or destination with that id";

			IEnumerable<ConnectionPath> paths = GetConnectionPaths(instance, source, destination, connectionType, roomId);

			TableBuilder response = new TableBuilder("Type", "SourceDevice","SourceControl", "SourceAddress", "DestDevice", "DestControl", "DestAddress");

			foreach (ConnectionPath path in paths)
			{
				response.AddHeader("", "", "", "", "", "", "");
				response.AddRow(path.ConnectionType,
					path.SourceEndpoint.Device, path.SourceEndpoint.Control, path.SourceEndpoint.Address,
				    path.DestinationEndpoint.Device, path.DestinationEndpoint.Control, path.DestinationEndpoint.Address);
				response.AddSeparator();
				foreach (Connection connection in path)
					response.AddRow(connection.ConnectionType, connection.Source.Device, connection.Source.Control,
					                connection.Source.Address, connection.Destination.Device, connection.Destination.Control,
					                connection.Destination.Address);
			}

			return response.ToString();
		}

		private static string RouteConsoleCommand(IRoutingGraph instance, int source, int destination, eConnectionType connectionType, int roomId)
		{
			if (instance == null)
				throw new ArgumentNullException("instance");

			if (!instance.Sources.ContainsChild(source) || !instance.Destinations.ContainsChild(destination))
				return "There is no source or destination with that id";

			IEnumerable<ConnectionPath> paths = GetConnectionPaths(instance, source, destination, connectionType, roomId);
			instance.RoutePaths(paths, roomId);

			return "Sucessfully executed route command";
		}

		private static IEnumerable<ConnectionPath> GetConnectionPaths(IRoutingGraph instance, int source, int destination,
		                                                              eConnectionType connectionType, int roomId)
		{
			IPathFinder pathFinder = new DefaultPathFinder(instance, roomId);

			return PathBuilder.FindPaths()
						   .From(instance.Sources.GetChild(source))
						   .To(instance.Destinations.GetChild(destination))
						   .OfType(connectionType)
						   .With(pathFinder);
		}

		private static string RouteDevicesConsoleCommand(IRoutingGraph instance, int sourceDeviceId, int sourceAddress,
		                                                 int destinationDeviceId, int destinationAddress)
		{
			if (instance == null)
				throw new ArgumentNullException("instance");

			// Not great, but currently unused anyway
			const int roomId = 0;

			IPathFinder pathFinder = new DefaultPathFinder(instance, roomId);

			IRouteSourceControl sourceControl = instance.GetSourceControl(sourceDeviceId, 0);
			IRouteDestinationControl destinationControl = instance.GetDestinationControl(destinationDeviceId, 0);

			ConnectorInfo outputConnector = sourceControl.GetOutput(sourceAddress);
			ConnectorInfo inputConnector = destinationControl.GetInput(destinationAddress);

			eConnectionType type = EnumUtils.GetFlagsIntersection(inputConnector.ConnectionType, outputConnector.ConnectionType);

			IEnumerable<ConnectionPath> paths =
				PathBuilder.FindPaths()
						   .From(sourceControl.GetOutputEndpointInfo(sourceAddress))
						   .To(destinationControl.GetInputEndpointInfo(destinationAddress))
						   .OfType(type)
						   .With(pathFinder);

			instance.RoutePaths(paths, roomId);

			return "Sucessfully executed route command";
		}
	}
}
