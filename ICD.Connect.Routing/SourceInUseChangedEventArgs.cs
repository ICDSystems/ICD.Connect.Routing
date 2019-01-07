using System;
using System.Collections.Generic;
using ICD.Common.Utils.Collections;
using ICD.Connect.Routing.Connections;
using ICD.Connect.Routing.Endpoints;

namespace ICD.Connect.Routing
{
	public sealed class SourceInUseChangedEventArgs : EventArgs
	{
		private readonly IEnumerable<KeyValuePair<eConnectionType, IcdHashSet<EndpointInfo>>> m_AddedSources;

		private readonly IEnumerable<KeyValuePair<eConnectionType, IcdHashSet<EndpointInfo>>> m_RemovedSources;
		
		public IEnumerable<KeyValuePair<eConnectionType, IcdHashSet<EndpointInfo>>> AddedSources
		{ get {return m_AddedSources;}}

		public IEnumerable<KeyValuePair<eConnectionType, IcdHashSet<EndpointInfo>>> RemovedSources
		{ get { return m_RemovedSources; } }

		public SourceInUseChangedEventArgs(IEnumerable<KeyValuePair<eConnectionType, IcdHashSet<EndpointInfo>>> addedSources, IEnumerable<KeyValuePair<eConnectionType, IcdHashSet<EndpointInfo>>> removedSources)
		{
			m_AddedSources = addedSources;
			m_RemovedSources = removedSources;
		}
	}
}
