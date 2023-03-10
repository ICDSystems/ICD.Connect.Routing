using System;
using System.Collections.Generic;
using System.Text;
using ICD.Common.Utils;
using ICD.Common.Utils.IO;
using ICD.Common.Utils.Services.Logging;
using ICD.Connect.API.Commands;
using ICD.Connect.API.Nodes;
using ICD.Connect.Devices.Controls;
using ICD.Connect.Misc.ControlSystems;
using ICD.Connect.Misc.CrestronPro.Devices;
using ICD.Connect.Routing.CrestronPro.ControlSystem.Controls;
using ICD.Connect.Routing.CrestronPro.ControlSystem.Controls.TouchScreens;
using ICD.Connect.Settings;
#if !NETSTANDARD
using Crestron.SimplSharpPro;
using Crestron.SimplSharpPro.DM;
using ICD.Connect.Misc.CrestronPro;
using ICD.Common.Properties;
using ICD.Connect.Misc.CrestronPro.Extensions;
using ICD.Connect.Panels.Crestron.Controls.TouchScreens;
#endif

namespace ICD.Connect.Routing.CrestronPro.ControlSystem
{
	/// <summary>
	/// Specifies the mixer setting to use for an output
	/// </summary>
	public enum eOutputMixerMode
	{
		Auto, //Automatically pick an unused mixer
		None,
		Mixer1,
		Mixer2,
		AudioFollowsVideo
	}

	/// <summary>
	/// Wraps a CrestronControlSystem to provide a device that can be used as a port provider and a switcher.
	/// </summary>
	public sealed class ControlSystemDevice : AbstractControlSystemDevice<ControlSystemDeviceSettings>, IPortParent, IDmParent
	{
		private readonly List<IDeviceControl> m_LoadedControls;
		private string m_ConfigPath;

		#region Properties

#if !NETSTANDARD
		/// <summary>
		/// Gets the wrapped Crestron control system.
		/// </summary>
		public CrestronControlSystem ControlSystem { get; private set; }
#endif

		public eOutputMixerMode Output1MixerMode { get; private set; }
		public eOutputMixerMode Output2MixerMode { get; private set; }
		public eOutputMixerMode Output3MixerMode { get; private set; }
		public eOutputMixerMode Output4MixerMode { get; private set; }

		#endregion

		/// <summary>
		/// Constructor.
		/// </summary>
		public ControlSystemDevice()
		{
#if !NETSTANDARD
			SetControlSystem(ProgramInfo.ControlSystem);
#endif
			m_LoadedControls = new List<IDeviceControl>();
		}

#if !NETSTANDARD
		/// <summary>
		/// Creates the touchscreen control instance based on the type of control system.
		/// </summary>
		/// <param name="parent"></param>
		/// <param name="id"></param>
		/// <returns></returns>
		[CanBeNull]
		private static IThreeSeriesTouchScreenControl InstantiateTouchScreen(ControlSystemDevice parent, int id)
		{
			if (parent == null)
				throw new ArgumentNullException("parent");

			switch (parent.ControlSystem.TouchscreenType)
			{
				case eTouchscreenType.TPCS:
					return new TPCSTouchScreenControl(parent, id);
				case eTouchscreenType.Fliptop:
					return new FTTouchScreenControl(parent, id);
				case eTouchscreenType.TSCW730:
					return new TSCW730TouchScreenControl(parent, id);
				case eTouchscreenType.MPC3x201:
					return new MPC3x201TouchScreenControl(parent, id);
				case eTouchscreenType.MPC3x30x:
					return new MPC3x30xTouchScreenControl(parent, id);

				default:
					return null;
			}
		}
#endif

		#region Methods

		/// <summary>
		/// Release resources.
		/// </summary>
		protected override void DisposeFinal(bool disposing)
		{
			base.DisposeFinal(disposing);
			DisposeLoadedControls();

#if !NETSTANDARD
			SetControlSystem(null);
#endif
		}

#if !NETSTANDARD
		/// <summary>
		/// Sets the wrapped CrestronControlSystem instance.
		/// </summary>
		/// <param name="controlSystem"></param>
		[PublicAPI]
		public void SetControlSystem(CrestronControlSystem controlSystem)
		{
			if (controlSystem == ControlSystem)
				return;

			ControlSystem = controlSystem;

			// enable switcher controls
			if (ControlSystem != null)
			{
				ISystemControl control = ControlSystem.SystemControl;
				if (control != null)
				{
					Dmps3SystemControl systemControl = control as Dmps3SystemControl;
					if (systemControl != null)
						systemControl.SystemPowerOn();
					if (control.VideoEnter.Type == eSigType.Bool)
						control.VideoEnter.BoolValue = true;
					if (control.AudioEnter.Type == eSigType.Bool)
						control.AudioEnter.BoolValue = true;
					if (control.EnableAudioBreakaway.Type == eSigType.Bool)
						control.EnableAudioBreakaway.BoolValue = true;
					if (control.USBEnter.Type == eSigType.Bool)
						control.USBEnter.BoolValue = true;
					if (control.EnableUSBBreakaway.Type == eSigType.Bool)
						control.EnableUSBBreakaway.BoolValue = true;

					control.FrontPanelLockOn();
				}
			}

			UpdateCachedOnlineStatus();
		}
#endif


		public void LoadControls(string path)
		{
			m_ConfigPath = path;

			string fullPath = PathUtils.GetDefaultConfigPath("DMPS3", path);

			try
			{
				string xml = IcdFile.ReadToEnd(fullPath, new UTF8Encoding(false));
				xml = EncodingUtils.StripUtf8Bom(xml);

				ParseXml(xml);
			}
			catch (Exception e)
			{
				Logger.Log(eSeverity.Error, "Failed to load integration config {0} - {1}", fullPath, e.Message);
			}
		}

		public bool TryGetMixerModeForOutput(int output, out eOutputMixerMode mixerMode)
		{
			mixerMode = eOutputMixerMode.None;
			switch (output)
			{
				case 1:
					mixerMode = Output1MixerMode;
					return true;
				case 2:
					mixerMode = Output2MixerMode;
					return true;
				case 3:
					mixerMode = Output3MixerMode;
					return true;
				case 4:
					mixerMode = Output4MixerMode;
					return true;
				default:
					return false;
			}
		}

		#region IO

#if !NETSTANDARD
		/// <summary>
		/// Gets the port at the given address.
		/// </summary>
		/// <param name="address"></param>
		/// <returns></returns>
		public IROutputPort GetIrOutputPort(int address)
		{
			if (ControlSystem.IROutputPorts == null)
				throw new NotSupportedException("Control System has no IrPorts");

			if (address < 0 || !ControlSystem.IROutputPorts.Contains((uint)address))
				throw new ArgumentOutOfRangeException("address", string.Format("{0} has no IrPort at address {1}", this, address));

			return ControlSystem.IROutputPorts[(uint)address];
		}

		/// <summary>
		/// Gets the port at the given address.
		/// </summary>
		/// <param name="address"></param>
		/// <returns></returns>
		public Relay GetRelayPort(int address)
		{
			if (ControlSystem.RelayPorts == null)
				throw new NotSupportedException("Control System has no RelayPorts");

			if (address < 0 || !ControlSystem.RelayPorts.Contains((uint)address))
				throw new ArgumentOutOfRangeException("address", string.Format("{0} has no RelayPort at address {1}", this, address));

			return ControlSystem.RelayPorts[(uint)address];
		}

		/// <summary>
		/// Gets the port at the given address.
		/// </summary>
		/// <param name="address"></param>
		/// <returns></returns>
		public Versiport GetIoPort(int address)
		{
			if (ControlSystem.VersiPorts == null)
				throw new NotSupportedException("Control System has no IoPorts");

			if (address < 0 || !ControlSystem.VersiPorts.Contains((uint)address))
				throw new ArgumentOutOfRangeException("address", string.Format("{0} has no IoPort at address {1}", this, address));

			return ControlSystem.VersiPorts[(uint)address];
		}

		/// <summary>
		/// Gets the port at the given address.
		/// </summary>
		/// <param name="address"></param>
		/// <returns></returns>
		public DigitalInput GetDigitalInputPort(int address)
		{
			if (ControlSystem.DigitalInputPorts == null)
				throw new NotSupportedException("Control System has no DigitalInputPorts");

			if (address < 0 || !ControlSystem.DigitalInputPorts.Contains((uint)address))
				throw new ArgumentOutOfRangeException("address", string.Format("{0} has no DigitalInput at address {1}", this, address));

			return ControlSystem.DigitalInputPorts[(uint)address];
		}

		/// <summary>
		/// Gets the port at the given address.
		/// Todo: Implement this for DMPS control systems
		/// </summary>
		/// <param name="io"></param>
		/// <param name="address"></param>
		/// <returns></returns>
		public Cec GetCecPort(eInputOuptut io, int address)
		{
			string message = string.Format("{0} has no {1}", this, typeof(Cec).Name);
			throw new ArgumentOutOfRangeException("address", message);
		}

		/// <summary>
		/// Gets the port at the given address.
		/// </summary>
		/// <param name="address"></param>
		/// <returns></returns>
		public ComPort GetComPort(int address)
		{
			if (ControlSystem.ComPorts == null)
				throw new NotSupportedException("Control System has no ComPorts");

			if (address < 0 || !ControlSystem.ComPorts.Contains((uint)address))
				throw new ArgumentOutOfRangeException("address", string.Format("{0} has no ComPort at address {1}", this, address));

			return ControlSystem.ComPorts[(uint)address];
		}

		/// <summary>
		/// Gets the DMInput at the given address.
		/// </summary>
		/// <param name="address"></param>
		/// <returns></returns>
		public DMInput GetDmInput(int address)
		{
			if (ControlSystem.SwitcherInputs == null)
				throw new NotSupportedException("Control System has no DmInputs");

			if (address < 0 || !ControlSystem.SwitcherInputs.Contains((uint)address))
				throw new ArgumentOutOfRangeException("address", string.Format("{0} has no input at address {1}", this, address));

			return ControlSystem.SwitcherInputs[(uint)address] as DMInput;
		}

		/// <summary>
		/// Gets the DMOutput at the given address.
		/// </summary>
		/// <param name="address"></param>
		/// <returns></returns>
		public DMOutput GetDmOutput(int address)
		{
			if (ControlSystem.SwitcherOutputs == null)
				throw new NotSupportedException("Control System has no DmOutputs");

			if (address < 0 || !ControlSystem.SwitcherOutputs.Contains((uint)address))
				throw new ArgumentOutOfRangeException("address", string.Format("{0} has no output at address {1}", this, address));

			return ControlSystem.SwitcherOutputs[(uint)address] as DMOutput;
		}

#endif

		#endregion

		#endregion

		#region Private Methods

		/// <summary>
		/// Gets the current online status of the device.
		/// </summary>
		/// <returns></returns>
		protected override bool GetIsOnlineStatus()
		{
#if !NETSTANDARD
			return ControlSystem != null;
#else
			return false;
#endif
		}

		private void ParseXml(string xml)
		{
			DisposeLoadedControls();

			// Load and add the new controls
			foreach (IDeviceControl control in Dmps3XmlUtils.GetControlsFromXml(xml, this))
			{
				Controls.Add(control);
				m_LoadedControls.Add(control);
			}
		}

		private void DisposeLoadedControls()
		{
			foreach (var control in m_LoadedControls)
				control.Dispose();

			m_LoadedControls.Clear();
		}

		private void FrontPanelLockEnable()
		{
#if !NETSTANDARD
			if (ControlSystem == null)
				return;

			ISystemControl control = ControlSystem.SystemControl;
			if (control == null)
				return;

			control.FrontPanelLockOn();
#else
			throw new NotSupportedException();
#endif
		}

		private void FrontPanelLockDisable()
		{
#if !NETSTANDARD
			if (ControlSystem == null)
				return;

			ISystemControl control = ControlSystem.SystemControl;
			if (control == null)
				return;

			control.FrontPanelLockOff();
#else
			throw new NotSupportedException();
#endif
		}

		private bool? FrontPanelLockStatus
		{
			get
			{
#if !NETSTANDARD
				if (ControlSystem == null)
					return null;

				ISystemControl control = ControlSystem.SystemControl;
				if (control == null)
					return null;

				return control.FrontPanelLockOnFeedback.GetBoolValueOrDefault();
#else
				return null;
#endif
			}
		}

		#endregion

		#region Settings

		protected override void ClearSettingsFinal()
		{
			base.ClearSettingsFinal();

			m_ConfigPath = null;

			Output1MixerMode = eOutputMixerMode.Auto;
			Output2MixerMode = eOutputMixerMode.Auto;
			Output3MixerMode = eOutputMixerMode.Auto;
			Output4MixerMode = eOutputMixerMode.Auto;
		}

		protected override void CopySettingsFinal(ControlSystemDeviceSettings settings)
		{
			base.CopySettingsFinal(settings);

			settings.Config = m_ConfigPath;

			settings.Output1MixerMode = Output1MixerMode;
			settings.Output2MixerMode = Output2MixerMode;
			settings.Output3MixerMode = Output3MixerMode;
			settings.Output4MixerMode = Output4MixerMode;
		}

		protected override void ApplySettingsFinal(ControlSystemDeviceSettings settings, IDeviceFactory factory)
		{
			base.ApplySettingsFinal(settings, factory);

			if (!string.IsNullOrEmpty(settings.Config))
				LoadControls(settings.Config);

			Output1MixerMode = settings.Output1MixerMode;
			Output2MixerMode = settings.Output2MixerMode;
			Output3MixerMode = settings.Output3MixerMode;
			Output4MixerMode = settings.Output4MixerMode;
		}

		/// <summary>
		/// Override to add controls to the device.
		/// </summary>
		/// <param name="settings"></param>
		/// <param name="factory"></param>
		/// <param name="addControl"></param>
		protected override void AddControls(ControlSystemDeviceSettings settings, IDeviceFactory factory, Action<IDeviceControl> addControl)
		{
			base.AddControls(settings, factory, addControl);

#if !NETSTANDARD
			if (ControlSystem.SupportsSwitcherInputs || ControlSystem.SupportsSwitcherOutputs)
				addControl(new ControlSystemSwitcherControl(this, 0));

			IThreeSeriesTouchScreenControl touchScreen = InstantiateTouchScreen(this, 1);
			if (touchScreen != null)
				addControl(touchScreen);

			ControlSystemSwitcherControl control = Controls.GetControl<ControlSystemSwitcherControl>();
			if (control != null)
				control.SetupOutputMixers();
#endif
		}

		#endregion

		#region Console

		/// <summary>
		/// Gets the child console commands.
		/// </summary>
		/// <returns></returns>
		public override IEnumerable<IConsoleCommand> GetConsoleCommands()
		{
			foreach (IConsoleCommand command in GetBaseConsoleCommands())
				yield return command;

			yield return new ConsoleCommand("FrontPanelLockEnable", "Enables front panel lockout on switcher control systems", () => FrontPanelLockEnable());
			yield return new ConsoleCommand("FrontPanelLockDisable", "Disables front panel lockout on switcher control systems", () => FrontPanelLockDisable());
		}

		private IEnumerable<IConsoleCommand> GetBaseConsoleCommands()
		{
			return base.GetConsoleCommands();
		}

		/// <summary>
		/// Calls the delegate for each console status item.
		/// </summary>
		/// <param name="addRow"></param>
		public override void BuildConsoleStatus(AddStatusRowDelegate addRow)
		{
			base.BuildConsoleStatus(addRow);

			bool? lockout = FrontPanelLockStatus;
			if (lockout.HasValue)
				addRow("Front Panel Lockout", lockout.Value);

			addRow("Output1MixerMode", Output1MixerMode);
			addRow("Output2MixerMode", Output2MixerMode);
			addRow("Output3MixerMode", Output3MixerMode);
			addRow("Output4MixerMode", Output4MixerMode);
		}

		#endregion
	}
}
