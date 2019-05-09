namespace ICD.Connect.Routing.SPlus.SPlusDestinationDevice.Proxy
{
	public static class SPlusDestinationApi
	{
		#region Events

		public const string EVENT_SET_POWER_STATE = "OnSetPowerState";
		public const string EVENT_SET_ACTIVE_INPUT = "OnSetActiveInput";
		public const string EVENT_SET_VOLUME_LEVEL = "OnSetVolumeLevel";
		public const string EVENT_SET_VOLUME_MUTE_STATE = "OnSetMuteState";
		public const string EVENT_VOLUME_MUTE_TOGGLE = "OnMuteToggle";
		public const string EVENT_RESEND_ACTIVE_INPUT = "OnResendActiveInput";

		public const string EVENT_SET_POWER_STATE_HELP = "Called to send power control actions to the destination through the shim";
		public const string EVENT_SET_ACTIVE_INPUT_HELP = "Called to set the active input on the destination through the shim";
		public const string EVENT_SET_VOLUME_LEVEL_HELP = "Called to set the volume level on the destination through the shim";
		public const string EVENT_SET_VOLUME_MUTE_STATE_HELP = "Called to set the mute state on the destination through the shim";
		public const string EVENT_VOLUME_MUTE_TOGGLE_HELP = "Called to toggle the mute state on the destination through the shim";

		public const string EVENT_RESEND_ACTIVE_INPUT_HELP =
			"Called to request the shim to resend the active input to the originator";

		#endregion

		#region Properties

		public const string PROPERTY_INPUT_COUNT = "InputCount";

		public const string PROPERTY_INPUT_COUNT_HELP = "The number of inputs configured on the device";

		#endregion

		#region Methods

		public const string METHOD_SET_POWER_STATE_FEEDBACK = "SetPowerStateFeedback";
		public const string METHOD_SET_ACTIVE_INPUT_FEEDBACK = "SetActiveInputFeedback";
		public const string METHOD_SET_INPUT_DETECTED_FEEDBACK = "SetInputDetectedFeedback";
		public const string METHOD_RESET_INPUT_DETECTED_FEEDBACK = "ResetInputDetectedFeedback";
		public const string METHOD_SET_VOLUME_LEVEL_FEEDBACK = "SetVolumeLevelFeedback";
		public const string METHOD_SET_VOLUME_MUTE_STATE_FEEDBACK = "SetVolumeMuteStateFeedback";


		public const string METHOD_SET_POWER_STATE_FEEDBACK_HELP = "Feedback from the shim of the current power state";
		public const string METHOD_SET_ACTIVE_INPUT_FEEDBACK_HELP = "Feedback from the shim of the current active input";
		public const string METHOD_SET_INPUT_DETECTED_FEEDBACK_HELP = "Feedback from the shim of the detection state of inputs";
		public const string METHOD_RESET_INPUT_DETECTED_FEEDBACK_HELP =
			"Resets all the detected input states and replaces with the provided list";
		public const string METHOD_SET_VOLUME_LEVEL_FEEDBACK_HELP = "Feedback from the shim of the current volume level";
		public const string METHOD_SET_VOLUME_MUTE_STATE_FEEDBACK_HELP = "Feedback fromt he shim of the current mute state";

		#endregion

	}
}