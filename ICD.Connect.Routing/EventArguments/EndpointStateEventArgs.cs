using System;
using ICD.Connect.Routing.Connections;
using ICD.Connect.Routing.Endpoints;

namespace ICD.Connect.Routing.EventArguments
{
	public sealed class EndpointStateEventArgs : EventArgs
	{
		private readonly EndpointInfo m_Endpoint;
		private readonly eConnectionType m_Type;
		private readonly bool m_State;

		public EndpointInfo Endpoint { get { return m_Endpoint; } }

		public eConnectionType Type { get { return m_Type; } }

		public bool State { get { return m_State; } }

		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="endpoint"></param>
		/// <param name="type"></param>
		/// <param name="state"></param>
		public EndpointStateEventArgs(EndpointInfo endpoint, eConnectionType type, bool state)
		{
			m_Endpoint = endpoint;
			m_Type = type;
			m_State = state;
		}

		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="args"></param>
		public EndpointStateEventArgs(EndpointStateEventArgs args)
			: this(args.Endpoint, args.Type, args.State)
		{
		}
	}
}
