using System;
using ICD.Common.Utils.Services.Logging;
using ICD.Connect.Devices;
using ICD.Connect.Routing.CrestronPro.Cards;
using ICD.Connect.Settings.Core;
#if SIMPLSHARP
using Crestron.SimplSharpPro;
using Crestron.SimplSharpPro.DM;
using ICD.Connect.Misc.CrestronPro.Devices;
using ICD.Connect.Misc.CrestronPro.Utils.Extensions;
using ICD.Connect.Routing.CrestronPro.Utils;
using ICD.Common.Properties;
using ICD.Common.Utils;
using ICD.Connect.API.Nodes;
#endif

namespace ICD.Connect.Routing.CrestronPro.Receivers.DmRmcScalerCBase
{
	/// <summary>
	/// DmRmcScalerCAdapter wraps a DmRmcScalerC to provide a routing device.
	/// </summary>
#if SIMPLSHARP
	public abstract class AbstractDmRmcScalerCAdapter<TScaler, TSettings> : AbstractDevice<TSettings>, IPortParent,
	                                                                        IDmRmcScalerCAdapter
		where TScaler : Crestron.SimplSharpPro.DM.Endpoints.Receivers.DmRmcScalerC
#else
    public abstract class AbstractDmRmcScalerCAdapter<TSettings> : AbstractDevice<TSettings>
#endif
		where TSettings : IDmRmcScalerCAdapterSettings, new()
	{
#if SIMPLSHARP
		/// <summary>
		/// Raised when the wrapped scaler changes.
		/// </summary>
		public event DmRmcScalerCChangeCallback OnScalerChanged;

		private TScaler m_Scaler;
#endif
		private int? m_ParentId;

		#region Properties

#if SIMPLSHARP
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
#endif

		#endregion

		/// <summary>
		/// Constructor.
		/// </summary>
		protected AbstractDmRmcScalerCAdapter()
		{
#if SIMPLSHARP
			Controls.Add(new DmRmcScalerCBaseRouteControl(this));
#endif
		}

		#region Methods

		/// <summary>
		/// Release resources
		/// </summary>
		protected override void DisposeFinal(bool disposing)
		{
			base.DisposeFinal(disposing);

#if SIMPLSHARP
			// Unsbscribe and unregister
			SetScaler(null, null);
#endif
		}

#if SIMPLSHARP
		/// <summary>
		/// Sets the wrapped scaler.
		/// </summary>
		/// <param name="scaler"></param>
		/// <param name="parentId"></param>
		[PublicAPI]
		public void SetScaler(TScaler scaler, int? parentId)
		{
			Unsubscribe(Scaler);
			Unregister(Scaler);

			m_ParentId = parentId;
			Scaler = scaler;

			Register(Scaler);
			Subscribe(Scaler);

			UpdateCachedOnlineStatus();
		}

		/// <summary>
		/// Unregisters the given scaler.
		/// </summary>
		/// <param name="scaler"></param>
		private void Unregister(TScaler scaler)
		{
			if (scaler == null || !scaler.Registered)
				return;

			scaler.UnRegister();

			try
			{
				scaler.Dispose();
			}
			catch
			{
			}
		}

		/// <summary>
		/// Registers the given scaler and re-registers the DM parent.
		/// </summary>
		/// <param name="scaler"></param>
		private void Register(TScaler scaler)
		{
			if (scaler == null || scaler.Registered)
				return;

			if (Name != null)
				scaler.Description = Name;

			eDeviceRegistrationUnRegistrationResponse result = scaler.Register();
			if (result != eDeviceRegistrationUnRegistrationResponse.Success)
			{
				Logger.AddEntry(eSeverity.Error, "{0} unable to register {1} - {2}", this, scaler.GetType().Name, result);
				return;
			}

			GenericDevice parent = scaler.Parent as GenericDevice;
			if (parent == null)
				return;

			eDeviceRegistrationUnRegistrationResponse parentResult = parent.ReRegister();
			if (parentResult != eDeviceRegistrationUnRegistrationResponse.Success)
			{
				Logger.AddEntry(eSeverity.Error, "{0} unable to register parent {1} - {2}", this, parent.GetType().Name,
				                parentResult);
			}
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
			throw new IndexOutOfRangeException(message);
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
			throw new IndexOutOfRangeException(message);
		}

		/// <summary>
		/// Gets the port at the given address.
		/// </summary>
		/// <param name="address"></param>
		/// <returns></returns>
		public virtual Relay GetRelayPort(int address)
		{
			string message = string.Format("{0} has no {1}", this, typeof(Relay).Name);
			throw new NotSupportedException(message);
		}

		/// <summary>
		/// Gets the port at the given address.
		/// </summary>
		/// <param name="address"></param>
		/// <returns></returns>
		public virtual Versiport GetIoPort(int address)
		{
			string message = string.Format("{0} has no {1}", this, typeof(Versiport).Name);
			throw new NotSupportedException(message);
		}

		/// <summary>
		/// Gets the port at the given address.
		/// </summary>
		/// <param name="address"></param>
		/// <returns></returns>
		public DigitalInput GetDigitalInputPort(int address)
		{
			string message = string.Format("{0} has no {1}", this, typeof(DigitalInput).Name);
			throw new NotSupportedException(message);
		}
#endif

		#endregion

		#region Settings

		/// <summary>
		/// Override to apply properties to the settings instance.
		/// </summary>
		/// <param name="settings"></param>
		protected override void CopySettingsFinal(TSettings settings)
		{
			base.CopySettingsFinal(settings);

#if SIMPLSHARP
			DMOutput input = m_Scaler == null ? null : m_Scaler.DMOutput;

			settings.Ipid = m_Scaler == null ? (byte)0 : (byte)m_Scaler.ID;
			settings.DmSwitch = m_ParentId;
			settings.DmOutputAddress = input == null ? (int?)null : (int)input.Number;
#else
            settings.Ipid = 0;
            settings.DmSwitch = m_ParentId;
            settings.DmOutputAddress = null;
#endif
		}

		/// <summary>
		/// Override to clear the instance settings.
		/// </summary>
		protected override void ClearSettingsFinal()
		{
			base.ClearSettingsFinal();

#if SIMPLSHARP
			SetScaler(null, null);
#endif
		}

		/// <summary>
		/// Override to apply settings to the instance.
		/// </summary>
		/// <param name="settings"></param>
		/// <param name="factory"></param>
		protected override void ApplySettingsFinal(TSettings settings, IDeviceFactory factory)
		{
			factory.LoadOriginators<ICardAdapter>();

			base.ApplySettingsFinal(settings, factory);

#if SIMPLSHARP
			TScaler scaler = null;

			try
			{
				scaler =
					DmEndpointFactoryUtils.InstantiateEndpoint<TScaler>(settings.Ipid, settings.DmOutputAddress,
					                                                    settings.DmSwitch, factory,
					                                                    InstantiateScaler,
					                                                    InstantiateScaler,
					                                                    InstantiateScaler);
			}
			catch (Exception e)
			{
				Logger.AddEntry(eSeverity.Error, "{0} failed to instantiate internal {1} - {2}",
				                this, typeof(TScaler).Name, e.Message);
			}

			SetScaler(scaler, settings.DmSwitch);
#else
            throw new NotImplementedException();
#endif
		}

#if SIMPLSHARP
		protected abstract TScaler InstantiateScaler(byte ipid, CrestronControlSystem controlSystem);

		protected abstract TScaler InstantiateScaler(byte ipid, DMOutput output);

		protected abstract TScaler InstantiateScaler(DMOutput output);
#endif

		#endregion

		#region Private Methods

#if SIMPLSHARP
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
#endif

		/// <summary>
		/// Gets the current online status of the device.
		/// </summary>
		/// <returns></returns>
		protected override bool GetIsOnlineStatus()
		{
#if SIMPLSHARP
			return Scaler != null && Scaler.IsOnline;
#else
            return false;
#endif
		}

		#endregion

		#region Console

#if SIMPLSHARP
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
#endif

		#endregion
	}
}
