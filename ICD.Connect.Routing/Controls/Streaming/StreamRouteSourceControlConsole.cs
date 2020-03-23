using System;
using System.Collections.Generic;
using ICD.Common.Utils;
using ICD.Connect.API.Commands;
using ICD.Connect.API.Nodes;

namespace ICD.Connect.Routing.Controls.Streaming
{
    public static class StreamRouteSourceControlConsole
    {
	    /// <summary>
	    /// Gets the child console nodes.
	    /// </summary>
	    /// <param name="instance"></param>
	    /// <returns></returns>
	    public static IEnumerable<IConsoleNodeBase> GetConsoleNodes(IStreamRouteSourceControl instance)
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
	    public static void BuildConsoleStatus(IStreamRouteSourceControl instance, AddStatusRowDelegate addRow)
	    {
		    if (instance == null)
			    throw new ArgumentNullException("instance");
	    }

	    /// <summary>
	    /// Gets the child console commands.
	    /// </summary>
	    /// <param name="instance"></param>
	    /// <returns></returns>
	    public static IEnumerable<IConsoleCommand> GetConsoleCommands(IStreamRouteSourceControl instance)
	    {
		    if (instance == null)
			    throw new ArgumentNullException("instance");

		    yield return new ConsoleCommand("PrintOutputStreams", "Prints a table of the output stream  for each output", () => PrintOutputStreams(instance));
	    }

	    private static string PrintOutputStreams(IStreamRouteSourceControl instance)
	    {
		    TableBuilder builder = new TableBuilder("Output", "Stream URI");

		    foreach (ConnectorInfo output in instance.GetOutputs())
			    builder.AddRow(output.Address, instance.GetStreamForOutput(output.Address));

		    return builder.ToString();
	    }
    }
}
