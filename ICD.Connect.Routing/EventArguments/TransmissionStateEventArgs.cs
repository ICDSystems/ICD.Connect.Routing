using System;
using ICD.Connect.Routing.Connections;

namespace ICD.Connect.Routing.EventArguments
{
	public sealed class TransmissionStateEventArgs : EventArgs
	{
		private readonly int m_Output;
		private readonly eConnectionType m_Type;
		private readonly bool m_State;

		/// <summary>
		/// The output address for the transmission.
		/// </summary>
		public int Output { get { return m_Output; } }

		/// <summary>
		/// The connection type.
		/// </summary>
		public eConnectionType Type { get { return m_Type; } }

		/// <summary>
		/// The transmission state.
		/// </summary>
		public bool State { get { return m_State; } }

		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="output"></param>
		/// <param name="type"></param>
		/// <param name="state"></param>
		public TransmissionStateEventArgs(int output, eConnectionType type, bool state)
		{
			m_Output = output;
			m_State = state;
			m_Type = type;
		}
	}
}
