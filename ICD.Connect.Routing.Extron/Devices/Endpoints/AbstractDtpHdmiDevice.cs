using System;
using ICD.Common.Properties;
using ICD.Common.Utils.EventArguments;
using ICD.Common.Utils.Extensions;
using ICD.Connect.Devices;
using ICD.Connect.Devices.Controls;
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

		[CanBeNull]
		private IDtpCrosspointDevice m_Parent;

		#region Properties

		[CanBeNull]
		public IDtpCrosspointDevice Parent
		{
			get { return m_Parent; }
			private set
			{
				if (value == m_Parent)
					return;

				Unsubscribe(m_Parent);
				m_Parent = value;
				Subscribe(m_Parent);
			}
		}

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
		/// Release resources.
		/// </summary>
		protected override void DisposeFinal(bool disposing)
		{
			OnPortInitialized = null;
			OnPortComSpecChanged = null;

			base.DisposeFinal(disposing);
		}

		#region Methods

		[CanBeNull]
		public ISerialPort GetSerialInsertionPort()
		{
			return Parent == null ? null : Parent.GetSerialInsertionPort(SwitcherAddress, SwitcherInputOutput);
		}

		public void InitializeComPort(eComBaudRates baudRate, eComDataBits dataBits, eComParityType parityType, eComStopBits stopBits)
		{
			if (Parent == null)
				throw new InvalidOperationException("Parent is null");

			Parent.SetComPortSpec(SwitcherAddress, SwitcherInputOutput, baudRate, dataBits, parityType, stopBits);
		}

		protected override bool GetIsOnlineStatus()
		{
			return Parent != null && Parent.IsOnline;
		}

		#endregion

		#region Parent Callbacks

		protected void Subscribe([CanBeNull] IDtpCrosspointDevice parent)
		{
			if (parent == null)
				return;

			parent.OnIsOnlineStateChanged += ParentOnOnIsOnlineStateChanged;
			parent.OnPortInitialized += ParentOnOnInputPortInitialized;
			parent.OnPortComSpecChanged += ParentOnPortComSpecChanged;
		}

		protected void Unsubscribe([CanBeNull] IDtpCrosspointDevice parent)
		{
			if (parent == null)
				return;

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

		/// <summary>
		/// Override to apply properties to the settings instance.
		/// </summary>
		/// <param name="settings"></param>
		protected override void CopySettingsFinal(TSettings settings)
		{
			base.CopySettingsFinal(settings);

			settings.DtpSwitch = Parent == null ? (int?)null : Parent.Id;
		}

		/// <summary>
		/// Override to apply settings to the instance.
		/// </summary>
		/// <param name="settings"></param>
		/// <param name="factory"></param>
		protected override void ApplySettingsFinal(TSettings settings, IDeviceFactory factory)
		{
			base.ApplySettingsFinal(settings, factory);

			Parent = settings.DtpSwitch == null ? null : factory.GetOriginatorById<IDtpCrosspointDevice>(settings.DtpSwitch.Value);
		}

		/// <summary>
		/// Override to clear the instance settings.
		/// </summary>
		protected override void ClearSettingsFinal()
		{
			base.ClearSettingsFinal();

			Parent = null;
		}

		/// <summary>
		/// Override to add controls to the device.
		/// </summary>
		/// <param name="settings"></param>
		/// <param name="factory"></param>
		/// <param name="addControl"></param>
		protected override void AddControls(TSettings settings, IDeviceFactory factory, Action<IDeviceControl> addControl)
		{
			base.AddControls(settings, factory, addControl);

			addControl(new DtpHdmiMidpointControl<AbstractDtpHdmiDevice<TSettings>>(this, 0));
		}

		#endregion
	}
}