using System;
using System.Collections.Generic;
using ICD.Connect.API.Attributes;
using ICD.Connect.Devices;
using ICD.Connect.Devices.CrestronSPlus.Devices.SPlus;
using ICD.Connect.Devices.Controls.Power;
using ICD.Connect.Routing.SPlus.EventArgs;
using ICD.Connect.Routing.SPlus.SPlusDestinationDevice.Proxy;

namespace ICD.Connect.Routing.SPlus.SPlusDestinationDevice
{
	[ApiClass(typeof(ProxySPlusDestinationDevice), typeof(IDevice))]
	public interface ISPlusDestinationDevice : ISPlusDevice
	{

		#region Properties

		int? InputCount { get; }

		#endregion

		#region Events To Shim

		event EventHandler<PowerControlEventArgs> OnSetPowerState;

		event EventHandler<SetActiveInputEventArgs> OnSetActiveInput;

		event EventHandler<SetVolumeLevelEventArgs> OnSetVolumeLevel;

		event EventHandler<SetVolumeMuteStateEventArgs> OnSetVolumeMuteState;

		event EventHandler<VolumeMuteToggleEventArgs> OnVolumeMuteToggle;

		event EventHandler<ResendActiveInputEventArgs> OnResendActiveInput;

		#endregion


		#region Methods From Shim

		void SetPowerStateFeedback(ePowerState state);

		void SetActiveInputFeedback(int? input);

		void SetInputDetectedFeedback(int input, bool state);

		void ResetInputDetectedFeedback(List<int> detectedInputs);

		void SetVolumeLevelFeedback(ushort volume);

		void SetVolumeMuteStateFeedback(bool state);

		#endregion

	}
}