using System;
using ICD.Common.Utils.EventArguments;
using ICD.Common.Utils.Extensions;
using ICD.Connect.Devices;
using ICD.Connect.Protocol.Ports;
using ICD.Connect.Protocol.Ports.ComPort;
using ICD.Connect.Routing.Extron.Devices.DtpCrosspointBase;
using ICD.Connect.Routing.Mock.Midpoint;
using ICD.Connect.Settings.Core;

namespace ICD.Connect.Routing.Extron.Devices.Dtp
{
	public abstract class AbstractDtpHdmiDevice<TSettings> : AbstractDevice<TSettings>, IDtpHdmiDevice
		where TSettings: IDtpHdmiDeviceSettings, new()
	{
		public event EventHandler<BoolEventArgs> OnInitializedChanged;

		public IDtpCrosspointDevice Parent { get; private set; }

		public AbstractDtpHdmiDevice()
		{
			Controls.Add(new MockRouteMidpointControl(this, 0));
		}

		#region Methods

		public abstract HostInfo? GetComPortHostInfo();

		public abstract void SetComPortSpec(eComBaudRates baudRate, eComDataBits dataBits, eComParityType parityType, eComStopBits stopBits);

		protected override bool GetIsOnlineStatus()
		{
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
			{
				Parent = factory.GetOriginatorById<IDtpCrosspointDevice>(settings.DtpSwitch.Value);
				Subscribe(Parent);
			}
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

		private void Subscribe(IDtpCrosspointDevice parent)
		{
			parent.OnInitializedChanged += ParentOnOnInitializedChanged;
		}

		private void Unsubscribe(IDtpCrosspointDevice parent)
		{
			parent.OnInitializedChanged -= ParentOnOnInitializedChanged;
		}

		private void ParentOnOnInitializedChanged(object sender, BoolEventArgs args)
		{
			OnInitializedChanged.Raise(this, args);
		}

		#endregion
	}
}