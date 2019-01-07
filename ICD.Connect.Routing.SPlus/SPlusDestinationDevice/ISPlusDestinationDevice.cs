using System;
using System.Collections.Generic;
using ICD.Connect.API.Attributes;
using ICD.Connect.Devices;
using ICD.Connect.Devices.Simpl;
using ICD.Connect.Routing.SPlus.SPlusDestinationDevice.EventArgs;
using ICD.Connect.Routing.SPlus.SPlusDestinationDevice.Proxy;

namespace ICD.Connect.Routing.SPlus.SPlusDestinationDevice
{
	[ApiClass(typeof(ProxySPlusDestinationDevice), typeof(IDevice))]
	public interface ISPlusDestinationDevice : ISimplDevice
	{

		#region Properties

		[ApiProperty(SPlusDestinationApi.PROPERTY_INPUT_COUNT, SPlusDestinationApi.PROPERTY_INPUT_COUNT_HELP)]
		int? InputCount { get; }

		#endregion

		#region Events To Shim

		[ApiEvent(SPlusDestinationApi.EVENT_SET_POWER_STATE, SPlusDestinationApi.EVENT_SET_POWER_STATE_HELP)]
		event EventHandler<PowerControlApiEventArgs> OnSetPowerState;

		[ApiEvent(SPlusDestinationApi.EVENT_SET_ACTIVE_INPUT, SPlusDestinationApi.EVENT_SET_ACTIVE_INPUT_HELP)]
		event EventHandler<SetActiveInputApiEventArgs> OnSetActiveInput;

		[ApiEvent(SPlusDestinationApi.EVENT_SET_VOLUME_LEVEL, SPlusDestinationApi.EVENT_SET_VOLUME_LEVEL_HELP)]
		event EventHandler<SetVolumeLevelApiEventArgs> OnSetVolumeLevel;

		[ApiEvent(SPlusDestinationApi.EVENT_SET_VOLUME_MUTE_STATE, SPlusDestinationApi.EVENT_SET_VOLUME_MUTE_STATE_HELP)]
		event EventHandler<SetVolumeMuteStateApiEventArgs> OnSetVolumeMuteState;

		[ApiEvent(SPlusDestinationApi.EVENT_VOLUME_MUTE_TOGGLE, SPlusDestinationApi.EVENT_VOLUME_MUTE_TOGGLE_HELP)]
		event EventHandler<VolumeMuteToggleApiEventArgs> OnVolumeMuteToggle; 

		#endregion


		#region Methods From Shim

		[ApiMethod(SPlusDestinationApi.METHOD_SET_POWER_STATE_FEEDBACK, SPlusDestinationApi.METHOD_SET_POWER_STATE_FEEDBACK_HELP)]
		void SetPowerStateFeedback(bool state);

		[ApiMethod(SPlusDestinationApi.METHOD_SET_ACTIVE_INPUT_FEEDBACK, SPlusDestinationApi.METHOD_SET_ACTIVE_INPUT_FEEDBACK_HELP)]
		void SetActiveInputFeedback(int? input);

		[ApiMethod(SPlusDestinationApi.METHOD_SET_INPUT_DETECTED_FEEDBACK, SPlusDestinationApi.METHOD_SET_INPUT_DETECTED_FEEDBACK_HELP)]
		void SetInputDetectedFeedback(int input, bool state);

		[ApiMethod(SPlusDestinationApi.METHOD_RESET_INPUT_DETECTED_FEEDBACK, SPlusDestinationApi.METHOD_RESET_INPUT_DETECTED_FEEDBACK_HELP)]
		void ResetInputDetectedFeedback(List<int> detectedInputs);

		[ApiMethod(SPlusDestinationApi.METHOD_SET_VOLUME_LEVEL_FEEDBACK, SPlusDestinationApi.METHOD_SET_VOLUME_LEVEL_FEEDBACK_HELP)]
		void SetVolumeLevelFeedback(ushort volume);

		[ApiMethod(SPlusDestinationApi.METHOD_SET_VOLUME_MUTE_STATE_FEEDBACK, SPlusDestinationApi.METHOD_SET_VOLUME_MUTE_STATE_FEEDBACK_HELP)]
		void SetVolumeMuteStateFeedback(bool state);

		#endregion

	}
}