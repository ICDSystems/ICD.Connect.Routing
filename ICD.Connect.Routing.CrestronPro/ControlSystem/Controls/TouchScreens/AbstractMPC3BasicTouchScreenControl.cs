using System.Collections.Generic;
using ICD.Connect.API.Commands;
using ICD.Connect.API.Nodes;
#if !NETSTANDARD
using Crestron.SimplSharpPro;
using Crestron.SimplSharpPro.DeviceSupport;
using ICD.Common.Utils;
using ICD.Common.Utils.Extensions;
using ICD.Connect.Misc.CrestronPro.Devices.Keypads;
using ICD.Connect.Misc.CrestronPro.Extensions;
#endif
using System;
using ICD.Connect.Misc.Keypads;
using ICD.Connect.Panels.Crestron.Controls.TouchScreens;
using eButtonState = ICD.Connect.Misc.Keypads.eButtonState;

namespace ICD.Connect.Routing.CrestronPro.ControlSystem.Controls.TouchScreens
{
#if !NETSTANDARD
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
#if !NETSTANDARD
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
#if !NETSTANDARD
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
#if !NETSTANDARD
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
#if !NETSTANDARD
				return TouchScreen.ActiveBrightnessFeedBack.GetUShortValueOrDefault();
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
#if !NETSTANDARD
				return TouchScreen.StandbyBrightnessFeedBack.GetUShortValueOrDefault();
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
#if !NETSTANDARD
				return TouchScreen.ActiveTimeoutFeedBack.GetUShortValueOrDefault();
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
#if !NETSTANDARD
				return TouchScreen.StandbyTimeoutFeedBack.GetUShortValueOrDefault();
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
#if !NETSTANDARD
				return TouchScreen.LEDBrightnessFeedBack.GetUShortValueOrDefault();
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
#if !NETSTANDARD
				return TouchScreen.AmbientLightThresholdForAutoBrightnessAdjustmentFeedBack.GetUShortValueOrDefault();
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
#if !NETSTANDARD
				return TouchScreen.ActiveModeAutoBrightnessLowLevelFeedBack.GetUShortValueOrDefault();
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
#if !NETSTANDARD
				return TouchScreen.ActiveModeAutoBrightnessHighLevelFeedBack.GetUShortValueOrDefault();
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
#if !NETSTANDARD
				return TouchScreen.StandbyModeAutoBrightnessLowLevelFeedBack.GetUShortValueOrDefault();
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
#if !NETSTANDARD
				return TouchScreen.StandbyModeAutoBrightnessHighLevelFeedBack.GetUShortValueOrDefault();
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
#if !NETSTANDARD
				return TouchScreen.AmbientLightLevelFeedBack.GetUShortValueOrDefault();
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
#if !NETSTANDARD
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
#if !NETSTANDARD
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
#if !NETSTANDARD
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
#if !NETSTANDARD
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
#if !NETSTANDARD
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
#if !NETSTANDARD
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
#if !NETSTANDARD
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
#if !NETSTANDARD
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
#if !NETSTANDARD
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

#if !NETSTANDARD
			Unsubscribe(TouchScreen);
#endif
		}

#region TouchScreen Methods

		/// <summary>
		/// Enable the mute button on this device.
		/// </summary>
		public void SetMuteButtonEnabled(bool enable)
		{
#if !NETSTANDARD
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
#if !NETSTANDARD
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
#if !NETSTANDARD
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
#if !NETSTANDARD
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
#if !NETSTANDARD
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
#if !NETSTANDARD
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
#if !NETSTANDARD
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
#if !NETSTANDARD
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
#if !NETSTANDARD
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
#if !NETSTANDARD
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
#if !NETSTANDARD
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
#if !NETSTANDARD
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
#if !NETSTANDARD
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
#if !NETSTANDARD
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
#if !NETSTANDARD
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
#if !NETSTANDARD
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
#if !NETSTANDARD
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
#if !NETSTANDARD
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
#if !NETSTANDARD
			// Hack - Bargraph only starts showing values at 15%, should at least show something for 1%.
			if (percent > 0)
				percent = (ushort)MathUtils.MapRange(0, ushort.MaxValue, 0.2f * ushort.MaxValue, ushort.MaxValue, percent);

			TouchScreen.VolumeBargraph.UShortValue = percent;
#else
			throw new NotSupportedException();
#endif
		}

#endregion

#if !NETSTANDARD
#region TouchScreen Callbacks

		/// <summary>
		/// Subscribe to touchscreen events.
		/// </summary>
		/// <param name="touchScreen"></param>
		private void Subscribe(TTouchScreen touchScreen)
		{
			touchScreen.ButtonStateChange += TouchScreenOnButtonStateChange;
			touchScreen.PanelStateChange += TouchScreenOnPanelStateChange;
		}

		/// <summary>
		/// Unsubscribe from touchscreen events.
		/// </summary>
		/// <param name="touchScreen"></param>
		private void Unsubscribe(TTouchScreen touchScreen)
		{
			touchScreen.ButtonStateChange -= TouchScreenOnButtonStateChange;
			touchScreen.PanelStateChange -= TouchScreenOnPanelStateChange;
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

		/// <summary>
		/// Called when a touchscreen panel state changes.
		/// </summary>
		/// <param name="device"></param>
		/// <param name="args"></param>
		protected virtual void TouchScreenOnPanelStateChange(GenericBase device, BaseEventArgs args)
		{
		}

		#endregion
#endif

		#region Console

		/// <summary>
		/// Gets the child console nodes.
		/// </summary>
		/// <returns></returns>
		public override IEnumerable<IConsoleNodeBase> GetConsoleNodes()
		{
			foreach (IConsoleNodeBase node in GetBaseConsoleNodes())
				yield return node;

			foreach (IConsoleNodeBase node in MPC3BasicTouchScreenControlConsole.GetConsoleNodes(this))
				yield return node;
		}

		/// <summary>
		/// Calls the delegate for each console status item.
		/// </summary>
		/// <param name="addRow"></param>
		public override void BuildConsoleStatus(AddStatusRowDelegate addRow)
		{
			base.BuildConsoleStatus(addRow);

			MPC3BasicTouchScreenControlConsole.BuildConsoleStatus(this, addRow);
		}

		/// <summary>
		/// Gets the child console commands.
		/// </summary>
		/// <returns></returns>
		public override IEnumerable<IConsoleCommand> GetConsoleCommands()
		{
			foreach (IConsoleCommand command in GetBaseConsoleCommands())
				yield return command;


			foreach (IConsoleCommand command in MPC3BasicTouchScreenControlConsole.GetConsoleCommands(this))
				yield return command;
		}

		/// <summary>
		/// Workaround for the "unverifiable code" warning.
		/// </summary>
		/// <returns></returns>
		private IEnumerable<IConsoleCommand> GetBaseConsoleCommands()
		{
			return base.GetConsoleCommands();
		}

		/// <summary>
		/// Workaround for "unverifiable code" warning.
		/// </summary>
		/// <returns></returns>
		private IEnumerable<IConsoleNodeBase> GetBaseConsoleNodes()
		{
			return base.GetConsoleNodes();
		}

		#endregion
	}
}
