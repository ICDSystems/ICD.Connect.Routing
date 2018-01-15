using System;
using System.Collections.Generic;
using System.Linq;
using ICD.Common.Utils;
using ICD.Common.Utils.Services;
using ICD.Connect.API.Commands;
using ICD.Connect.API.Nodes;
using ICD.Connect.Routing.Connections;
using ICD.Connect.Routing.ConnectionUsage;
using ICD.Connect.Routing.Controls;
using ICD.Connect.Routing.Endpoints;
using ICD.Connect.Routing.Endpoints.Destinations;
using ICD.Connect.Routing.Endpoints.Groups;
using ICD.Connect.Routing.Endpoints.Sources;
using ICD.Connect.Routing.EventArguments;
using ICD.Connect.Routing.StaticRoutes;
using ICD.Connect.Routing.Utils;
using ICD.Connect.Settings;

namespace ICD.Connect.Routing
{
	public abstract class AbstractRoutingGraph<TSettings> : AbstractOriginator<TSettings>, IRoutingGraph, IConsoleNode
		where TSettings : IRoutingGraphSettings, new()
	{
		public abstract event EventHandler<RouteFinishedEventArgs> OnRouteFinished;
		public abstract event EventHandler OnRouteChanged;
		public abstract event EventHandler<EndpointStateEventArgs> OnSourceTransmissionStateChanged;
		public abstract event EventHandler<EndpointStateEventArgs> OnSourceDetectionStateChanged;

		#region Properties

		public abstract IConnectionsCollection Connections { get; }
		public abstract IConnectionUsageCollection ConnectionUsages { get; }
		public abstract IStaticRoutesCollection StaticRoutes { get; }
		public abstract IOriginatorCollection<ISource> Sources { get; }
		public abstract IOriginatorCollection<IDestination> Destinations { get; }
		public abstract IOriginatorCollection<IDestinationGroup> DestinationGroups { get; }

		/// <summary>
		/// Gets the name of the node.
		/// </summary>
		public string ConsoleName { get { return Name ?? GetType().Name; } }

		/// <summary>
		/// Gets the help information for the node.
		/// </summary>
		public string ConsoleHelp { get { return "Maps the routing of device outputs to inputs."; } }

		#endregion

		/// <summary>
		/// Constructor.
		/// </summary>
		protected AbstractRoutingGraph()
		{
			ServiceProvider.AddService<IRoutingGraph>(this);
		}

		/// <summary>
		/// Override to release resources.
		/// </summary>
		/// <param name="disposing"></param>
		protected override void DisposeFinal(bool disposing)
		{
			base.DisposeFinal(disposing);

			ServiceProvider.RemoveService<IRoutingGraph>(this);
		}

		#region Methods

		/// <summary>
		/// Finds the actively routed sources for the destination at the given input address.
		/// Will return multiple items when connection types are combined, e.g. seperate audio and video sources.
		/// </summary>
		/// <param name="destinationInput"></param>
		/// <param name="type"></param>
		/// <param name="signalDetected">When true skips inputs where no video is detected.</param>
		/// <param name="inputActive"></param>
		/// <exception cref="ArgumentNullException"></exception>
		/// <returns>The sources</returns>
		public abstract IEnumerable<EndpointInfo> GetActiveSourceEndpoints(EndpointInfo destinationInput, eConnectionType type, bool signalDetected,
		                                                     bool inputActive);

		/// <summary>
		/// Finds the actively routed source for the destination at the given input address.
		/// </summary>
		/// <param name="destination"></param>
		/// <param name="input"></param>
		/// <param name="type"></param>
		/// <param name="signalDetected">When true skips inputs where no video is detected.</param>
		/// <param name="inputActive"></param>
		/// <exception cref="ArgumentNullException"></exception>
		/// <returns>The source</returns>
		public abstract EndpointInfo? GetActiveSourceEndpoint(IRouteDestinationControl destination, int input, eConnectionType type,
		                                                      bool signalDetected, bool inputActive);

		/// <summary>
		/// Finds the destinations that the source is actively routed to.
		/// </summary>
		/// <param name="sourceControl"></param>
		/// <param name="sourceOutput"></param>
		/// <param name="type"></param>
		/// <param name="signalDetected">When true skips inputs where no video is detected.</param>
		/// <param name="inputActive"></param>
		/// <exception cref="ArgumentNullException"></exception>
		/// <returns>The sources</returns>
		public abstract IEnumerable<EndpointInfo> GetActiveDestinationEndpoints(IRouteSourceControl sourceControl, int sourceOutput, eConnectionType type,
		                                                          bool signalDetected, bool inputActive);

		/// <summary>
		/// Recurses over all of the source devices that can be routed to the destination.
		/// </summary>
		/// <param name="destinationControl"></param>
		/// <param name="type"></param>
		/// <returns></returns>
		public abstract IEnumerable<IRouteSourceControl> GetSourceControlsRecursive(IRouteDestinationControl destinationControl, eConnectionType type);

		/// <summary>
		/// Simple check to see if the source is detected by the next node in the graph.
		/// </summary>
		/// <param name="sourceControl"></param>
		/// <param name="type"></param>
		/// <returns></returns>
		public abstract bool SourceDetected(IRouteSourceControl sourceControl, eConnectionType type);

		/// <summary>
		/// Returns true if the source is detected by the next node in the graph at the given output.
		/// </summary>
		/// <param name="sourceControl"></param>
		/// <param name="output"></param>
		/// <param name="type"></param>
		/// <returns></returns>
		public abstract bool SourceDetected(IRouteSourceControl sourceControl, int output, eConnectionType type);

		/// <summary>
		/// Returns true if there is a path from the given source to the given destination.
		/// </summary>
		/// <param name="source"></param>
		/// <param name="destination"></param>
		/// <param name="type"></param>
		/// <param name="roomId"></param>
		/// <returns></returns>
		public abstract bool HasPath(EndpointInfo source, EndpointInfo destination, eConnectionType type, int roomId);

		/// <summary>
		/// Finds the shortest available path from the source to the destination.
		/// </summary>
		/// <param name="source"></param>
		/// <param name="destination"></param>
		/// <param name="flag"></param>
		/// <param name="roomId"></param>
		public abstract ConnectionPath FindPath(EndpointInfo source, EndpointInfo destination, eConnectionType flag, int roomId);

		/// <summary>
		/// Returns the shortest paths from the source to the given destinations.
		/// </summary>
		/// <param name="source"></param>
		/// <param name="destinations"></param>
		/// <param name="flag"></param>
		/// <param name="roomId"></param>
		/// <returns></returns>
		public abstract IEnumerable<KeyValuePair<EndpointInfo, ConnectionPath>> FindPaths(EndpointInfo source, IEnumerable<EndpointInfo> destinations, eConnectionType flag, int roomId);

		/// <summary>
		/// Finds the current paths from the given source to the destination.
		/// Return multiple paths if multiple connection types are provided.
		/// </summary>
		/// <param name="source"></param>
		/// <param name="destination"></param>
		/// <param name="type"></param>
		/// <param name="signalDetected"></param>
		/// <param name="inputActive"></param>
		/// <returns></returns>
		public abstract IEnumerable<Connection[]> FindActivePaths(EndpointInfo source, EndpointInfo destination, eConnectionType type, bool signalDetected,
		                                            bool inputActive);

		/// <summary>
		/// Finds all of the active paths from the given source.
		/// </summary>
		/// <param name="source"></param>
		/// <param name="type"></param>
		/// <param name="signalDetected"></param>
		/// <param name="inputActive"></param>
		/// <returns></returns>
		public abstract IEnumerable<Connection[]> FindActivePaths(EndpointInfo source, eConnectionType type, bool signalDetected, bool inputActive);

		/// <summary>
		/// Routes the source to the destination.
		/// </summary>
		/// <param name="source"></param>
		/// <param name="destination"></param>
		/// <param name="type"></param>
		/// <param name="roomId"></param>
		/// <returns>False if route could not be established</returns>
		public abstract void Route(EndpointInfo source, EndpointInfo destination, eConnectionType type, int roomId);

		/// <summary>
		/// Routes the source to the destinations.
		/// </summary>
		/// <param name="source"></param>
		/// <param name="destinations"></param>
		/// <param name="type"></param>
		/// <param name="roomId"></param>
		public abstract void RouteMultiple(EndpointInfo source, IEnumerable<EndpointInfo> destinations, eConnectionType type, int roomId);

		/// <summary>
		/// Applies the given path to the switchers.
		/// </summary>
		/// <param name="op"></param>
		/// <param name="path"></param>
		public abstract void RoutePath(RouteOperation op, IEnumerable<Connection> path);

		/// <summary>
		/// Routes the source to the destination.
		/// </summary>
		/// <param name="sourceControl"></param>
		/// <param name="sourceAddress"></param>
		/// <param name="destinationControl"></param>
		/// <param name="destinationAddress"></param>
		/// <param name="type"></param>
		/// <param name="roomId"></param>
		/// <returns>False if route could not be established</returns>
		public abstract void Route(IRouteSourceControl sourceControl, int sourceAddress, IRouteDestinationControl destinationControl,
		                           int destinationAddress, eConnectionType type, int roomId);

		/// <summary>
		/// Performs the routing operation.
		/// </summary>
		/// <param name="op"></param>>
		/// <returns>False if route could not be established</returns>
		public abstract void Route(RouteOperation op);

		/// <summary>
		/// Searches for switchers currently routing the source to the destination and unroutes them.
		/// </summary>
		/// <param name="source"></param>
		/// <param name="destination"></param>
		/// <param name="type"></param>
		/// <param name="roomId"></param>
		/// <returns></returns>
		public abstract void Unroute(EndpointInfo source, EndpointInfo destination, eConnectionType type, int roomId);

		/// <summary>
		/// Searches for switchers currently routing the source and unroutes them.
		/// </summary>
		/// <param name="sourceControl"></param>
		/// <param name="type"></param>
		/// <param name="roomId"></param>
		/// <returns></returns>
		public abstract void Unroute(IRouteSourceControl sourceControl, eConnectionType type, int roomId);

		/// <summary>
		/// Searches for switchers currently routing the source and unroutes them.
		/// </summary>
		/// <param name="sourceControl"></param>
		/// <param name="sourceAddress"></param>
		/// <param name="type"></param>
		/// <param name="roomId"></param>
		public abstract void Unroute(IRouteSourceControl sourceControl, int sourceAddress, eConnectionType type, int roomId);

		/// <summary>
		/// Searches for switchers currently routing the source to the destination and unroutes them.
		/// </summary>
		/// <param name="sourceControl"></param>
		/// <param name="sourceAddress"></param>
		/// <param name="destinationControl"></param>
		/// <param name="destinationAddress"></param>
		/// <param name="type"></param>
		/// <param name="roomId"></param>
		/// <returns>False if the devices could not be unrouted.</returns>
		public abstract void Unroute(IRouteSourceControl sourceControl, int sourceAddress, IRouteDestinationControl destinationControl,
		                             int destinationAddress, eConnectionType type, int roomId);

		/// <summary>
		/// Unroutes every path from the given source to the destination.
		/// </summary>
		/// <param name="sourceControl"></param>
		/// <param name="sourceAddress"></param>
		/// <param name="destinationControl"></param>
		/// <param name="type"></param>
		/// <param name="roomId"></param>
		/// <returns>False if the devices could not be unrouted.</returns>
		public abstract void Unroute(IRouteSourceControl sourceControl, int sourceAddress, IRouteDestinationControl destinationControl,
		                             eConnectionType type, int roomId);

		/// <summary>
		/// Unroutes every path from the given source to the destination.
		/// </summary>
		/// <param name="sourceControl"></param>
		/// <param name="destinationControl"></param>
		/// <param name="type"></param>
		/// <param name="roomId"></param>
		/// <returns>False if the devices could not be unrouted.</returns>
		public abstract void Unroute(IRouteSourceControl sourceControl, IRouteDestinationControl destinationControl, eConnectionType type,
		                             int roomId);

		/// <summary>
		/// Unroutes the given connection path.
		/// </summary>
		/// <param name="path"></param>
		/// <param name="type"></param>
		/// <param name="roomId"></param>
		public abstract void Unroute(Connection[] path, eConnectionType type, int roomId);

		/// <summary>
		/// Gets the controls for the given connection.
		/// </summary>
		/// <param name="connection"></param>
		/// <returns></returns>
		public abstract IEnumerable<IRouteControl> GetControls(Connection connection);

		/// <summary>
		/// Gets the immediate destination control at the given address.
		/// </summary>
		/// <param name="sourceControl"></param>
		/// <param name="address"></param>
		/// <param name="type"></param>
		/// <param name="destinationInput"></param>
		/// <returns></returns>
		public abstract IRouteDestinationControl GetDestinationControl(IRouteSourceControl sourceControl, int address, eConnectionType type,
		                                                               out int destinationInput);

		public abstract IRouteDestinationControl GetDestinationControl(int device, int control);

		/// <summary>
		/// Returns the immediate source controls from [1 -> input count] inclusive, including nulls.
		/// </summary>
		/// <param name="destination"></param>
		/// <param name="type"></param>
		/// <returns></returns>
		public abstract IEnumerable<IRouteSourceControl> GetSourceControls(IRouteDestinationControl destination, eConnectionType type);

		/// <summary>
		/// Gets the immediate source control at the given address.
		/// </summary>
		/// <param name="destination"></param>
		/// <param name="address"></param>
		/// <param name="type"></param>
		/// <param name="sourceOutput"></param>
		/// <returns></returns>
		public abstract IRouteSourceControl GetSourceControl(IRouteDestinationControl destination, int address, eConnectionType type,
		                                                     out int sourceOutput);

		/// <summary>
		/// Gets the source control with the given device and control ids.
		/// </summary>
		/// <param name="device"></param>
		/// <param name="control"></param>
		/// <returns></returns>
		public abstract IRouteSourceControl GetSourceControl(int device, int control);

		#endregion

		#region Console

		/// <summary>
		/// Gets the child console nodes.
		/// </summary>
		/// <returns></returns>
		public virtual IEnumerable<IConsoleNodeBase> GetConsoleNodes()
		{
			yield break;
		}

		/// <summary>
		/// Calls the delegate for each console status item.
		/// </summary>
		/// <param name="addRow"></param>
		public virtual void BuildConsoleStatus(AddStatusRowDelegate addRow)
		{
		}

		/// <summary>
		/// Gets the child console commands.
		/// </summary>
		/// <returns></returns>
		public virtual IEnumerable<IConsoleCommand> GetConsoleCommands()
		{
			yield return
				new ConsoleCommand("PrintTable", "Prints a table of the routed devices and their input/output information.",
								   () => PrintTable());
			yield return new ConsoleCommand("PrintConnections", "Prints the list of all connections.", () => PrintConnections());
			yield return new ConsoleCommand("PrintSources", "Prints the list of Sources", () => PrintSources());
			yield return new ConsoleCommand("PrintDestinations", "Prints the list of Destinations", () => PrintDestinations());
			yield return new ConsoleCommand("PrintUsages", "Prints a table of the connection usages.", () => PrintUsages());

			yield return new GenericConsoleCommand<int, int, eConnectionType, int>("Route",
	"Routes source to destination. Usage: Route <sourceId> <destId> <connType> <roomId>",
	(a, b, c, d) => RouteConsoleCommand(a, b, c, d));
			yield return new GenericConsoleCommand<int, int, eConnectionType, int>("RouteGroup",
				"Routes source to destination group. Usage: Route <sourceId> <destGrpId> <connType> <roomId>",
				(a, b, c, d) => RouteGroupConsoleCommand(a, b, c, d));
		}

		private string PrintSources()
		{
			TableBuilder builder = new TableBuilder("Id", "Source");

			foreach (var source in Sources.GetChildren().OrderBy(c => c.Id))
				builder.AddRow(source.Id, source);

			return builder.ToString();
		}

		private string PrintDestinations()
		{
			TableBuilder builder = new TableBuilder("Id", "Destination");

			foreach (var destination in Destinations.GetChildren().OrderBy(c => c.Id))
				builder.AddRow(destination.Id, destination);

			return builder.ToString();
		}

		/// <summary>
		/// Loop over the devices, build a table of inputs, outputs, and their statuses.
		/// </summary>
		private string PrintTable()
		{
			RoutingGraphTableBuilder builder = new RoutingGraphTableBuilder(this);
			return builder.ToString();
		}

		private string PrintConnections()
		{
			TableBuilder builder = new TableBuilder("Source", "Output", "Destination", "Input", "Type");

			foreach (var con in Connections.GetConnections().OrderBy(c => c.Source.Device).ThenBy(c => c.Source.Address))
				builder.AddRow(con.Source, con.Source.Address, con.Destination, con.Destination.Address, con.ConnectionType);

			return builder.ToString();
		}

		/// <summary>
		/// Loop over the connections and build a table of usages.
		/// </summary>
		private string PrintUsages()
		{
			TableBuilder builder = new TableBuilder("Connection", "Type", "Source", "Rooms");

			Connection[] connections = Connections.ToArray();

			for (int index = 0; index < connections.Length; index++)
			{
				Connection connection = connections[index];
				ConnectionUsageInfo info = ConnectionUsages.GetConnectionUsageInfo(connection);
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

		private string RouteConsoleCommand(int source, int destination, eConnectionType connectionType, int roomId)
		{
			if (!Sources.ContainsChild(source) || !Destinations.ContainsChild(destination))
				return "Krang does not contains a source or destination with that id";

			Route(Sources.GetChild(source), Destinations.GetChild(destination), connectionType, roomId);

			return "Sucessfully executed route command";
		}

		private string RouteGroupConsoleCommand(int source, int destination, eConnectionType connectionType, int roomId)
		{
			if (!Sources.ContainsChild(source) || !DestinationGroups.ContainsChild(destination))
				return "Krang does not contains a source or destination group with that id";

			Route(Sources.GetChild(source), DestinationGroups.GetChild(destination), connectionType, roomId);

			return "Sucessfully executed route command";
		}

		private void Route(ISource source, IDestination destination, eConnectionType connectionType, int roomId)
		{
			RouteOperation operation = new RouteOperation
			{
				Source = source.Endpoint,
				Destination = destination.Endpoint,
				ConnectionType = connectionType,
				RoomId = roomId
			};

			Route(operation);
		}

		private void Route(ISource source, IDestinationGroup destinationGroup, eConnectionType connectionType, int roomId)
		{
			foreach (var destination in destinationGroup.Destinations.Where(Destinations.ContainsChild).Select(d => Destinations.GetChild(d)))
			{
				IDestination destination1 = destination;
				ThreadingUtils.SafeInvoke(() => Route(source, destination1, connectionType, roomId));
			}
		}

		#endregion
	}
}
