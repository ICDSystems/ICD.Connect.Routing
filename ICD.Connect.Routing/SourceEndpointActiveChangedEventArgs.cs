using System;
using System.Collections.Generic;
using ICD.Common.Utils.Collections;
using ICD.Connect.Routing.Connections;
using ICD.Connect.Routing.Endpoints;

namespace ICD.Connect.Routing
{
	public sealed class SourceEndpointActiveChangedEventArgs : EventArgs
	{
		private readonly IEnumerable<KeyValuePair<eConnectionType, IcdHashSet<EndpointInfo>>> m_ActiveEndpoints;

		private readonly IEnumerable<KeyValuePair<eConnectionType, IcdHashSet<EndpointInfo>>> m_InactiveEndpoints;
		
		public IEnumerable<KeyValuePair<eConnectionType, IcdHashSet<EndpointInfo>>> ActiveEndpoints
		{ get {return m_ActiveEndpoints;}}

		public IEnumerable<KeyValuePair<eConnectionType, IcdHashSet<EndpointInfo>>> InactiveEndpoints
		{ get { return m_InactiveEndpoints; } }

		public SourceEndpointActiveChangedEventArgs(IEnumerable<KeyValuePair<eConnectionType, IcdHashSet<EndpointInfo>>> activeEndpoints, IEnumerable<KeyValuePair<eConnectionType, IcdHashSet<EndpointInfo>>> inactiveEndpoints)
		{
			m_ActiveEndpoints = activeEndpoints;
			m_InactiveEndpoints = inactiveEndpoints;
		}
	}
}
