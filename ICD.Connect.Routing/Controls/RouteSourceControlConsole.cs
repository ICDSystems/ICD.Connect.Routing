using System;
using System.Collections.Generic;
using ICD.Common.Utils;
using ICD.Connect.API.Commands;
using ICD.Connect.API.Nodes;
using ICD.Connect.Routing.Connections;

namespace ICD.Connect.Routing.Controls
{
	public static class RouteSourceControlConsole
	{
		/// <summary>
		/// Gets the child console nodes.
		/// </summary>
		/// <param name="instance"></param>
		/// <returns></returns>
		public static IEnumerable<IConsoleNodeBase> GetConsoleNodes(IRouteSourceControl instance)
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
		public static void BuildConsoleStatus(IRouteSourceControl instance, AddStatusRowDelegate addRow)
		{
			if (instance == null)
				throw new ArgumentNullException("instance");
		}

		/// <summary>
		/// Gets the child console commands.
		/// </summary>
		/// <param name="instance"></param>
		/// <returns></returns>
		public static IEnumerable<IConsoleCommand> GetConsoleCommands(IRouteSourceControl instance)
		{
			if (instance == null)
				throw new ArgumentNullException("instance");

			yield return new ConsoleCommand("PrintOutputsTransmission", "Prints a table of the active transmission state for each output", () => PrintOutputsTransmission(instance));
		}

		private static string PrintOutputsTransmission(IRouteSourceControl instance)
		{
			TableBuilder builder = new TableBuilder("Output", "Type", "Transmitting");

			foreach (ConnectorInfo output in instance.GetOutputs())
			{
				bool first = true;

				foreach (eConnectionType flag in EnumUtils.GetFlagsExceptNone(output.ConnectionType))
				{
					bool transmitting = instance.GetActiveTransmissionState(output.Address, flag);

					string address = first ? output.Address.ToString() : null;
					string transmittingName = transmitting ? true.ToString() : null;

					builder.AddRow(address, flag, transmittingName);

					first = false;
				}
			}

			return builder.ToString();
		}
	}
}
