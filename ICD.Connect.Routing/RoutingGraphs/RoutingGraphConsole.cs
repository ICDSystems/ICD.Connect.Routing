using System;
using System.Collections.Generic;
using System.Linq;
using ICD.Common.Utils;
using ICD.Connect.API.Commands;
using ICD.Connect.API.Nodes;
using ICD.Connect.Routing.Connections;
using ICD.Connect.Routing.ConnectionUsage;
using ICD.Connect.Routing.Endpoints;
using ICD.Connect.Routing.Endpoints.Destinations;
using ICD.Connect.Routing.Endpoints.Groups;
using ICD.Connect.Routing.Endpoints.Sources;
using ICD.Connect.Routing.Pathfinding;
using ICD.Connect.Routing.Utils;
using ICD.Connect.Settings;

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

			yield break;
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
				new ConsoleCommand("PrintConnections", "Prints the list of all connections.", () => PrintConnections(instance));
			yield return new ConsoleCommand("PrintSources", "Prints the list of Sources", () => PrintSources(instance));
			yield return
				new ConsoleCommand("PrintDestinations", "Prints the list of Destinations", () => PrintDestinations(instance));
			yield return
				new ConsoleCommand("PrintPathable", "Prints a table of sources to routable destinations",
				                   () => PrintPathable(instance));
			yield return
				new ConsoleCommand("PrintUsages", "Prints a table of the connection usages.", () => PrintUsages(instance));

			yield return new GenericConsoleCommand<int, int, eConnectionType, int>("Route",
			                                                                       "Routes source to destination. Usage: Route <sourceId> <destId> <connType> <roomId>",
			                                                                       (a, b, c, d) =>
			                                                                       RouteConsoleCommand(instance, a, b, c, d));
			yield return new GenericConsoleCommand<int, int, eConnectionType, int>("RouteGroup",
			                                                                       "Routes source to destination group. Usage: Route <sourceId> <destGrpId> <connType> <roomId>",
			                                                                       (a, b, c, d) =>
			                                                                       RouteGroupConsoleCommand(instance, a, b, c, d));
		}

		private static string PrintSources(IRoutingGraph instance)
		{
			if (instance == null)
				throw new ArgumentNullException("instance");

			TableBuilder builder = new TableBuilder("Id", "Source");

			foreach (ISource source in instance.Sources.GetChildren().OrderBy(c => c.Id))
				builder.AddRow(source.Id, source);

			return builder.ToString();
		}

		private static string PrintDestinations(IRoutingGraph instance)
		{
			if (instance == null)
				throw new ArgumentNullException("instance");

			TableBuilder builder = new TableBuilder("Id", "Destination");

			foreach (IDestination destination in instance.Destinations.GetChildren().OrderBy(c => c.Id))
				builder.AddRow(destination.Id, destination);

			return builder.ToString();
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

		private static string PrintConnections(IRoutingGraph instance)
		{
			if (instance == null)
				throw new ArgumentNullException("instance");

			TableBuilder builder = new TableBuilder("Source", "Output", "Destination", "Input", "Type");

			foreach (Connection con in instance.Connections.GetChildren().OrderBy(c => c.Source.Device).ThenBy(c => c.Source.Address))
				builder.AddRow(con.Source, con.Source.Address, con.Destination, con.Destination.Address, con.ConnectionType);

			return builder.ToString();
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

		/// <summary>
		/// Loop over the connections and build a table of usages.
		/// </summary>
		private static string PrintUsages(IRoutingGraph instance)
		{
			if (instance == null)
				throw new ArgumentNullException("instance");

			TableBuilder builder = new TableBuilder("Connection", "Type", "Source", "Rooms");

			Connection[] connections = instance.Connections.ToArray();

			for (int index = 0; index < connections.Length; index++)
			{
				Connection connection = connections[index];
				ConnectionUsageInfo info = instance.ConnectionUsages.GetConnectionUsageInfo(connection);
				int row = 0;

				foreach (eConnectionType type in EnumUtils.GetFlagsExceptNone(connection.ConnectionType))
				{
					string connectionString = row == 0 ? string.Format("{0} - {1}", connection.Id, connection.Name) : string.Empty;
					EndpointInfo? source = info.GetSource(type);
					int[] rooms = info.GetRooms(type).ToArray();
					string roomsString = rooms.Length == 0 ? string.Empty : StringUtils.ArrayFormat(rooms);

					builder.AddRow(connectionString, type, source, roomsString);

					row++;
				}

				if (index < connections.Length - 1)
					builder.AddSeparator();
			}

			return builder.ToString();
		}

		private static string RouteConsoleCommand(IRoutingGraph instance, int source, int destination, eConnectionType connectionType, int roomId)
		{
			if (instance == null)
				throw new ArgumentNullException("instance");

			if (!instance.Sources.ContainsChild(source) || !instance.Destinations.ContainsChild(destination))
				return "There is no source or destination with that id";

			IPathFinder pathFinder = new DefaultPathFinder(instance, roomId);

			IEnumerable<ConnectionPath> paths =
				PathBuilder.FindPaths()
				           .From(instance.Sources.GetChild(source))
				           .To(instance.Destinations.GetChild(destination))
				           .OfType(connectionType)
				           .With(pathFinder);

			instance.RoutePaths(paths, roomId);

			return "Sucessfully executed route command";
		}

		private static string RouteGroupConsoleCommand(IRoutingGraph instance, int source, int destinationGroup, eConnectionType connectionType, int roomId)
		{
			if (instance == null)
				throw new ArgumentNullException("instance");

			if (!instance.Sources.ContainsChild(source) || !instance.DestinationGroups.ContainsChild(destinationGroup))
				return "There is no source or destination group with that id";

			Route(instance, instance.Sources.GetChild(source), instance.DestinationGroups.GetChild(destinationGroup), connectionType, roomId);

			return "Sucessfully executed route command";
		}

		private static void Route(IRoutingGraph instance, ISource source, IDestinationGroup destinationGroup, eConnectionType connectionType, int roomId)
		{
			if (instance == null)
				throw new ArgumentNullException("instance");

			if (source == null)
				throw new ArgumentNullException("source");

			if (destinationGroup == null)
				throw new ArgumentNullException("destinationGroup");

			IEnumerable<IDestination> destinations =
				destinationGroup.Destinations
				                .Where(instance.Destinations.ContainsChild)
				                .Select(d => instance.Destinations.GetChild(d));

			IPathFinder pathFinder = new DefaultPathFinder(instance, roomId);

			IEnumerable<ConnectionPath> paths =
				PathBuilder.FindPaths()
				           .From(source)
				           .To(destinations)
				           .OfType(connectionType)
				           .With(pathFinder);

			instance.RoutePaths(paths, roomId);
		}
	}
}
