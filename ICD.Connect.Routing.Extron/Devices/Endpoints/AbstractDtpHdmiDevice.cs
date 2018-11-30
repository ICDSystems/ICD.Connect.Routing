using System;
using ICD.Common.Utils.EventArguments;
using ICD.Common.Utils.Extensions;
using ICD.Connect.Devices;
using ICD.Connect.Devices.EventArguments;
using ICD.Connect.Protocol.Ports;
using ICD.Connect.Protocol.Ports.ComPort;
using ICD.Connect.Routing.Extron.Devices.Switchers.DtpCrosspoint;
using ICD.Connect.Settings;

namespace ICD.Connect.Routing.Extron.Devices.Endpoints
{
	public abstract class AbstractDtpHdmiDevice<TSettings> : AbstractDevice<TSettings>, IDtpHdmiDevice
		where TSettings : IDtpHdmiDeviceSettings, new()
	{
		public event EventHandler<BoolEventArgs> OnPortInitialized;

		/// <summary>
		/// Raised when the comspec changes for the port.
		/// </summary>
		public event EventHandler<GenericEventArgs<ComSpec>> OnPortComSpecChanged;

		private bool m_PortInitialized;

		#region Properties

		public IDtpCrosspointDevice Parent { get; private set; }

		public bool PortInitialized
		{
			get { return m_PortInitialized; }
			protected set
			{
				if (value == m_PortInitialized)
					return;

				m_PortInitialized = value;

				OnPortInitialized.Raise(this, new BoolEventArgs(m_PortInitialized));
			}
		}

		/// <summary>
		/// Gets the address where this endpoint is connected to the switcher.
		/// </summary>
		public abstract int SwitcherAddress { get; }

		/// <summary>
		/// Returns Input for TX, Output for RX.
		/// </summary>
		public abstract eDtpInputOuput SwitcherInputOutput { get; }

		#endregion

		/// <summary>
		/// Constructor.
		/// </summary>
		protected AbstractDtpHdmiDevice()
		{
			Controls.Add(new DtpHdmiMidpointControl<AbstractDtpHdmiDevice<TSettings>>(this, 0));
		}

		/// <summary>
		/// Release resources.
		/// </summary>
		protected override void DisposeFinal(bool disposing)
		{
			OnPortInitialized = null;
			OnPortComSpecChanged = null;

			base.DisposeFinal(disposing);
		}

		#region Methods

		public ISerialPort GetSerialInsertionPort()
		{
			return Parent.GetSerialInsertionPort(SwitcherAddress, SwitcherInputOutput);
		}

		public void InitializeComPort(eComBaudRates baudRate, eComDataBits dataBits, eComParityType parityType, eComStopBits stopBits)
		{
			Parent.SetComPortSpec(SwitcherAddress, SwitcherInputOutput, baudRate, dataBits, parityType, stopBits);
		}

		protected override bool GetIsOnlineStatus()
		{
			if (Parent == null)
				return false;
			return Parent.IsOnline;
		}

		#endregion

		#region Parent Callbacks

		protected void Subscribe(IDtpCrosspointDevice parent)
		{
			parent.OnIsOnlineStateChanged += ParentOnOnIsOnlineStateChanged;
			parent.OnPortInitialized += ParentOnOnInputPortInitialized;
			parent.OnPortComSpecChanged += ParentOnPortComSpecChanged;
		}

		protected void Unsubscribe(IDtpCrosspointDevice parent)
		{
			parent.OnIsOnlineStateChanged -= ParentOnOnIsOnlineStateChanged;
			parent.OnPortInitialized -= ParentOnOnInputPortInitialized;
			parent.OnPortComSpecChanged -= ParentOnPortComSpecChanged;
		}

		private void ParentOnPortComSpecChanged(IDtpCrosspointDevice device, int address, eDtpInputOuput inputOutput, ComSpec comSpec)
		{
			if (address == SwitcherAddress && inputOutput == SwitcherInputOutput)
				OnPortComSpecChanged.Raise(this, new GenericEventArgs<ComSpec>(comSpec));
		}

		private void ParentOnOnInputPortInitialized(IDtpCrosspointDevice device, int address, eDtpInputOuput inputOutput)
		{
			if (address == SwitcherAddress && inputOutput == SwitcherInputOutput)
				PortInitialized = true;
		}

		private void ParentOnOnIsOnlineStateChanged(object sender, DeviceBaseOnlineStateApiEventArgs e)
		{
			UpdateCachedOnlineStatus();
		}

		#endregion

		#region Settings

		protected override void CopySettingsFinal(TSettings settings)
		{
			base.CopySettingsFinal(settings);

			settings.DtpSwitch = Parent.Id;
		}

		protected override void ApplySettingsFinal(TSettings settings, IDeviceFactory factory)
		{
			base.ApplySettingsFinal(settings, factory);

			if (settings.DtpSwitch != null)
				Parent = factory.GetOriginatorById<IDtpCrosspointDevice>(settings.DtpSwitch.Value);
			if (Parent != null)
				Subscribe(Parent);
		}

		protected override void ClearSettingsFinal()
		{
			base.ClearSettingsFinal();

			if (Parent != null)
				Unsubscribe(Parent);
			Parent = null;
		}

		#endregion
	}
}