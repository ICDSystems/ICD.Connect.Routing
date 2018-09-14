using System.Collections.Generic;
#if SIMPLSHARP
using Crestron.SimplSharpPro;
using Crestron.SimplSharpPro.DeviceSupport;
#endif
using System;
using ICD.Common.Utils.Extensions;
using ICD.Connect.Misc.CrestronPro.Devices.Keypads;
using ICD.Connect.Misc.Keypads;
using ICD.Connect.Panels.Crestron.Controls.TouchScreens;
using eButtonState = ICD.Connect.Misc.Keypads.eButtonState;

namespace ICD.Connect.Routing.CrestronPro.ControlSystem.Controls.TouchScreens
{
#if SIMPLSHARP
	public abstract class AbstractMPC3BasicTouchScreenControl<TTouchScreen> : AbstractControlSystemTouchScreenControl<TTouchScreen>, IMPC3BasicTouchScreenControl
		where TTouchScreen : MPC3Basic
#else
	public abstract class AbstractMPC3BasicTouchScreenControl : AbstractControlSystemTouchScreenControl, IMPC3BasicTouchScreenControl
#endif
	{
		private readonly Dictionary<uint, bool> m_CachedNumericalButtonSelected;
		private readonly Dictionary<uint, bool> m_CachedNumericalButtonEnabled;

		private bool? m_CachedMuteButtonEnabled;
		private bool? m_CachedPowerButtonEnabled;

		private bool? m_CachedMuteButtonSelected;
		private bool? m_CachedPowerButtonSelected;

		/// <summary>
		/// Raised when a button state changes.
		/// </summary>
		public event EventHandler<KeypadButtonPressedEventArgs> OnButtonStateChange;

		#region TouchScreen Properties

		/// <summary>
		/// When true, indicates automatic LED brightness adjustment based ambient light is enabled on this device.
		/// </summary>
		public bool AutoBrightnessEnabled
		{
			get
			{
#if SIMPLSHARP
				return TouchScreen.AutoBrightnessEnabledFeedBack.BoolValue;
#else
				throw new NotSupportedException();
#endif
			}
		}

		/// <summary>
		/// Property that returns true when the mute button is enabled on this device, false otherwise.
		/// </summary>
		public bool MuteButtonEnabled
		{
			get
			{
#if SIMPLSHARP
				return TouchScreen.MuteButtonEnabledFeedBack.BoolValue;
#else
				throw new NotSupportedException();
#endif
			}
		}

		/// <summary>
		/// Property that returns true when the power button is enabled on this device, false otherwise.
		/// </summary>
		public bool PowerButtonEnabled
		{
			get
			{
#if SIMPLSHARP
				return TouchScreen.PowerButtonEnabledFeedBack.BoolValue;
#else
				throw new NotSupportedException();
#endif
			}
		}

		/// <summary>
		/// Indicates the LED brightness level in Active State,
		/// Valid values range from 0 (0%) to 65535 (100%).
		/// </summary>
		public ushort ActiveBrightnessPercent
		{
			get
			{
#if SIMPLSHARP
				return TouchScreen.ActiveBrightnessFeedBack.UShortValue;
#else
				throw new NotSupportedException();
#endif
			}
		}

		/// <summary>
		/// Indicates the LED brightness level in Standby State.
		/// Valid values range from 0 (0%) to 65535 (100%).
		/// </summary>
		public ushort StandbyBrightnessPercent
		{
			get
			{
#if SIMPLSHARP
				return TouchScreen.StandbyBrightnessFeedBack.UShortValue;
#else
				throw new NotSupportedException();
#endif
			}
		}

		/// <summary>
		/// Indicates Active State timeout value in minutes.
		/// </summary>
		public ushort ActiveTimeoutMinutes
		{
			get
			{
#if SIMPLSHARP
				return TouchScreen.ActiveTimeoutFeedBack.UShortValue;
#else
				throw new NotSupportedException();
#endif
			}
		}

		/// <summary>
		/// Indicates Standby State timeout value in minutes.
		/// </summary>
		public ushort StandbyTimeoutMinutes
		{
			get
			{
#if SIMPLSHARP
				return TouchScreen.StandbyTimeoutFeedBack.UShortValue;
#else
				throw new NotSupportedException();
#endif
			}
		}

		/// <summary>
		/// Indicates the button LED brightness level.
		/// Valid values range from 0 (0%) to 65535 (100%).
		/// </summary>
		public ushort LedBrightnessPercent
		{
			get
			{
#if SIMPLSHARP
				return TouchScreen.LEDBrightnessFeedBack.UShortValue;
#else
				throw new NotSupportedException();
#endif
			}
		}

		/// <summary>
		/// Reports the current ambient light threshold level in lux unit.
		/// This property is only valid when the AutoBrightnessEnabled property is set to true.
		/// </summary>
		public ushort AmbientLightThresholdForAutoBrightnessAdjustmentLux
		{
			get
			{
#if SIMPLSHARP
				return TouchScreen.AmbientLightThresholdForAutoBrightnessAdjustmentFeedBack.UShortValue;
#else
				throw new NotSupportedException();
#endif
			}
		}

		/// <summary>
		/// Reports the current active mode auto brightness low level in lux unit.
		/// Valid values range from 0 (0%) to 65535 (100%).
		/// </summary>
		public ushort ActiveModeAutoBrightnessLowLevelPercent
		{
			get
			{
#if SIMPLSHARP
				return TouchScreen.ActiveModeAutoBrightnessLowLevelFeedBack.UShortValue;
#else
				throw new NotSupportedException();
#endif
			}
		}

		/// <summary>
		/// Reports the current active mode auto brightness high level in lux unit.
		/// Valid values range from 0 (0%) to 65535 (100%).
		/// </summary>
		public ushort ActiveModeAutoBrightnessHighLevelPercent
		{
			get
			{
#if SIMPLSHARP
				return TouchScreen.ActiveModeAutoBrightnessHighLevelFeedBack.UShortValue;
#else
				throw new NotSupportedException();
#endif
			}
		}

		/// <summary>
		/// Reports the current standby mode auto brightness low level in lux unit.
		/// Valid values range from 0 (0%) to 65535 (100%).
		/// </summary>
		public ushort StandbyModeAutoBrightnessLowLevelPercent
		{
			get
			{
#if SIMPLSHARP
				return TouchScreen.StandbyModeAutoBrightnessLowLevelFeedBack.UShortValue;
#else
				throw new NotSupportedException();
#endif
			}
		}

		/// <summary>
		/// Reports the current standby mode auto brightness high level in lux unit.
		/// Valid values range from 0 (0%) to 65535 (100%).
		/// </summary>
		public ushort StandbyModeAutoBrightnessHighLevelPercent
		{
			get
			{
#if SIMPLSHARP
				return TouchScreen.StandbyModeAutoBrightnessHighLevelFeedBack.UShortValue;
#else
				throw new NotSupportedException();
#endif
			}
		}

		/// <summary>
		/// Reports the current ambient light level in lux unit for selecting high LED level vs low LED level.
		/// 100-400: normal office, 600: bright lab, 10000+: direct sunlight.
		/// </summary>
		public ushort AmbientLightLevelLux
		{
			get
			{
#if SIMPLSHARP
				return TouchScreen.AmbientLightLevelFeedBack.UShortValue;
#else
				throw new NotSupportedException();
#endif
			}
		}

		/// <summary>
		/// Get the Button object corresponding to the Mute button for this device.
		/// </summary>
		public eButtonState Mute
		{
			get
			{
#if SIMPLSHARP
				return ButtonStateConverter.GetButtonState(TouchScreen.Mute);
#else
				throw new NotSupportedException();
#endif
			}
		}

		/// <summary>
		/// Get the Button object corresponding to the Power button for this device.
		/// </summary>
		public eButtonState Power
		{
			get
			{
#if SIMPLSHARP
				return ButtonStateConverter.GetButtonState(TouchScreen.Power);
#else
				throw new NotSupportedException();
#endif
			}
		}

		/// <summary>
		/// Get the Button object corresponding to the Button1 button for this device.
		/// </summary>
		public eButtonState Button1
		{
			get
			{
#if SIMPLSHARP
				return ButtonStateConverter.GetButtonState(TouchScreen.Button1);
#else
				throw new NotSupportedException();
#endif
			}
		}

		/// <summary>
		/// Get the Button object corresponding to the Button2 button for this device.
		/// </summary>
		public eButtonState Button2
		{
			get
			{
#if SIMPLSHARP
				return ButtonStateConverter.GetButtonState(TouchScreen.Button2);
#else
				throw new NotSupportedException();
#endif
			}
		}

		/// <summary>
		/// Get the Button object corresponding to the Button3 buttonm for this device.
		/// </summary>
		public eButtonState Button3
		{
			get
			{
#if SIMPLSHARP
				return ButtonStateConverter.GetButtonState(TouchScreen.Button3);
#else
				throw new NotSupportedException();
#endif
			}
		}

		/// <summary>
		/// Get the Button object corresponding to the Button4 button for this device.
		/// </summary>
		public eButtonState Button4
		{
			get
			{
#if SIMPLSHARP
				return ButtonStateConverter.GetButtonState(TouchScreen.Button4);
#else
				throw new NotSupportedException();
#endif
			}
		}

		/// <summary>
		/// Get the Button object corresponding to the Button5 button for this device.
		/// </summary>
		public eButtonState Button5
		{
			get
			{
#if SIMPLSHARP
				return ButtonStateConverter.GetButtonState(TouchScreen.Button5);
#else
				throw new NotSupportedException();
#endif
			}
		}

		/// <summary>
		/// Get the Button object corresponding to the Button6 button for this device.
		/// </summary>
		public eButtonState Button6
		{
			get
			{
#if SIMPLSHARP
				return ButtonStateConverter.GetButtonState(TouchScreen.Button6);
#else
				throw new NotSupportedException();
#endif
			}
		}

		#endregion

		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="parent"></param>
		/// <param name="id"></param>
		protected AbstractMPC3BasicTouchScreenControl(ControlSystemDevice parent, int id)
			: base(parent, id)
		{
#if SIMPLSHARP
			m_CachedNumericalButtonSelected = new Dictionary<uint, bool>();
			m_CachedNumericalButtonEnabled = new Dictionary<uint, bool>();

			Subscribe(TouchScreen);
#endif
		}

		/// <summary>
		/// Override to release resources.
		/// </summary>
		/// <param name="disposing"></param>
		protected override void DisposeFinal(bool disposing)
		{
			OnButtonStateChange = null;

			base.DisposeFinal(disposing);

#if SIMPLSHARP
			Unsubscribe(TouchScreen);
#endif
		}

#region TouchScreen Methods

		/// <summary>
		/// Enable the mute button on this device.
		/// </summary>
		public void SetMuteButtonEnabled(bool enable)
		{
#if SIMPLSHARP
			if (enable == m_CachedMuteButtonEnabled)
				return;

			m_CachedMuteButtonEnabled = enable;

			if (enable)
				TouchScreen.EnableMuteButton();
			else
				TouchScreen.DisableMuteButton();
#else
			throw new NotSupportedException();
#endif
		}

		/// <summary>
		/// Select the mute button on this device.
		/// </summary>
		public void SetMuteButtonSelected(bool select)
		{
#if SIMPLSHARP
			if (select == m_CachedMuteButtonSelected)
				return;

			m_CachedMuteButtonSelected = select;

			TouchScreen.FeedbackMute.State = select;
#else
			throw new NotSupportedException();
#endif
		}

		/// <summary>
		/// Enable the power button on this device.
		/// </summary>
		/// <param name="enable"></param>
		public void SetPowerButtonEnabled(bool enable)
		{
#if SIMPLSHARP
			if (enable == m_CachedPowerButtonEnabled)
				return;

			m_CachedPowerButtonEnabled = enable;

			if (enable)
				TouchScreen.EnablePowerButton();
			else
				TouchScreen.DisablePowerButton();
#else
			throw new NotSupportedException();
#endif
		}

		/// <summary>
		/// Select the power button on this device.
		/// </summary>
		/// <param name="select"></param>
		public void SetPowerButtonSelected(bool select)
		{
#if SIMPLSHARP
			if (select == m_CachedPowerButtonSelected)
				return;

			m_CachedPowerButtonSelected = select;

			TouchScreen.FeedbackPower.State = select;
#else
			throw new NotSupportedException();
#endif
		}

		/// <summary>
		/// Enable a given numerical button on this device.
		/// </summary>
		/// <param name="buttonNumber">1-6 on MPC3-201 Touchscreen panel.</param>
		/// <param name="enabled"></param>
		/// <exception cref="T:System.IndexOutOfRangeException">Invalid Button Number specified.</exception>
		public void SetNumericalButtonEnabled(uint buttonNumber, bool enabled)
		{
#if SIMPLSHARP
			bool cached;
			if (m_CachedNumericalButtonEnabled.TryGetValue(buttonNumber, out cached) && enabled == cached)
				return;

			m_CachedNumericalButtonEnabled[buttonNumber] = enabled;

			if (enabled)
				TouchScreen.EnableNumericalButton(buttonNumber);
			else
				TouchScreen.DisableNumericalButton(buttonNumber);
#else
			throw new NotSupportedException();
#endif
		}

		/// <summary>
		/// Select a given numerical button on this device.
		/// </summary>
		/// <param name="buttonNumber">1-6 on MPC3-201 Touchscreen panel.</param>
		/// <param name="selected"></param>
		/// <exception cref="T:System.IndexOutOfRangeException">Invalid Button Number specified.</exception>
		public void SetNumericalButtonSelected(uint buttonNumber, bool selected)
		{
#if SIMPLSHARP
			bool cached;
			if (m_CachedNumericalButtonSelected.TryGetValue(buttonNumber, out cached) && selected == cached)
				return;

			m_CachedNumericalButtonSelected[buttonNumber] = selected;

			switch (buttonNumber)
			{
				case 1:
					TouchScreen.Feedback1.State = selected;
					break;
				case 2:
					TouchScreen.Feedback2.State = selected;
					break;
				case 3:
					TouchScreen.Feedback3.State = selected;
					break;
				case 4:
					TouchScreen.Feedback4.State = selected;
					break;
				case 5:
					TouchScreen.Feedback5.State = selected;
					break;
				case 6:
					TouchScreen.Feedback6.State = selected;
					break;
				default:
					throw new IndexOutOfRangeException();
			}
#else
			throw new NotSupportedException();
#endif
		}

		/// <summary>
		/// Enable automatic LED brightness adjustment based ambient light while the property is true.
		/// Setting the property to false will disable it.
		/// </summary>
		public void SetAutoBrightnessEnabled(bool enable)
		{
#if SIMPLSHARP
			TouchScreen.AutoBrightnessEnabled.BoolValue = enable;
#else
			throw new NotSupportedException();
#endif
		}

		/// <summary>
		/// Property to indicate if a numerical button is enabled for a button number on this device.
		/// </summary>
		public bool GetNumericalButtonEnabled(uint buttonNumber)
		{
#if SIMPLSHARP
			return TouchScreen.NumericalButtonEnabledFeedBack[buttonNumber].BoolValue;
#else
			throw new NotSupportedException();
#endif
		}

		/// <summary>
		/// Specifies LED brightness level in active state.
		/// Valid values range from 0 (0%) to 65535 (100%).
		/// This property is not supported by <see cref="T:Crestron.SimplSharpPro.MPC3x30xTouchscreen"/>.
		/// </summary>
		public void SetActiveBrightness(ushort percent)
		{
#if SIMPLSHARP
			TouchScreen.ActiveBrightness.UShortValue = percent;
#else
			throw new NotSupportedException();
#endif
		}

		/// <summary>
		/// Specifies LED brightness level in standby state.
		/// Valid values range from 0 (0%) to 65535 (100%).
		/// This property is not supported by <see cref="T:Crestron.SimplSharpPro.MPC3x30xTouchscreen"/>.
		/// </summary>
		public void SetStandbyBrightness(ushort percent)
		{
#if SIMPLSHARP
			TouchScreen.StandbyBrightness.UShortValue = percent;
#else
			throw new NotSupportedException();
#endif
		}

		/// <summary>
		/// Specifies the Active State timeout value in minutes.
		/// Value 0: the timeout is disabled.
		/// This property is not supported by <see cref="T:Crestron.SimplSharpPro.MPC3x30xTouchscreen"/>.
		/// </summary>
		public void SetActiveTimeout(ushort minutes)
		{
#if SIMPLSHARP
			TouchScreen.ActiveTimeout.UShortValue = minutes;
#else
			throw new NotSupportedException();
#endif
		}

		/// <summary>
		/// Specifies the Standby State timeout value in minutes.
		/// Value 0: the timeout is disabled.
		/// This property is not supported by <see cref="T:Crestron.SimplSharpPro.MPC3x30xTouchscreen"/>.
		/// </summary>
		public void SetStandbyTimeout(ushort minutes)
		{
#if SIMPLSHARP
			TouchScreen.StandbyTimeout.UShortValue = minutes;
#else
			throw new NotSupportedException();
#endif
		}

		/// <summary>
		/// Specifies the button LED brightness level.
		/// Valid values range from 0 (0%) to 65535 (100%).
		/// </summary>
		public void SetLedBrightness(ushort percent)
		{
#if SIMPLSHARP
			TouchScreen.LEDBrightness.UShortValue = percent;
#else
			throw new NotSupportedException();
#endif
		}

		/// <summary>
		/// Specifies ambient light level in lux unit for selecting high LED level vs low LED level.
		/// This property is only valid when the <see cref="P:Crestron.SimplSharpPro.MPC3Basic.AutoBrightnessEnabled"/> property is set to true.
		/// </summary>
		public void SetAmbientLightThresholdForAutoBrightnessAdjustment(ushort lux)
		{
#if SIMPLSHARP
			TouchScreen.AmbientLightThresholdForAutoBrightnessAdjustment.UShortValue = lux;
#else
			throw new NotSupportedException();
#endif
		}

		/// <summary>
		/// Specifies active mode auto brightness low level in lux unit.
		/// Valid values range from 0 (0%) to 65535 (100%).
		/// This property is not supported by <see cref="T:Crestron.SimplSharpPro.MPC3x30xTouchscreen"/>.
		/// </summary>
		public void SetActiveModeAutoBrightnessLowLevel(ushort percent)
		{
#if SIMPLSHARP
			TouchScreen.ActiveModeAutoBrightnessLowLevel.UShortValue = percent;
#else
			throw new NotSupportedException();
#endif
		}

		/// <summary>
		/// Specifies active mode auto brightness high level in lux unit.
		/// Valid values range from 0 (0%) to 65535 (100%).
		/// This property is not supported by <see cref="T:Crestron.SimplSharpPro.MPC3x30xTouchscreen"/>.
		/// 
		/// </summary>
		public void SetActiveModeAutoBrightnessHighLevel(ushort percent)
		{
#if SIMPLSHARP
			TouchScreen.ActiveModeAutoBrightnessHighLevel.UShortValue = percent;
#else
			throw new NotSupportedException();
#endif
		}

		/// <summary>
		/// Specifies standby mode auto brightness low level in lux unit.
		/// Valid values range from 0 (0%) to 65535 (100%).
		/// This property is not supported by <see cref="T:Crestron.SimplSharpPro.MPC3x30xTouchscreen"/>.
		/// 
		/// </summary>
		public void SetStandbyModeAutoBrightnessLowLevel(ushort percent)
		{
#if SIMPLSHARP
			TouchScreen.StandbyModeAutoBrightnessLowLevel.UShortValue = percent;
#else
			throw new NotSupportedException();
#endif
		}

		/// <summary>
		/// Specifies standby mode auto brightness high level in lux unit.
		/// Valid values range from 0 (0%) to 65535 (100%).
		/// This property is not supported by <see cref="T:Crestron.SimplSharpPro.MPC3x30xTouchscreen"/>.
		/// </summary>
		public void SetStandbyModeAutoBrightnessHighLevel(ushort percent)
		{
#if SIMPLSHARP
			TouchScreen.StandbyModeAutoBrightnessHighLevel.UShortValue = percent;
#else
			throw new NotSupportedException();
#endif
		}

		/// <summary>
		/// Property to set volume bargraph level on this device.
		/// Valid values range from 0 (0%) to 65535 (100%).
		/// This property is not supported by <see cref="T:Crestron.SimplSharpPro.MPC3x30xTouchscreen"/>.
		/// </summary>
		public void SetVolumeBargraph(ushort percent)
		{
#if SIMPLSHARP
			TouchScreen.VolumeBargraph.UShortValue = percent;
#else
			throw new NotSupportedException();
#endif
		}

#endregion

#if SIMPLSHARP
#region TouchScreen Callbacks

		/// <summary>
		/// Subscribe to touchscreen events.
		/// </summary>
		/// <param name="touchScreen"></param>
		private void Subscribe(TTouchScreen touchScreen)
		{
			touchScreen.ButtonStateChange += TouchScreenOnButtonStateChange;
		}

		/// <summary>
		/// Unsubscribe from touchscreen events.
		/// </summary>
		/// <param name="touchScreen"></param>
		private void Unsubscribe(TTouchScreen touchScreen)
		{
			touchScreen.ButtonStateChange -= TouchScreenOnButtonStateChange;
		}

		/// <summary>
		/// Called when a touchscreen button state changes.
		/// </summary>
		/// <param name="device"></param>
		/// <param name="args"></param>
		private void TouchScreenOnButtonStateChange(GenericBase device, ButtonEventArgs args)
		{
			uint button = args.Button.Number;
			eButtonState state = ButtonStateConverter.GetButtonState(args.NewButtonState);

			OnButtonStateChange.Raise(this, new KeypadButtonPressedEventArgs(button, state));
		}

#endregion
#endif
	}
}