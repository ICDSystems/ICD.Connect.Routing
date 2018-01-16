using System;
using ICD.Common.Properties;
using ICD.Common.Utils;
using ICD.Connect.Routing.Connections;

namespace ICD.Connect.Routing.EventArguments
{
	/// <summary>
	/// Used in input source detection state change events.
	/// </summary>
	public sealed class SourceDetectionStateChangeEventArgs : EventArgs
	{
		private readonly int m_Input;
		private readonly eConnectionType m_Type;
		private readonly bool m_State;

		/// <summary>
		/// The input address.
		/// </summary>
		[PublicAPI]
		public int Input { get { return m_Input; } }

		/// <summary>
		/// The input type.
		/// </summary>
		[PublicAPI]
		public eConnectionType Type { get { return m_Type; } }

		/// <summary>
		/// The video detected state.
		/// </summary>
		[PublicAPI]
		public bool State { get { return m_State; } }

		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="input"></param>
		/// <param name="type"></param>
		/// <param name="state"></param>
		public SourceDetectionStateChangeEventArgs(int input, eConnectionType type, bool state)
		{
			m_Input = input;
			m_Type = type;
			m_State = state;
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

			builder.AppendProperty("Input", m_Input);
			builder.AppendProperty("Type", m_Type);
			builder.AppendProperty("State", m_State);

			return builder.ToString();
		}
	}
}
