using System;
using ICD.Connect.Routing.Connections;

namespace ICD.Connect.Routing
{
	public sealed class EndpointRouteChangedEventArgs : EventArgs
	{
		private readonly eConnectionType m_Type;

		public eConnectionType Type { get { return m_Type; } }

		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="type"></param>
		public EndpointRouteChangedEventArgs(eConnectionType type)
		{
			m_Type = type;
		}
	}
}
