using System;
using ICD.Connect.Routing.Endpoints;

namespace ICD.Connect.Routing.RoutingGraphs
{
	public sealed class SourceDestinationBaseDisabledStateChangedEventArgs : EventArgs
	{
		public ISourceDestinationBase SourceDestination { get; private set; }
		public bool Disabled { get; private set; }

		public SourceDestinationBaseDisabledStateChangedEventArgs(ISourceDestinationBase sourceDestination, bool disbled)
		{
			SourceDestination = sourceDestination;
			Disabled = disbled;
		}

	}
}
