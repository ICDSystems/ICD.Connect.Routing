using ICD.Common.Utils;
using ICD.Connect.API.EventArguments;
using ICD.Connect.Routing.Connections;
using ICD.Connect.Routing.Proxies;

namespace ICD.Connect.Routing.EventArguments
{
	public sealed class ActiveInputStateChangeEventArgs : AbstractGenericApiEventArgs<InputStateChangeData>
	{
		/// <summary>
		/// Gets the input that is now active/inactive.
		/// </summary>
		public int Input { get { return Data.Input; } }

		/// <summary>
		/// Gets the connection type for the input.
		/// </summary>
		public eConnectionType Type { get { return Data.Type; } }

		/// <summary>
		/// Gets the active state of the input.
		/// </summary>
		public bool Active { get { return Data.State; } }

		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="input"></param>
		/// <param name="type"></param>
		/// <param name="active"></param>
		public ActiveInputStateChangeEventArgs(int input, eConnectionType type, bool active)
			: base(RouteDestinationControlApi.EVENT_ACTIVE_INPUTS_CHANGED, new InputStateChangeData(input, type, active))
		{
		}

		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="args"></param>
		public ActiveInputStateChangeEventArgs(ActiveInputStateChangeEventArgs args)
			: this(args.Input, args.Type, args.Active)
		{
		}

		/// <summary>
		/// Gets the string representation for this instance.
		/// </summary>
		/// <returns></returns>
		public override string ToString()
		{
			ReprBuilder builder = new ReprBuilder(this);

			builder.AppendProperty("Input", Input);
			builder.AppendProperty("Type", Type);
			builder.AppendProperty("Active", Active);

			return builder.ToString();
		}
	}
}
