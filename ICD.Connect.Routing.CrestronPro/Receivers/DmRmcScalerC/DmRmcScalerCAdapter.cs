using System;
using System.Collections.Generic;
using Crestron.SimplSharpPro;
using Crestron.SimplSharpPro.DM;
using ICD.Common.Properties;
using ICD.Common.Services.Logging;
using ICD.Connect.Devices;
using ICD.Connect.Misc.CrestronPro.Devices;
using ICD.Connect.Misc.CrestronPro.Utils;
using ICD.Connect.Settings.Core;

namespace ICD.Connect.Routing.CrestronPro.Receivers.DmRmcScalerC
{
	/// <summary>
	/// DmRmcScalerCAdapter wraps a DmRmcScalerC to provide a routing device.
	/// </summary>
	public sealed class DmRmcScalerCAdapter : AbstractDevice<DmRmcScalerCAdapterSettings>, IPortParent
	{
		public delegate void ScalerChangeCallback(
			DmRmcScalerCAdapter sender, Crestron.SimplSharpPro.DM.Endpoints.Receivers.DmRmcScalerC scaler);

		/// <summary>
		/// Raised when the wrapped scaler changes.
		/// </summary>
		public event ScalerChangeCallback OnScalerChanged;

		private Crestron.SimplSharpPro.DM.Endpoints.Receivers.DmRmcScalerC m_Scaler;
		private int? m_ParentId;

		#region Properties

		/// <summary>
		/// Gets the wrapped scaler.
		/// </summary>
		public Crestron.SimplSharpPro.DM.Endpoints.Receivers.DmRmcScalerC Scaler
		{
			get { return m_Scaler; }
			private set
			{
				if (value == m_Scaler)
					return;

				m_Scaler = value;

				ScalerChangeCallback handler = OnScalerChanged;
				if (handler != null)
					handler(this, m_Scaler);
			}
		}

		#endregion

		/// <summary>
		/// Constructor.
		/// </summary>
		public DmRmcScalerCAdapter()
		{
			Controls.Add(new DmRmcScalerCRouteControl(this));
		}

		#region Methods

		/// <summary>
		/// Release resources
		/// </summary>
		protected override void DisposeFinal(bool disposing)
		{
			base.DisposeFinal(disposing);

			// Unsbscribe and unregister
			SetScaler(null, null);
		}

		/// <summary>
		/// Sets the wrapped scaler.
		/// </summary>
		/// <param name="scaler"></param>
		/// <param name="parentId"></param>
		[PublicAPI]
		public void SetScaler(Crestron.SimplSharpPro.DM.Endpoints.Receivers.DmRmcScalerC scaler, int? parentId)
		{
			Unsubscribe(Scaler);

			if (Scaler != null)
			{
				if (Scaler.Registered)
					Scaler.UnRegister();

				try
				{
					Scaler.Dispose();
				}
				catch
				{
				}
			}

			m_ParentId = parentId;
			Scaler = scaler;

			if (Scaler != null && !Scaler.Registered)
			{
				if (Name != null)
					Scaler.Description = Name;
				eDeviceRegistrationUnRegistrationResponse result = Scaler.Register();
				if (result != eDeviceRegistrationUnRegistrationResponse.Success)
					Logger.AddEntry(eSeverity.Error, "Unable to register {0} - {1}", Scaler.GetType().Name, result);
			}

			Subscribe(Scaler);
			UpdateCachedOnlineStatus();
		}

		/// <summary>
		/// Gets the port at the given addres.
		/// </summary>
		/// <param name="address"></param>
		/// <returns></returns>
		public ComPort GetComPort(int address)
		{
			if (Scaler == null)
				throw new InvalidOperationException("No scaler instantiated");

			if (address == 1)
				return Scaler.ComPorts[1];

			string message = string.Format("{0} has no {1} with address {2}", this, typeof(ComPort).Name, address);
			throw new KeyNotFoundException(message);
		}

		/// <summary>
		/// Gets the port at the given addres.
		/// </summary>
		/// <param name="address"></param>
		/// <returns></returns>
		public IROutputPort GetIrOutputPort(int address)
		{
			if (Scaler == null)
				throw new InvalidOperationException("No scaler instantiated");

			if (address == 1)
				return Scaler.IROutputPorts[1];

			string message = string.Format("{0} has no {1} with address {2}", this, typeof(IROutputPort).Name, address);
			throw new KeyNotFoundException(message);
		}

		/// <summary>
		/// Gets the port at the given address.
		/// </summary>
		/// <param name="address"></param>
		/// <returns></returns>
		public Relay GetRelayPort(int address)
		{
			string message = string.Format("{0} has no {1} with address {2}", this, typeof(Relay).Name, address);
			throw new KeyNotFoundException(message);
		}

		/// <summary>
		/// Gets the port at the given address.
		/// </summary>
		/// <param name="address"></param>
		/// <returns></returns>
		public Versiport GetIoPort(int address)
		{
			string message = string.Format("{0} has no {1} with address {2}", this, typeof(Versiport).Name, address);
			throw new KeyNotFoundException(message);
		}

		#endregion

		#region Settings

		/// <summary>
		/// Override to apply properties to the settings instance.
		/// </summary>
		/// <param name="settings"></param>
		protected override void CopySettingsFinal(DmRmcScalerCAdapterSettings settings)
		{
			base.CopySettingsFinal(settings);

			DMOutput input = m_Scaler == null ? null : m_Scaler.DMOutput;

			settings.Ipid = m_Scaler == null ? (byte)0 : (byte)m_Scaler.ID;
			settings.DmSwitch = m_ParentId;
			settings.DmOutputAddress = input == null ? (int?)null : (int)input.Number;
		}

		/// <summary>
		/// Override to clear the instance settings.
		/// </summary>
		protected override void ClearSettingsFinal()
		{
			base.ClearSettingsFinal();

			SetScaler(null, null);
		}

		/// <summary>
		/// Override to apply settings to the instance.
		/// </summary>
		/// <param name="settings"></param>
		/// <param name="factory"></param>
		protected override void ApplySettingsFinal(DmRmcScalerCAdapterSettings settings, IDeviceFactory factory)
		{
			base.ApplySettingsFinal(settings, factory);

			Crestron.SimplSharpPro.DM.Endpoints.Receivers.DmRmcScalerC scaler =
				DmEndpointFactoryUtils.InstantiateEndpoint<Crestron.SimplSharpPro.DM.Endpoints.Receivers.DmRmcScalerC>(settings.Ipid, settings.DmOutputAddress,
				                                                         settings.DmSwitch, factory,
				                                                         InstantiateScaler,
				                                                         InstantiateScaler,
				                                                         InstantiateScaler);

			SetScaler(scaler, settings.DmSwitch);
		}

		private static Crestron.SimplSharpPro.DM.Endpoints.Receivers.DmRmcScalerC InstantiateScaler(byte ipid, CrestronControlSystem controlSystem)
		{
			return new Crestron.SimplSharpPro.DM.Endpoints.Receivers.DmRmcScalerC(ipid, controlSystem);
		}

		private static Crestron.SimplSharpPro.DM.Endpoints.Receivers.DmRmcScalerC InstantiateScaler(byte ipid, DMOutput output)
		{
			return new Crestron.SimplSharpPro.DM.Endpoints.Receivers.DmRmcScalerC(ipid, output);
		}

		private static Crestron.SimplSharpPro.DM.Endpoints.Receivers.DmRmcScalerC InstantiateScaler(DMOutput output)
		{
			return new Crestron.SimplSharpPro.DM.Endpoints.Receivers.DmRmcScalerC(output);
		}

		#endregion

		#region Private Methods

		/// <summary>
		/// Subscribe to the scaler events.
		/// </summary>
		/// <param name="scaler"></param>
		private void Subscribe(Crestron.SimplSharpPro.DM.Endpoints.Receivers.DmRmcScalerC scaler)
		{
			if (scaler == null)
				return;

			scaler.OnlineStatusChange += ScalerOnlineStatusChange;
		}

		/// <summary>
		/// Unsubscribes from the scaler events.
		/// </summary>
		/// <param name="scaler"></param>
		private void Unsubscribe(Crestron.SimplSharpPro.DM.Endpoints.Receivers.DmRmcScalerC scaler)
		{
			if (scaler == null)
				return;

			scaler.OnlineStatusChange -= ScalerOnlineStatusChange;
		}

		/// <summary>
		/// Called when the device online status changes.
		/// </summary>
		/// <param name="currentDevice"></param>
		/// <param name="args"></param>
		private void ScalerOnlineStatusChange(GenericBase currentDevice, OnlineOfflineEventArgs args)
		{
			UpdateCachedOnlineStatus();
		}

		/// <summary>
		/// Gets the current online status of the device.
		/// </summary>
		/// <returns></returns>
		protected override bool GetIsOnlineStatus()
		{
			return Scaler != null && Scaler.IsOnline;
		}

		#endregion
	}
}
