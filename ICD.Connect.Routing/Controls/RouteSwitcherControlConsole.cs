using System;
using System.Collections.Generic;
using ICD.Common.Utils;
using ICD.Connect.API.Commands;
using ICD.Connect.API.Nodes;
using ICD.Connect.Routing.Connections;

namespace ICD.Connect.Routing.Controls
{
	public static class RouteSwitcherControlConsole
	{
		/// <summary>
		/// Gets the child console nodes.
		/// </summary>
		/// <param name="instance"></param>
		/// <returns></returns>
		public static IEnumerable<IConsoleNodeBase> GetConsoleNodes(IRouteSwitcherControl instance)
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
		public static void BuildConsoleStatus(IRouteSwitcherControl instance, AddStatusRowDelegate addRow)
		{
			if (instance == null)
				throw new ArgumentNullException("instance");
		}

		/// <summary>
		/// Gets the child console commands.
		/// </summary>
		/// <param name="instance"></param>
		/// <returns></returns>
		public static IEnumerable<IConsoleCommand> GetConsoleCommands(IRouteSwitcherControl instance)
		{
			if (instance == null)
				throw new ArgumentNullException("instance");

			string connectionTypes = StringUtils.ArrayFormat(EnumUtils.GetValuesExceptNone<eConnectionType>());

			string routeHelp = string.Format("Route <INPUT> <OUTPUT> <{0}>", connectionTypes);
			yield return
				new GenericConsoleCommand<int, int, eConnectionType>("Route", routeHelp, (i, o, t) => instance.Route(i, o, t));

			string clearOutputHelp = string.Format("Clear <OUTPUT> <{0}>", connectionTypes);
			yield return
				new GenericConsoleCommand<int, eConnectionType>("ClearOutput", clearOutputHelp, (o, t) => instance.ClearOutput(o, t));
		}
	}
}
