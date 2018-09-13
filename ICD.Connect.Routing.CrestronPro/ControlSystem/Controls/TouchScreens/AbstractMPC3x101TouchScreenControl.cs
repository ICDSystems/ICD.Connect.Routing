#if SIMPLSHARP
using Crestron.SimplSharpPro;
#else
using System;
#endif
using ICD.Connect.Misc.CrestronPro.Devices.Keypads;
using ICD.Connect.Misc.Keypads;
using ICD.Connect.Panels.Crestron.Controls.TouchScreens;

namespace ICD.Connect.Routing.CrestronPro.ControlSystem.Controls.TouchScreens
{
#if SIMPLSHARP
	public abstract class AbstractMPC3x101TouchScreenControl<TTouchPanel> : AbstractMPC3BasicTouchScreenControl<TTouchPanel>, IMPC3x101TouchScreenControl
		where TTouchPanel : MPC3x101Touchscreen
#else
	public abstract class AbstractMPC3x101TouchScreenControl : AbstractMPC3BasicTouchScreenControl, IMPC3x101TouchScreenControl
#endif
	{
		#region Properties

		/// <summary>
		/// Get the Button object corresponding to the VolumeUp button for this device.
		/// </summary>
		public eButtonState VolumeUp
		{
			get
			{
#if SIMPLSHARP
				return ButtonStateConverter.GetButtonState(TouchScreen.VolumeUp);
#else
				throw new NotSupportedException();
#endif
			}
		}

		/// <summary>
		/// Get the Button object corresponding to the VolumeDown button for this device.
		/// </summary>
		public eButtonState VolumeDown
		{
			get
			{
#if SIMPLSHARP
				return ButtonStateConverter.GetButtonState(TouchScreen.VolumeDown);
#else
				throw new NotSupportedException();
#endif
			}
		}

		/// <summary>
		/// When true, indicates front panel button press beeping is enabled on this device.
		/// When false, indicates front panel button press beeping is disabled on this device.
		/// </summary>
		public bool ButtonPressBeepingEnabled
		{
			get
			{
#if SIMPLSHARP
				return TouchScreen.ButtonPressBeepingEnabledFeedBack.BoolValue;
#else
				throw new NotSupportedException();
#endif
			}
		}

		/// <summary>
		/// When true, indicates the target is detected within range.
		/// </summary>
		public bool ProximityDetected
		{
			get
			{
#if SIMPLSHARP
				return TouchScreen.ProximityDetectedFeedBack.BoolValue;
#else
				throw new NotSupportedException();
#endif
			}
		}

		/// <summary>
		/// When true, indicates the proximity wakeup is enabled on this device.
		/// </summary>
		public bool ProximityWakeupEnabled
		{
			get
			{
#if SIMPLSHARP
				return TouchScreen.ProximityWakeupEnabledFeedBack.BoolValue;
#else
				throw new NotSupportedException();
#endif
			}
		}

		/// <summary>
		/// Property that returns true when the volume down button is enabled on this device, false otherwise.
		/// </summary>
		public bool VolumeDownButtonEnabled
		{
			get
			{
#if SIMPLSHARP
				return TouchScreen.VolumeDownButtonEnabledFeedBack.BoolValue;
#else
				throw new NotSupportedException();
#endif
			}
		}

		/// <summary>
		/// Property that returns true when the volume up button is enabled on this device, false otherwise.
		/// </summary>
		public bool VolumeUpButtonEnabled
		{
			get
			{
#if SIMPLSHARP
				return TouchScreen.VolumeUpButtonEnabledFeedBack.BoolValue;
#else
				throw new NotSupportedException();
#endif
			}
		}

		/// <summary>
		/// Reports the current proximity range detected in centimeter.
		/// Valid values range from 5 to 200. Invalid value: -1.
		/// </summary>
		public ushort ProximityRange
		{
			get
			{
#if SIMPLSHARP
				return TouchScreen.ProximityRangeFeedBack.UShortValue;
#else
				throw new NotSupportedException();
#endif
			}
		}

		/// <summary>
		/// Indicates Proximity Sensor Detecting Threshold in Centimeter Unit.
		/// Valid values range from 2 (cm) to 200 (cm).
		/// </summary>
		public ushort ProximityThreshold
		{
			get
			{
#if SIMPLSHARP
				return TouchScreen.ProximityThresholdFeedBack.UShortValue;
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
		protected AbstractMPC3x101TouchScreenControl(ControlSystemDevice parent, int id)
			: base(parent, id)
		{
		}

		#region Methods

		/// <summary>
		/// Method to enable beeping for front panel button presses.
		/// </summary>
		public void SetButtonPressBeepingEnabled(bool enable)
		{
#if SIMPLSHARP
			if (enable)
				TouchScreen.EnableButtonPressBeeping();
			else
				TouchScreen.DisableButtonPressBeeping();
#else
			throw new NotSupportedException();
#endif
		}

		/// <summary>
		/// Property to allow proximity detection to wake unit - transition to Active.
		/// Set to true, to enable the proximity wakeup on this device.
		/// </summary>
		public void SetProximityWakeupEnabled(bool enable)
		{
#if SIMPLSHARP
			TouchScreen.EnableProximityWakeup.BoolValue = enable;
#else
			throw new NotSupportedException();
#endif
		}

		/// <summary>
		/// Specifies Proximity Sensor Detecting Threshold in Centimeter Unit.
		/// Valid values range from 2 (cm) to 200 (cm).
		/// </summary>
		public void SetProximityThreshold(ushort centimeters)
		{
#if SIMPLSHARP
			TouchScreen.ProximityThreshold.UShortValue = centimeters;
#else
			throw new NotSupportedException();
#endif
		}

		#endregion
	}
}