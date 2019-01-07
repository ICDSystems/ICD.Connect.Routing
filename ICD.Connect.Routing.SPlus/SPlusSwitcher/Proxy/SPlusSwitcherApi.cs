namespace ICD.Connect.Routing.SPlus.SPlusSwitcher.Proxy
{
	public static class SPlusSwitcherApi
	{

		#region Events

		public const string EVENT_SET_ROUTE = "OnSetRoute";
		public const string EVENT_CLEAR_ROUTE = "OnClearRoute";

		public const string EVENT_SET_ROUTE_HELP = "Called when the device wants to send a route operation to S+";
		public const string EVENT_CLEAR_ROUTE_HELP = "Called when the device wants to send a clear route operation to S+";

		#endregion

		#region Methods

		public const string METHOD_SET_SIGNAL_DETECTED_STATE_FEEDBACK = "SetSignalDetectedStateFeedback";
		public const string METHOD_SET_INPUT_FOR_OUTPUT_FEEDBACK = "SetInputForOutputFeedback";
		public const string METHOD_CLEAR_CACHE = "ClearCache";

		public const string METHOD_SET_SIGNAL_DETECTED_STATE_FEEDBACK_HELP =
			"Called for S+ to tell the device of signal detection feedback";
		public const string METHOD_SET_INPUT_FOR_OUTPUT_FEEDBACK_HELP =
			"Called for S+ to tell teh device of routing feedback";
		public const string METHOD_CLEAR_CACHE_HELP =
			"Called for S+ to clear the switcher cache";

		#endregion
	}
}