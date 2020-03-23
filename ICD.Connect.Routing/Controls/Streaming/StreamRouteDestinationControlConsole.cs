using System;
using System.Collections.Generic;
using ICD.Common.Utils;
using ICD.Connect.API.Commands;
using ICD.Connect.API.Nodes;

namespace ICD.Connect.Routing.Controls.Streaming
{
	public static class StreamRouteDestinationControlConsole
	{
		/// <summary>
		/// Gets the child console nodes.
		/// </summary>
		/// <param name="instance"></param>
		/// <returns></returns>
		public static IEnumerable<IConsoleNodeBase> GetConsoleNodes(IStreamRouteDestinationControl instance)
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
		public static void BuildConsoleStatus(IStreamRouteDestinationControl instance, AddStatusRowDelegate addRow)
		{
			if (instance == null)
				throw new ArgumentNullException("instance");
		}

		/// <summary>
		/// Gets the child console commands.
		/// </summary>
		/// <param name="instance"></param>
		/// <returns></returns>
		public static IEnumerable<IConsoleCommand> GetConsoleCommands(IStreamRouteDestinationControl instance)
		{
			if (instance == null)
				throw new ArgumentNullException("instance");

			yield return new ConsoleCommand("PrintInputStreams", "Prints a table of the input stream for each input", () => PrintInputStreams(instance));
		}

		private static string PrintInputStreams(IStreamRouteDestinationControl instance)
		{
			TableBuilder builder = new TableBuilder("Output", "Stream URI");

			foreach (ConnectorInfo input in instance.GetInputs())
				builder.AddRow(input.Address, instance.GetStreamForInput(input.Address));

			return builder.ToString();
		}
	}
}
