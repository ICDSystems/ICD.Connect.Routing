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

		#region Properties

		public IDtpCrosspointDevice Parent { get; private set; }

		private bool m_PortInitialized;

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

		#endregion

		/// <summary>
		/// Constructor.
		/// </summary>
		protected AbstractDtpHdmiDevice()
		{
			Controls.Add(new DtpHdmiMidpointControl<AbstractDtpHdmiDevice<TSettings>>(this, 0));
		}

		#region Methods

		public abstract ISerialPort GetSerialInsertionPort();

		public abstract void InitializeComPort(eComBaudRates baudRate, eComDataBits dataBits, eComParityType parityType, eComStopBits stopBits);

		protected override bool GetIsOnlineStatus()
		{
			if (Parent == null)
				return false;
			return Parent.IsOnline;
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

		#region Parent Callbacks

		protected virtual void Subscribe(IDtpCrosspointDevice parent)
		{
			parent.OnIsOnlineStateChanged += ParentOnOnIsOnlineStateChanged;
		}

		protected virtual void Unsubscribe(IDtpCrosspointDevice parent)
		{
			parent.OnIsOnlineStateChanged -= ParentOnOnIsOnlineStateChanged;
		}

		private void ParentOnOnIsOnlineStateChanged(object sender, DeviceBaseOnlineStateApiEventArgs e)
		{
			UpdateCachedOnlineStatus();
		}

		#endregion
	}
}