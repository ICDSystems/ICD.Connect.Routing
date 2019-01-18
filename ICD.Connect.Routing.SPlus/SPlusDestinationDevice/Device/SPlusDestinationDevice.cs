using System;
using System.Collections.Generic;
using ICD.Common.Utils.Extensions;
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

		private SPlusDestinationRouteControl m_RouteControl;
		private SPlusDestinationPowerControl m_PowerControl;
		private SPlusDestinationVolumeControl m_VolumeControl;

		#endregion

		#region Properties

		public int? InputCount
		{
			get
			{
				if (m_RouteControl == null)
					return null;
				return m_RouteControl.InputCount;
			}
		}

		#endregion

		#region Routing Control

		#region Events to Shim

		public event EventHandler<SetActiveInputApiEventArgs> OnSetActiveInput;

		#endregion

		#region Methods from Shim

		public void SetActiveInputFeedback(int? input)
		{
			if (m_RouteControl != null)
				m_RouteControl.SetActiveInputFeedback(input);
		}

		public void SetInputDetectedFeedback(int input, bool state)
		{
			if (m_RouteControl != null)
				m_RouteControl.SetInputDetectedFeedback(input, state);
		}

		public void ResetInputDetectedFeedback(List<int> detectedInputs)
		{
			if (m_RouteControl != null)
				m_RouteControl.ResetInputDetectedFeedback(detectedInputs);
		}

		#endregion

		#region Methods From Control

		internal void SetActiveInput(int? input)
		{
			OnSetActiveInput.Raise(this, new SetActiveInputApiEventArgs(input));
		}

		#endregion

		#endregion

		#region Power Control
		#region Events to Shim

		/// <summary>
		/// Event to send power control actions to the shim
		/// </summary>
		public event EventHandler<PowerControlApiEventArgs> OnSetPowerState;

		#endregion

		#region Methods From Shim

		public void SetPowerStateFeedback(bool state)
		{
			if (m_PowerControl != null)
				m_PowerControl.SetPowerStateFeedback(state);
		}

		#endregion

		#region Methods From Control

		internal void PowerOn()
		{
			OnSetPowerState.Raise(this, new PowerControlApiEventArgs(true));
		}

		internal void PowerOff()
		{
			OnSetPowerState.Raise(this, new PowerControlApiEventArgs(false));
		}

		#endregion
		#endregion

		#region Volume Control
		#region Events to Shim

		public event EventHandler<SetVolumeLevelApiEventArgs> OnSetVolumeLevel;

		public event EventHandler<SetVolumeMuteStateApiEventArgs> OnSetVolumeMuteState;

		public event EventHandler<VolumeMuteToggleApiEventArgs> OnVolumeMuteToggle;

		#endregion

		#region Methods From Shim

		public void SetVolumeLevelFeedback(ushort volume)
		{
			if (m_VolumeControl != null)
				m_VolumeControl.SetVolumeFeedback(volume);
		}

		public void SetVolumeMuteStateFeedback(bool state)
		{
			if (m_VolumeControl != null)
				m_VolumeControl.SetVolumeMuteStateFeedback(state);
		}

		#endregion

		#region Methods From Control

		internal void SetVolumeLevel(ushort volume)
		{
			OnSetVolumeLevel.Raise(this, new SetVolumeLevelApiEventArgs(volume));
		}

		internal void SetVolumeMuteState(bool state)
		{
			OnSetVolumeMuteState.Raise(this, new SetVolumeMuteStateApiEventArgs(state));
		}

		internal void VolumeMuteToggle()
		{
			OnVolumeMuteToggle.Raise(this, new VolumeMuteToggleApiEventArgs());
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

			m_RouteControl = null;
			m_PowerControl = null;
			m_VolumeControl = null;
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

			m_RouteControl = new SPlusDestinationRouteControl(this, ROUTE_CONTROL_ID, settings.InputCount);
			Controls.Add(m_RouteControl);

			if (settings.PowerControl)
			{
				m_PowerControl = new SPlusDestinationPowerControl(this, POWER_CONTROL_ID);
				Controls.Add(m_PowerControl);
			}
			if (settings.VolumeControl)
			{
				m_VolumeControl = new SPlusDestinationVolumeControl(this, VOLUME_CONTROL_ID);
				Controls.Add(m_VolumeControl);
				if (settings.VolumeMin.HasValue)
					m_VolumeControl.VolumeRawMin = settings.VolumeMin.Value;
				if (settings.VolumeMax.HasValue)
					m_VolumeControl.VolumeRawMax = settings.VolumeMax.Value;
			}
		}

		/// <summary>
		/// Override to apply properties to the settings instance.
		/// </summary>
		/// <param name="settings"></param>
		protected override void CopySettingsFinal(SPlusDestinationDeviceSettings settings)
		{
			base.CopySettingsFinal(settings);

			if (m_RouteControl != null)
				settings.InputCount = m_RouteControl.InputCount;
			
			if (m_PowerControl != null)
				settings.PowerControl = true;
			if (m_VolumeControl != null)
			{
				settings.VolumeControl = true;
				settings.VolumeMin = (ushort?)m_VolumeControl.VolumeRawMin;
				settings.VolumeMax = (ushort?)m_VolumeControl.VolumeRawMax;
			}
		}

		/// <summary>
		/// Override to clear the instance settings.
		/// </summary>
		protected override void ClearSettingsFinal()
		{
			base.ClearSettingsFinal();

			if (m_RouteControl != null)
			{
				Controls.Remove(ROUTE_CONTROL_ID);
				m_RouteControl.Dispose();
				m_RouteControl = null;
			}

			if (m_PowerControl != null)
			{
				Controls.Remove(POWER_CONTROL_ID);
				m_PowerControl.Dispose();
				m_PowerControl = null;
			}
			if (m_VolumeControl != null)
			{
				Controls.Remove(VOLUME_CONTROL_ID);
				m_VolumeControl.Dispose();
				m_VolumeControl = null;
			}
		}

		#endregion
	}
}