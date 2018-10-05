using System;
using System.Collections.Generic;
using System.Linq;
using ICD.Common.Utils.Collections;
using ICD.Connect.Routing.Connections;
using ICD.Connect.Routing.Endpoints;

namespace ICD.Connect.Routing
{
	public sealed class CacheStateChangedEventArgs : EventArgs
	{
		private readonly IcdHashSet<EndpointInfo> m_Endpoints;

		public IEnumerable<EndpointInfo> Endpoints { get { return m_Endpoints; } }

		public eConnectionType Type { get; private set; }

		public bool State { get; private set; }

		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="endpoints"></param>
		/// <param name="type"></param>
		/// <param name="state"></param>
		public CacheStateChangedEventArgs(IEnumerable<EndpointInfo> endpoints, eConnectionType type, bool state)
		{
			endpoints = endpoints ?? Enumerable.Empty<EndpointInfo>();

			m_Endpoints = new IcdHashSet<EndpointInfo>(endpoints);
			Type = type;
			State = state;
		}
	}
}
