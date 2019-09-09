using System;
using System.Collections.Generic;
using ICD.Common.Utils.Extensions;
using ICD.Connect.Devices.Controls;
using ICD.Connect.Devices.EventArguments;
using ICD.Connect.Devices.Simpl;
using ICD.Connect.Routing.SPlus.SPlusDestinationDevice.Controls;
using ICD.Connect.Routing.SPlus.SPlusDestinationDevice.EventArgs;
using ICD.Connect.Settings;

namespace ICD.Connect.Routing.SPlus.SPlusDestinationDevice.Device
{
	public sealed class SPlusDestinationDevice : AbstractSimplDevice<SPlusDestinationDeviceSettings>, ISPlusDestinationDevice
	{
		#region Consts
		
		private const int ROUTE_CONTROL_ID = 0;
		private const int POWER_CONTROL_ID = 1;
		private const int VOLUME_CONTROL_ID = 2;
		
		#endregion

		#region Fields

		#endregion

		#region Properties

		internal SPlusDestinationRouteControl RouteControl { get; private set; }
		internal SPlusDestinationPowerControl PowerControl { get; private set; }
		internal SPlusDestinationVolumeControl VolumeControl { get; private set; }

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
				VolumeControl.SetVolumeMuteStateFeedback(state);
		}

		#endregion

		#region Methods From Control

		internal void SetVolumeLevel(ushort volume)
		{
			OnSetVolumeLevel.Raise(this, new SetVolumeLevelEventArgs(volume));
		}

		internal void SetVolumeMuteState(bool state)
		{
			OnSetVolumeMuteState.Raise(this, new SetVolumeMuteStateEventArgs(state));
		}

		internal void VolumeMuteToggle()
		{
			OnVolumeMuteToggle.Raise(this, new VolumeMuteToggleEventArgs());
		}

		#endregion

		#endregion

		#region Methods

		/// <summary>
		/// Release resources.
		/// </summary>
		protected override void DisposeFinal(bool disposing)
		{
			base.DisposeFinal(disposing);

			RouteControl = null;
			PowerControl = null;
			VolumeControl = null;
		}

		#endregion

		#region Settings

		/// <summary>
		/// Override to apply settings to the instance.
		/// </summary>
		/// <param name="settings"></param>
		/// <param name="factory"></param>
		protected override void ApplySettingsFinal(SPlusDestinationDeviceSettings settings, IDeviceFactory factory)
		{
			base.ApplySettingsFinal(settings, factory);

			//Note: Order is important - power control must be first
			if (settings.PowerControl)
			{
				PowerControl = new SPlusDestinationPowerControl(this, POWER_CONTROL_ID);
				Controls.Add(PowerControl);
				Subscribe(PowerControl);
			}

			RouteControl = new SPlusDestinationRouteControl(this, ROUTE_CONTROL_ID, settings.InputCount);
			Controls.Add(RouteControl);

			
			if (settings.VolumeControl)
			{
				VolumeControl = new SPlusDestinationVolumeControl(this, VOLUME_CONTROL_ID);
				Controls.Add(VolumeControl);
				if (settings.VolumeMin.HasValue)
					VolumeControl.VolumeRawMin = settings.VolumeMin.Value;
				if (settings.VolumeMax.HasValue)
					VolumeControl.VolumeRawMax = settings.VolumeMax.Value;
			}
		}

		private void Subscribe(SPlusDestinationPowerControl powerControl)
		{
			powerControl.OnPowerStateChanged += PowerControlOnPowerStateChanged;
		}

		private void Unsubscribe(SPlusDestinationPowerControl powerControl)
		{
			powerControl.OnPowerStateChanged -= PowerControlOnPowerStateChanged;
		}

		private void PowerControlOnPowerStateChanged(object sender, PowerDeviceControlPowerStateApiEventArgs powerDeviceControlPowerStateApiEventArgs)
		{
			
		}

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
			{
				settings.VolumeControl = true;
				settings.VolumeMin = (ushort?)VolumeControl.VolumeRawMin;
				settings.VolumeMax = (ushort?)VolumeControl.VolumeRawMax;
			}
		}

		/// <summary>
		/// Override to clear the instance settings.
		/// </summary>
		protected override void ClearSettingsFinal()
		{
			base.ClearSettingsFinal();

			if (RouteControl != null)
			{
				Controls.Remove(ROUTE_CONTROL_ID);
				RouteControl.Dispose();
				RouteControl = null;
			}

			if (PowerControl != null)
			{
				Controls.Remove(POWER_CONTROL_ID);
				PowerControl.Dispose();
				PowerControl = null;
			}
			if (VolumeControl != null)
			{
				Controls.Remove(VOLUME_CONTROL_ID);
				VolumeControl.Dispose();
				VolumeControl = null;
			}
		}

		#endregion
	}
}