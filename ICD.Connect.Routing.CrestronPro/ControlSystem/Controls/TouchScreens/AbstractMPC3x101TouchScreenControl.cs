using System;
using System.Collections.Generic;
using ICD.Common.Logging.Activities;
using ICD.Common.Logging.LoggingContexts;
using ICD.Common.Utils.EventArguments;
using ICD.Common.Utils.Extensions;
using ICD.Common.Utils.Services.Logging;
using ICD.Connect.API.Commands;
using ICD.Connect.API.Nodes;
#if !NETSTANDARD
using Crestron.SimplSharpPro;
using Crestron.SimplSharpPro.DeviceSupport;
using ICD.Connect.Misc.CrestronPro.Devices.Keypads;
using ICD.Connect.Misc.CrestronPro.Extensions;
#endif
using ICD.Connect.Panels.Crestron.Controls.TouchScreens;
using eButtonState = ICD.Connect.Misc.Keypads.eButtonState;

namespace ICD.Connect.Routing.CrestronPro.ControlSystem.Controls.TouchScreens
{
#if !NETSTANDARD
	public abstract class AbstractMPC3x101TouchScreenControl<TTouchPanel> : AbstractMPC3BasicTouchScreenControl<TTouchPanel>, IMPC3x101TouchScreenControl
		where TTouchPanel : MPC3x101Touchscreen
#else
	public abstract class AbstractMPC3x101TouchScreenControl : AbstractMPC3BasicTouchScreenControl, IMPC3x101TouchScreenControl
#endif
	{
		/// <summary>
		/// Raised when the proximity sensor detection state changes.
		/// </summary>
		public event EventHandler<BoolEventArgs> OnProximityDetectedStateChange;

		private bool? m_CachedVolumeUpButtonEnabled;
		private bool? m_CachedVolumeDownButtonEnabled;

		private bool m_ProximityDetected;

		#region Properties

		/// <summary>
		/// Get the Button object corresponding to the VolumeUp button for this device.
		/// </summary>
		public eButtonState VolumeUp
		{
			get
			{
#if !NETSTANDARD
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
#if !NETSTANDARD
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
#if !NETSTANDARD
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
			get { return m_ProximityDetected; }
			private set
			{
				try
				{
					if (value == m_ProximityDetected)
						return;

					m_ProximityDetected = value;

					Logger.LogSetTo(eSeverity.Informational, "ProximityDetected", m_ProximityDetected);

					OnProximityDetectedStateChange.Raise(this, new BoolEventArgs(m_ProximityDetected));
				}
				finally
				{
					Activities.LogActivity(m_ProximityDetected
						                       ? new Activity(Activity.ePriority.Low, "Proximity Detected", "Proximity Detected",
						                                      eSeverity.Informational)
						                       : new Activity(Activity.ePriority.Low, "Proximity Detected", "Proximity Not Detected",
						                                      eSeverity.Informational));
				}
			}
		}

		/// <summary>
		/// When true, indicates the proximity wakeup is enabled on this device.
		/// </summary>
		public bool ProximityWakeupEnabled
		{
			get
			{
#if !NETSTANDARD
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
#if !NETSTANDARD
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
#if !NETSTANDARD
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
#if !NETSTANDARD
				return TouchScreen.ProximityRangeFeedBack.GetUShortValueOrDefault();
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
#if !NETSTANDARD
				return TouchScreen.ProximityThresholdFeedBack.GetUShortValueOrDefault();
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
			// Initialize activities
			ProximityDetected = false;
		}

		/// <summary>
		/// Override to release resources.
		/// </summary>
		/// <param name="disposing"></param>
		protected override void DisposeFinal(bool disposing)
		{
			OnProximityDetectedStateChange = null;

			base.DisposeFinal(disposing);
		}

		#region Methods

		/// <summary>
		/// Enable the volume down button on this device.
		/// </summary>
		public void SetVolumeDownButtonEnabled(bool enable)
		{
#if !NETSTANDARD
			if (enable == m_CachedVolumeDownButtonEnabled)
				return;

			m_CachedVolumeDownButtonEnabled = enable;

			if (enable)
				TouchScreen.EnableVolumeDownButton();
			else
				TouchScreen.DisableVolumeDownButton();
#else
			throw new NotSupportedException();
#endif
		}

		/// <summary>
		/// Enable the volume up button on this device.
		/// </summary>
		public void SetVolumeUpButtonEnabled(bool enable)
		{
#if !NETSTANDARD
			if (enable == m_CachedVolumeUpButtonEnabled)
				return;

			m_CachedVolumeUpButtonEnabled = enable;

			if (enable)
				TouchScreen.EnableVolumeUpButton();
			else
				TouchScreen.DisableVolumeUpButton();
#else
			throw new NotSupportedException();
#endif
		}

		/// <summary>
		/// Method to enable beeping for front panel button presses.
		/// </summary>
		public void SetButtonPressBeepingEnabled(bool enable)
		{
#if !NETSTANDARD
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
#if !NETSTANDARD
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
#if !NETSTANDARD
			TouchScreen.ProximityThreshold.UShortValue = centimeters;
#else
			throw new NotSupportedException();
#endif
		}

		#endregion

#if !NETSTANDARD
		/// <summary>
		/// Called when a touchscreen panel state changes.
		/// </summary>
		/// <param name="device"></param>
		/// <param name="args"></param>
		protected override void TouchScreenOnPanelStateChange(GenericBase device, BaseEventArgs args)
		{
			base.TouchScreenOnPanelStateChange(device, args);

			switch (args.EventId)
			{
				case FrontPanelEventIds.ProximityDetectedEventId:
					ProximityDetected = TouchScreen.ProximityDetectedFeedBack.BoolValue;
					break;
			}
		}
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

			foreach (IConsoleNodeBase node in MPC3x101TouchScreenControlConsole.GetConsoleNodes(this))
				yield return node;
		}

		/// <summary>
		/// Calls the delegate for each console status item.
		/// </summary>
		/// <param name="addRow"></param>
		public override void BuildConsoleStatus(AddStatusRowDelegate addRow)
		{
			base.BuildConsoleStatus(addRow);

			MPC3x101TouchScreenControlConsole.BuildConsoleStatus(this, addRow);
		}

		/// <summary>
		/// Gets the child console commands.
		/// </summary>
		/// <returns></returns>
		public override IEnumerable<IConsoleCommand> GetConsoleCommands()
		{
			foreach (IConsoleCommand command in GetBaseConsoleCommands())
				yield return command;


			foreach (IConsoleCommand command in MPC3x101TouchScreenControlConsole.GetConsoleCommands(this))
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
