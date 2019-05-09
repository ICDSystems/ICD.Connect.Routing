using System;
using System.Collections.Generic;
using ICD.Common.Utils.Extensions;
using ICD.Connect.API;
using ICD.Connect.API.Info;
using ICD.Connect.Devices.Simpl;
using ICD.Connect.Routing.SPlus.SPlusDestinationDevice.EventArgs;

namespace ICD.Connect.Routing.SPlus.SPlusDestinationDevice.Proxy
{
	public sealed class ProxySPlusDestinationDevice : AbstractSimplProxyDevice<ProxySPlusDestinationDeviceSettings>, ISPlusDestinationDevice
	{
		#region Events

		public event EventHandler<PowerControlApiEventArgs> OnSetPowerState;
		public event EventHandler<SetActiveInputApiEventArgs> OnSetActiveInput;
		public event EventHandler<SetVolumeLevelApiEventArgs> OnSetVolumeLevel;
		public event EventHandler<SetVolumeMuteStateApiEventArgs> OnSetVolumeMuteState;
		public event EventHandler<VolumeMuteToggleApiEventArgs> OnVolumeMuteToggle;
		public event EventHandler<ResendActiveInputApiEventArgs> OnResendActiveInput;

		#endregion

		#region Properties
		
		public int? InputCount { get; private set; }

		#endregion

		#region Methods

		public void SetPowerStateFeedback(bool state)
		{
			CallMethod(SPlusDestinationApi.METHOD_SET_POWER_STATE_FEEDBACK, state);
		}

		public void SetActiveInputFeedback(int? input)
		{
			CallMethod(SPlusDestinationApi.METHOD_SET_ACTIVE_INPUT_FEEDBACK, input);
		}

		public void SetInputDetectedFeedback(int input, bool state)
		{
			CallMethod(SPlusDestinationApi.METHOD_SET_INPUT_DETECTED_FEEDBACK, input, state);
		}

		public void ResetInputDetectedFeedback(List<int> detectedInputs)
		{
			CallMethod(SPlusDestinationApi.METHOD_RESET_INPUT_DETECTED_FEEDBACK, detectedInputs);
		}

		public void SetVolumeLevelFeedback(ushort volume)
		{
			CallMethod(SPlusDestinationApi.METHOD_SET_VOLUME_LEVEL_FEEDBACK, volume);
		}

		public void SetVolumeMuteStateFeedback(bool state)
		{
			CallMethod(SPlusDestinationApi.METHOD_SET_VOLUME_MUTE_STATE_FEEDBACK, state);
		}

		#endregion

		#region API

		/// <summary>
		/// Override to build initialization commands on top of the current class info.
		/// </summary>
		/// <param name="command"></param>
		protected override void Initialize(ApiClassInfo command)
		{
			base.Initialize(command);
			
			ApiCommandBuilder.UpdateCommand(command)
							 .SubscribeEvent(SPlusDestinationApi.EVENT_SET_ACTIVE_INPUT)
							 .SubscribeEvent(SPlusDestinationApi.EVENT_SET_POWER_STATE)
							 .SubscribeEvent(SPlusDestinationApi.EVENT_SET_VOLUME_LEVEL)
							 .SubscribeEvent(SPlusDestinationApi.EVENT_SET_VOLUME_MUTE_STATE)
							 .SubscribeEvent(SPlusDestinationApi.EVENT_VOLUME_MUTE_TOGGLE)
							 .SubscribeEvent(SPlusDestinationApi.EVENT_RESEND_ACTIVE_INPUT)
							 .GetProperty(SPlusDestinationApi.PROPERTY_INPUT_COUNT)
							 .Complete();
			RaiseOnRequestShimResync(this);
		}

		/// <summary>
		/// Updates the proxy with event feedback info.
		/// </summary>
		/// <param name="name"></param>
		/// <param name="result"></param>
		protected override void ParseEvent(string name, ApiResult result)
		{
			base.ParseEvent(name, result);

			switch (name)
			{
				case SPlusDestinationApi.EVENT_SET_POWER_STATE:
					RaiseSetPowerState(result.GetValue<bool>());
					break;
				case SPlusDestinationApi.EVENT_SET_ACTIVE_INPUT:
					RaiseSetActiveInput(result.GetValue<int?>());
					break;
				case SPlusDestinationApi.EVENT_SET_VOLUME_LEVEL:
					RaiseSetVolumeLevel(result.GetValue<ushort>());
					break;
				case SPlusDestinationApi.EVENT_SET_VOLUME_MUTE_STATE:
					RaiseSetVolumeMuteState(result.GetValue<bool>());
					break;
				case SPlusDestinationApi.EVENT_VOLUME_MUTE_TOGGLE:
					RaiseVolumeMuteToggle();
					break;
				case SPlusDestinationApi.EVENT_RESEND_ACTIVE_INPUT:
					RaiseResendActiveInput();
					break;
			}
		}

		/// <summary>
		/// Updates the proxy with a property result.
		/// </summary>
		/// <param name="name"></param>
		/// <param name="result"></param>
		protected override void ParseProperty(string name, ApiResult result)
		{
			base.ParseProperty(name, result);

			switch (name)
			{
				case SPlusDestinationApi.PROPERTY_INPUT_COUNT:
					InputCount = result.GetValue<int?>();
					break;
			}
		}

		#endregion

		#region Private Methods

		private void RaiseSetPowerState(bool state)
		{
			OnSetPowerState.Raise(this, new PowerControlApiEventArgs(state));
		}

		private void RaiseSetActiveInput(int? activeInput)
		{
			OnSetActiveInput.Raise(this, new SetActiveInputApiEventArgs(activeInput));
		}

		private void RaiseSetVolumeLevel(ushort volume)
		{
			OnSetVolumeLevel.Raise(this, new SetVolumeLevelApiEventArgs(volume));
		}

		private void RaiseSetVolumeMuteState(bool state)
		{
			OnSetVolumeMuteState.Raise(this, new SetVolumeMuteStateApiEventArgs(state));
		}

		private void RaiseVolumeMuteToggle()
		{
			OnVolumeMuteToggle.Raise(this, new VolumeMuteToggleApiEventArgs());
		}

		private void RaiseResendActiveInput()
		{
			OnResendActiveInput.Raise(this, new ResendActiveInputApiEventArgs());
		}

		#endregion
	}
}