using System;
using ICD.Connect.Routing.Endpoints;

namespace ICD.Connect.Routing.EventArguments
{
	public sealed class SourceDestinationBaseDisabledStateChangedEventArgs : EventArgs
	{
		public ISourceDestinationCommon SourceDestination { get; private set; }
		public bool Disabled { get; private set; }

		public SourceDestinationBaseDisabledStateChangedEventArgs(ISourceDestinationCommon sourceDestination, bool disbled)
		{
			SourceDestination = sourceDestination;
			Disabled = disbled;
		}

	}
}
