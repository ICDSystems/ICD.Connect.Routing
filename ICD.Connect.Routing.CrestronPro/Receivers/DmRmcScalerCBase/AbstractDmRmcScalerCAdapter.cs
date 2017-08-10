using System;
using System.Collections.Generic;
using Crestron.SimplSharpPro;
using Crestron.SimplSharpPro.DM;
using ICD.Common.Properties;
using ICD.Common.Services.Logging;
using ICD.Common.Utils;
using ICD.Connect.API.Nodes;
using ICD.Connect.Devices;
using ICD.Connect.Misc.CrestronPro.Devices;
using ICD.Connect.Misc.CrestronPro.Utils;
using ICD.Connect.Settings.Core;

namespace ICD.Connect.Routing.CrestronPro.Receivers.DmRmcScalerCBase
{
	/// <summary>
	/// DmRmcScalerCAdapter wraps a DmRmcScalerC to provide a routing device.
	/// </summary>
	public abstract class AbstractDmRmcScalerCAdapter<TScaler, TSettings> : AbstractDevice<TSettings>, IPortParent, IDmRmcScalerCAdapter
		where TScaler : Crestron.SimplSharpPro.DM.Endpoints.Receivers.DmRmcScalerC
		where TSettings : IDmRmcScalerCAdapterSettings, new()
	{
		/// <summary>
		/// Raised when the wrapped scaler changes.
		/// </summary>
		public event DmRmcScalerCChangeCallback OnScalerChanged;

		private TScaler m_Scaler;
		private int? m_ParentId;

		#region Properties

		/// <summary>
		/// Gets the wrapped scaler.
		/// </summary>
		public TScaler Scaler
		{
			get { return m_Scaler; }
			private set
			{
				if (value == m_Scaler)
					return;

				m_Scaler = value;

				DmRmcScalerCChangeCallback handler = OnScalerChanged;
				if (handler != null)
					handler(this, m_Scaler);
			}
		}

		Crestron.SimplSharpPro.DM.Endpoints.Receivers.DmRmcScalerC IDmRmcScalerCAdapter.Scaler { get { return Scaler; } }

		#endregion

		/// <summary>
		/// Constructor.
		/// </summary>
		protected AbstractDmRmcScalerCAdapter()
		{
			Controls.Add(new DmRmcScalerCBaseRouteControl(this));
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
		public void SetScaler(TScaler scaler, int? parentId)
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
		public virtual ComPort GetComPort(int address)
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
		public virtual IROutputPort GetIrOutputPort(int address)
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
		public virtual Relay GetRelayPort(int address)
		{
			string message = string.Format("{0} has no {1} with address {2}", this, typeof(Relay).Name, address);
			throw new KeyNotFoundException(message);
		}

		/// <summary>
		/// Gets the port at the given address.
		/// </summary>
		/// <param name="address"></param>
		/// <returns></returns>
		public virtual Versiport GetIoPort(int address)
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
		protected override void CopySettingsFinal(TSettings settings)
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
		protected override void ApplySettingsFinal(TSettings settings, IDeviceFactory factory)
		{
			base.ApplySettingsFinal(settings, factory);

			TScaler scaler =
				DmEndpointFactoryUtils.InstantiateEndpoint<TScaler>(settings.Ipid, settings.DmOutputAddress,
				                                                    settings.DmSwitch, factory,
				                                                    InstantiateScaler,
				                                                    InstantiateScaler,
				                                                    InstantiateScaler);

			SetScaler(scaler, settings.DmSwitch);
		}

		protected abstract TScaler InstantiateScaler(byte ipid, CrestronControlSystem controlSystem);

		protected abstract TScaler InstantiateScaler(byte ipid, DMOutput output);

		protected abstract TScaler InstantiateScaler(DMOutput output);

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

		#region Console

		/// <summary>
		/// Calls the delegate for each console status item.
		/// </summary>
		/// <param name="addRow"></param>
		public override void BuildConsoleStatus(AddStatusRowDelegate addRow)
		{
			base.BuildConsoleStatus(addRow);

			addRow("IPID", m_Scaler == null ? null : StringUtils.ToIpIdString((byte)m_Scaler.ID));
			addRow("DM Switch", m_ParentId);

			DMOutput output = m_Scaler == null ? null : m_Scaler.DMOutput;
			addRow("DM Output", output == null ? null : output.Number.ToString());
		}

		#endregion
	}
}
