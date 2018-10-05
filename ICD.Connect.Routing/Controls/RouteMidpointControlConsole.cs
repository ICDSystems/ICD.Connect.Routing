using System;
using System.Collections.Generic;
using System.Linq;
using ICD.Common.Utils;
using ICD.Common.Utils.Extensions;
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
			TableBuilder builder = new TableBuilder("Input", "Type", "Detected", "Outputs");

			foreach (ConnectorInfo input in instance.GetInputs().OrderBy(c => c.Address))
			{
				bool first = true;

				foreach (eConnectionType type in EnumUtils.GetFlagsExceptNone(input.ConnectionType))
				{
					string inputString = first ? input.Address.ToString() : string.Empty;
					first = false;

					string typeString = type.ToString();
					string detectedString = instance.GetSignalDetectedState(input.Address, type) ? "True" : string.Empty;
					string outputsString = StringUtils.ArrayFormat(instance.GetOutputs(input.Address, type).Order());

					builder.AddRow(inputString, typeString, detectedString, outputsString);
				}
			}

			return builder.ToString();
		}
	}
}
