using System;
using ICD.Connect.Routing.Connections;

namespace ICD.Connect.Routing.EventArguments
{
	public sealed class ActiveInputStateChangeEventArgs : EventArgs
	{
		private readonly int m_Input;
		private readonly eConnectionType m_Type;
		private readonly bool m_Active;

		/// <summary>
		/// Gets the input that is now active/inactive.
		/// </summary>
		public int Input { get { return m_Input; } }

		/// <summary>
		/// Gets the connection type for the input.
		/// </summary>
		public eConnectionType Type { get { return m_Type; } }

		/// <summary>
		/// Gets the active state of the input.
		/// </summary>
		public bool Active { get { return m_Active; } }

		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="input"></param>
		/// <param name="type"></param>
		/// <param name="active"></param>
		public ActiveInputStateChangeEventArgs(int input, eConnectionType type, bool active)
		{
			m_Input = input;
			m_Type = type;
			m_Active = active;
		}

		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="args"></param>
		public ActiveInputStateChangeEventArgs(ActiveInputStateChangeEventArgs args)
			: this(args.Input, args.Type, args.Active)
		{
		}
	}
}
