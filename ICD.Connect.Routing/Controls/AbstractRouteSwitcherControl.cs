using System;
using System.Collections.Generic;
using ICD.Common.Utils;
using ICD.Connect.API.Commands;
using ICD.Connect.Devices;
using ICD.Connect.Routing.Connections;
using ICD.Connect.Routing.EventArguments;

namespace ICD.Connect.Routing.Controls
{
	public abstract class AbstractRouteSwitcherControl<T> : AbstractRouteMidpointControl<T>, IRouteSwitcherControl
		where T : IDeviceBase
	{
		public abstract event EventHandler<RouteChangeEventArgs> OnRouteChange;

		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="parent"></param>
		/// <param name="id"></param>
		protected AbstractRouteSwitcherControl(T parent, int id)
			: base(parent, id)
		{
		}

		#region Methods

		public abstract bool Route(RouteOperation info);

		/// <summary>
		/// Stops routing to the given output.
		/// </summary>
		/// <param name="output"></param>
		/// <param name="type"></param>
		/// <returns>True if successfully cleared.</returns>
		public abstract bool ClearOutput(int output, eConnectionType type);

		#endregion

		#region Console

		/// <summary>
		/// Gets the child console commands.
		/// </summary>
		/// <returns></returns>
		public override IEnumerable<IConsoleCommand> GetConsoleCommands()
		{
			foreach (IConsoleCommand command in GetBaseConsoleCommands())
				yield return command;

			string connectionTypes = StringUtils.ArrayFormat(EnumUtils.GetValuesExceptNone<eConnectionType>());

			string routeHelp = string.Format("Route <INPUT> <OUTPUT> <{0}>", connectionTypes);
			yield return
				new GenericConsoleCommand<int, int, eConnectionType>("Route", routeHelp, (i, o, t) => this.Route(i, o, t));

			string clearOutputHelp = string.Format("Clear <OUTPUT> <{0}>", connectionTypes);
			yield return
				new GenericConsoleCommand<int, eConnectionType>("ClearOutput", clearOutputHelp, (o, t) => ClearOutput(o, t));
		}

		/// <summary>
		/// Workaround for "unverifiable code" warning.
		/// </summary>
		/// <returns></returns>
		private IEnumerable<IConsoleCommand> GetBaseConsoleCommands()
		{
			return base.GetConsoleCommands();
		}

		#endregion
	}
}
