using System;
using System.Collections.Generic;
using System.Linq;
using ICD.Common.Utils;
using ICD.Connect.API.Commands;
using ICD.Connect.API.Nodes;
using ICD.Connect.Routing.Connections;

namespace ICD.Connect.Routing.Controls
{
	public static class RouteDestinationControlConsole
	{
		/// <summary>
		/// Gets the child console nodes.
		/// </summary>
		/// <param name="instance"></param>
		/// <returns></returns>
		public static IEnumerable<IConsoleNodeBase> GetConsoleNodes(IRouteDestinationControl instance)
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
		public static void BuildConsoleStatus(IRouteDestinationControl instance, AddStatusRowDelegate addRow)
		{
			if (instance == null)
				throw new ArgumentNullException("instance");
		}

		/// <summary>
		/// Gets the child console commands.
		/// </summary>
		/// <param name="instance"></param>
		/// <returns></returns>
		public static IEnumerable<IConsoleCommand> GetConsoleCommands(IRouteDestinationControl instance)
		{
			if (instance == null)
				throw new ArgumentNullException("instance");

			yield return new ConsoleCommand("PrintSignalDetection", "Prints a table of the source detection state for each input", () => PrintSignalDetection(instance));
			yield return new ConsoleCommand("PrintInputsActive", "Prints a table of the active state for each input", () => PrintInputsActive(instance));
		}

		/// <summary>
		/// Prints a table of the source detection state for each input.
		/// </summary>
		/// <param name="instance"></param>
		/// <returns></returns>
		private static string PrintSignalDetection(IRouteDestinationControl instance)
		{
			TableBuilder builder = new TableBuilder("Input", "Type", "Detected");

			foreach (ConnectorInfo input in instance.GetInputs().OrderBy(c => c.Address))
			{
				bool first = true;

				foreach (eConnectionType flag in EnumUtils.GetFlagsExceptNone(input.ConnectionType))
				{
					bool detected = instance.GetSignalDetectedState(input.Address, flag);

					string address = first ? input.Address.ToString() : null;
					string detectedName = detected ? true.ToString() : null;

					builder.AddRow(address, flag, detectedName);

					first = false;
				}
			}

			return builder.ToString();
		}

		/// <summary>
		/// Prints a table of the input active state for each input.
		/// </summary>
		/// <param name="instance"></param>
		/// <returns></returns>
		private static string PrintInputsActive(IRouteDestinationControl instance)
		{
			TableBuilder builder = new TableBuilder("Input", "Type", "Active");

			foreach (ConnectorInfo input in instance.GetInputs().OrderBy(c => c.Address))
			{
				bool first = true;

				foreach (eConnectionType flag in EnumUtils.GetFlagsExceptNone(input.ConnectionType))
				{
					bool inputActive = instance.GetInputActiveState(input.Address, flag);

					string address = first ? input.Address.ToString() : null;
					string inputActiveName = inputActive ? true.ToString() : null;

					builder.AddRow(address, flag, inputActiveName);

					first = false;
				}
			}

			return builder.ToString();
		}
	}
}
