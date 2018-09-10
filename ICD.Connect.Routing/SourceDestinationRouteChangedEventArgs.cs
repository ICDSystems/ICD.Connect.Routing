using System;
using ICD.Connect.Routing.Connections;

namespace ICD.Connect.Routing
{
	public sealed class SourceDestinationRouteChangedEventArgs : EventArgs
	{
		public eConnectionType Type { get; private set; }

		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="type"></param>
		public SourceDestinationRouteChangedEventArgs(eConnectionType type)
		{
			Type = type;
		}
	}
}
