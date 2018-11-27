using System;
using System.Collections.Generic;
using System.Linq;
using ICD.Common.Utils;
using ICD.Connect.API.Commands;
using ICD.Connect.API.Nodes;
using ICD.Connect.Routing.Connections;

namespace ICD.Connect.Routing.Controls
{
	public static class RouteMidpointControlConsole
	{
		/// <summary>
		/// Gets the child console nodes.
		/// </summary>
		/// <param name="instance"></param>
		/// <returns></returns>
		public static IEnumerable<IConsoleNodeBase> GetConsoleNodes(IRouteMidpointControl instance)
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
		public static void BuildConsoleStatus(IRouteMidpointControl instance, AddStatusRowDelegate addRow)
		{
			if (instance == null)
				throw new ArgumentNullException("instance");
		}

		/// <summary>
		/// Gets the child console commands.
		/// </summary>
		/// <param name="instance"></param>
		/// <returns></returns>
		public static IEnumerable<IConsoleCommand> GetConsoleCommands(IRouteMidpointControl instance)
		{
			if (instance == null)
				throw new ArgumentNullException("instance");

			yield return
				new ConsoleCommand("PrintRouting", "Prints a table of the current routing state",
				                   () => PrintRouteStatusTable(instance));

		}

		private static string PrintRouteStatusTable(IRouteMidpointControl instance)
		{
			TableBuilder builder = new TableBuilder("Output", "Type", "Input");

			foreach (ConnectorInfo output in instance.GetOutputs().OrderBy(c => c.Address))
			{
				bool first = true;

				foreach (eConnectionType flag in EnumUtils.GetFlagsExceptNone(output.ConnectionType))
				{
					string outputString = first ? output.Address.ToString() : string.Empty;
					first = false;

					string typeString = flag.ToString();

					ConnectorInfo? input = instance.GetInput(output.Address, flag);
					string inputString = input.HasValue ? input.Value.Address.ToString() : null;

					builder.AddRow(outputString, typeString, inputString);
				}
			}

			return builder.ToString();
		}
	}
}
