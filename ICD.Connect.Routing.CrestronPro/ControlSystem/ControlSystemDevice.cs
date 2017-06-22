using System.Collections.Generic;
using Crestron.SimplSharpPro;
using Crestron.SimplSharpPro.DM;
using ICD.Common.Properties;
using ICD.Connect.Devices;
using ICD.Connect.Misc.CrestronPro;
using ICD.Connect.Misc.CrestronPro.Devices;

namespace ICD.Connect.Routing.CrestronPro.ControlSystem
{
	/// <summary>
	/// Wraps a CrestronControlSystem to provide a device that can be used as a port provider and a switcher.
	/// </summary>
	public sealed class ControlSystemDevice : AbstractDevice<ControlSystemDeviceSettings>, IPortParent, IDmParent
	{
		public delegate void ControlSystemChangeCallback(ControlSystemDevice sender, CrestronControlSystem controlSystem);

		/// <summary>
		/// Raised when the wrapped control system changes.
		/// </summary>
		public event ControlSystemChangeCallback OnControlSystemChanged;

		private CrestronControlSystem m_ControlSystem;

		#region Controls

		/// <summary>
		/// Gets the wrapped Crestron control system.
		/// </summary>
		public CrestronControlSystem ControlSystem
		{
			get { return m_ControlSystem; }
			private set
			{
				if (value == m_ControlSystem)
					return;

				m_ControlSystem = value;

				ControlSystemChangeCallback handler = OnControlSystemChanged;
				if (handler != null)
					handler(this, m_ControlSystem);
			}
		}

		#endregion

		/// <summary>
		/// Constructor.
		/// </summary>
		public ControlSystemDevice()
		{
			SetControlSystem(ProgramInfo.ControlSystem);

			Controls.Add(new ControlSystemSwitcherControl(this));
		}

		#region Methods

		/// <summary>
		/// Release resources.
		/// </summary>
		protected override void DisposeFinal(bool disposing)
		{
			base.DisposeFinal(disposing);

			SetControlSystem(null);
		}

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
					if (control.USBEnter.Type == eSigType.Bool)
						control.USBEnter.BoolValue = true;
				}
			}

			UpdateCachedOnlineStatus();
		}

		#region IO

		/// <summary>
		/// Gets the port at the given addres.
		/// </summary>
		/// <param name="address"></param>
		/// <returns></returns>
		public IROutputPort GetIrOutputPort(int address)
		{
			if (ControlSystem.IROutputPorts == null)
				throw new KeyNotFoundException("Control System has no IrPorts");

			if (address < 0 || !ControlSystem.IROutputPorts.Contains((uint)address))
				throw new KeyNotFoundException(string.Format("{0} has no IrPort at address {1}", this, address));

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
				throw new KeyNotFoundException("Control System has no RelayPorts");

			if (address < 0 || !ControlSystem.RelayPorts.Contains((uint)address))
				throw new KeyNotFoundException(string.Format("{0} has no RelayPort at address {1}", this, address));

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
				throw new KeyNotFoundException("Control System has no IoPorts");

			if (address < 0 || !ControlSystem.VersiPorts.Contains((uint)address))
				throw new KeyNotFoundException(string.Format("{0} has no IoPort at address {1}", this, address));

			return ControlSystem.VersiPorts[(uint)address];
		}

		/// <summary>
		/// Gets the port at the given addres.
		/// </summary>
		/// <param name="address"></param>
		/// <returns></returns>
		public ComPort GetComPort(int address)
		{
			if (ControlSystem.ComPorts == null)
				throw new KeyNotFoundException("Control System has no ComPorts");

			if (address < 0 || !ControlSystem.ComPorts.Contains((uint)address))
				throw new KeyNotFoundException(string.Format("{0} has no ComPort at address {1}", this, address));

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
				throw new KeyNotFoundException("Control System has no DmInputs");

			if (address < 0 || !ControlSystem.SwitcherInputs.Contains((uint)address))
				throw new KeyNotFoundException(string.Format("{0} has no input at address {1}", this, address));

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
				throw new KeyNotFoundException("Control System has no DmOutputs");

			if (address < 0 || !ControlSystem.SwitcherOutputs.Contains((uint)address))
				throw new KeyNotFoundException(string.Format("{0} has no output at address {1}", this, address));

			return ControlSystem.SwitcherOutputs[(uint)address] as DMOutput;
		}

		#endregion

		#endregion

		#region Private Methods

		/// <summary>
		/// Gets the current online status of the device.
		/// </summary>
		/// <returns></returns>
		protected override bool GetIsOnlineStatus()
		{
			return ControlSystem != null;
		}

		#endregion
	}
}
