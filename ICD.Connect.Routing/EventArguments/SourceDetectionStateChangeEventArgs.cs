using ICD.Common.Properties;
using ICD.Common.Utils;
using ICD.Connect.API.EventArguments;
using ICD.Connect.Routing.Connections;
using ICD.Connect.Routing.Proxies;

namespace ICD.Connect.Routing.EventArguments
{
	/// <summary>
	/// Used in input source detection state change events.
	/// </summary>
	public sealed class SourceDetectionStateChangeEventArgs : AbstractGenericApiEventArgs<InputStateChangeData>
	{
		

		/// <summary>
		/// The input address.
		/// </summary>
		[PublicAPI]
		public int Input { get { return Data.Input; } }

		/// <summary>
		/// The input type.
		/// </summary>
		[PublicAPI]
		public eConnectionType Type { get { return Data.Type; } }

		/// <summary>
		/// The video detected state.
		/// </summary>
		[PublicAPI]
		public bool State { get { return Data.State; } }

		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="input"></param>
		/// <param name="type"></param>
		/// <param name="state"></param>
		public SourceDetectionStateChangeEventArgs(int input, eConnectionType type, bool state):base(RouteDestinationControlApi.EVENT_SOURCE_DETECTION_STATE_CHANGE,new InputStateChangeData(input, type, state))
		{
		}

		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="args"></param>
		public SourceDetectionStateChangeEventArgs(SourceDetectionStateChangeEventArgs args)
			: this(args.Input, args.Type, args.State)
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
			builder.AppendProperty("State", State);

			return builder.ToString();
		}
	}
}
