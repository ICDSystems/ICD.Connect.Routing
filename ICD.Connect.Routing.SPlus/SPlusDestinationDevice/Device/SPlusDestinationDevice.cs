using System;
using System.Collections.Generic;
using ICD.Common.Utils.Extensions;
using ICD.Connect.Devices.Controls;
using ICD.Connect.Devices.CrestronSPlus.Devices.SPlus;
using ICD.Connect.Devices.Controls.Power;
using ICD.Connect.Routing.SPlus.Controls.Volume;
using ICD.Connect.Routing.SPlus.EventArgs;
using ICD.Connect.Routing.SPlus.SPlusDestinationDevice.Controls;
using ICD.Connect.Settings;

namespace ICD.Connect.Routing.SPlus.SPlusDestinationDevice.Device
{
	public sealed class SPlusDestinationDevice : AbstractSPlusDevice<SPlusDestinationDeviceSettings>, ISPlusDestinationDevice, ISPlusVolumeDeviceControlParent
	{
		#region Consts
		
		private const int ROUTE_CONTROL_ID = 0;
		private const int POWER_CONTROL_ID = 1;
		private const int VOLUME_CONTROL_ID = 2;
		
		#endregion

		#region Properties

		private SPlusDestinationRouteControl RouteControl
		{
			get { return Controls.GetControl<SPlusDestinationRouteControl>(); }
		}

		private SPlusDestinationPowerControl PowerControl
		{
			get { return Controls.GetControl<SPlusDestinationPowerControl>(); }
		}

		private SPlusVolumeDeviceControl VolumeControl
		{
			get { return Controls.GetControl<SPlusVolumeDeviceControl>(); }
		}

		public int? InputCount
		{
			get
			{
				if (RouteControl == null)
					return null;
				return RouteControl.InputCount;
			}
		}

		#endregion

		#region Routing Control

		#region Events to Shim

		public event EventHandler<SetActiveInputEventArgs> OnSetActiveInput;

		#endregion

		#region Methods from Shim

		public void SetActiveInputFeedback(int? input)
		{
			if (RouteControl == null)
				return;

			// If we have power contorl, only set active input if powered on
			if (PowerControl == null || PowerControl.PowerState == ePowerState.PowerOn)
				RouteControl.SetActiveInputFeedback(input);
		}

		public void SetInputDetectedFeedback(int input, bool state)
		{
			if (RouteControl != null)
				RouteControl.SetInputDetectedFeedback(input, state);
		}

		public void ResetInputDetectedFeedback(List<int> detectedInputs)
		{
			if (RouteControl != null)
				RouteControl.ResetInputDetectedFeedback(detectedInputs);
		}

		#endregion

		#region Methods From Control

		internal void SetActiveInput(int? input)
		{
			OnSetActiveInput.Raise(this, new SetActiveInputEventArgs(input));
		}

		#endregion

		#endregion

		#region Power Control
		#region Events to Shim

		/// <summary>
		/// Event to send power control actions to the shim
		/// </summary>
		public event EventHandler<PowerControlEventArgs> OnSetPowerState;

		public event EventHandler<ResendActiveInputEventArgs> OnResendActiveInput;

		#endregion

		#region Methods From Shim

		public void SetPowerStateFeedback(ePowerState state)
		{
			if (PowerControl == null)
				return;
			
			PowerControl.SetPowerStateFeedback(state);
			
			// When powering on, request shim to resend active input
			// When powering off, clear active input
			if (state == ePowerState.PowerOn)
				OnResendActiveInput.Raise(this, new ResendActiveInputEventArgs());
			else
			{
				if (RouteControl != null)
					RouteControl.SetActiveInputFeedback(null);
			}
		}

		#endregion

		#region Methods From Control

		internal void PowerOn()
		{
			OnSetPowerState.Raise(this, new PowerControlEventArgs(true));
		}

		internal void PowerOff()
		{
			OnSetPowerState.Raise(this, new PowerControlEventArgs(false));
		}

		#endregion
		#endregion

		#region Volume Control
		#region Events to Shim

		public event EventHandler<SetVolumeLevelEventArgs> OnSetVolumeLevel;

		public event EventHandler<SetVolumeMuteStateEventArgs> OnSetVolumeMuteState;

		public event EventHandler<VolumeMuteToggleEventArgs> OnVolumeMuteToggle;

		#endregion

		#region Methods From Shim

		public void SetVolumeLevelFeedback(ushort volume)
		{
			if (VolumeControl != null)
				VolumeControl.SetVolumeFeedback(volume);
		}

		public void SetVolumeMuteStateFeedback(bool state)
		{
			if (VolumeControl != null)
				VolumeControl.SetVolumeIsMutedFeedback(state);
		}

		#endregion

		#region Methods From Control

	    public void SetVolumeLevel(ushort volume)
		{
			OnSetVolumeLevel.Raise(this, new SetVolumeLevelEventArgs(volume));
		}

	    public void SetVolumeMuteState(bool state)
		{
			OnSetVolumeMuteState.Raise(this, new SetVolumeMuteStateEventArgs(state));
		}

	    public void VolumeMuteToggle()
		{
			OnVolumeMuteToggle.Raise(this, new VolumeMuteToggleEventArgs());
		}

		#endregion

		#endregion

		#region Settings

		/// <summary>
		/// Override to apply properties to the settings instance.
		/// </summary>
		/// <param name="settings"></param>
		protected override void CopySettingsFinal(SPlusDestinationDeviceSettings settings)
		{
			base.CopySettingsFinal(settings);

			if (RouteControl != null)
				settings.InputCount = RouteControl.InputCount;
			if (PowerControl != null)
				settings.PowerControl = true;
			if (VolumeControl != null)
				settings.VolumeControl = true;
		}

		/// <summary>
		/// Override to add controls to the device.
		/// </summary>
		/// <param name="settings"></param>
		/// <param name="factory"></param>
		/// <param name="addControl"></param>
		protected override void AddControls(SPlusDestinationDeviceSettings settings, IDeviceFactory factory, Action<IDeviceControl> addControl)
		{
			base.AddControls(settings, factory, addControl);

			//Note: Order is important - power control must be first
			if (settings.PowerControl)
				addControl(new SPlusDestinationPowerControl(this, POWER_CONTROL_ID));

			addControl(new SPlusDestinationRouteControl(this, ROUTE_CONTROL_ID, settings.InputCount));

			if (settings.VolumeControl)
				addControl(new SPlusVolumeDeviceControl(this, VOLUME_CONTROL_ID));
		}

		#endregion
	}
}