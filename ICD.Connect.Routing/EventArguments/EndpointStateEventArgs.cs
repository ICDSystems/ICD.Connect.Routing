using System;
using ICD.Connect.Routing.Endpoints;

namespace ICD.Connect.Routing.EventArguments
{
	public sealed class EndpointStateEventArgs : EventArgs
	{
		private readonly EndpointInfo m_Endpoint;
		private readonly bool m_State;

		public EndpointInfo Endpoint { get { return m_Endpoint; } }

		public bool State { get { return m_State; } }

		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="endpoint"></param>
		/// <param name="state"></param>
		public EndpointStateEventArgs(EndpointInfo endpoint, bool state)
		{
			m_Endpoint = endpoint;
			m_State = state;
		}

		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="args"></param>
		public EndpointStateEventArgs(EndpointStateEventArgs args)
			: this(args.Endpoint, args.State)
		{
		}
	}
}
