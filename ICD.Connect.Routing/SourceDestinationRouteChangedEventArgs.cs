using System;
using ICD.Connect.Routing.Connections;

namespace ICD.Connect.Routing
{
	public sealed class SourceDestinationRouteChangedEventArgs : EventArgs
	{
		private readonly eConnectionType m_Type;

		public eConnectionType Type { get { return m_Type; } }

		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="type"></param>
		public SourceDestinationRouteChangedEventArgs(eConnectionType type)
		{
			m_Type = type;
		}
	}
}
