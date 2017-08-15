#if SIMPLSHARP
using Crestron.SimplSharpPro;
using Crestron.SimplSharpPro.DM;
using ICD.Connect.Misc.CrestronPro.Utils;
#endif
using ICD.Common.Properties;
using ICD.Common.Services.Logging;
using ICD.Common.Utils;
using ICD.Connect.API.Nodes;
using ICD.Connect.Devices;
using ICD.Connect.Settings.Core;
using System;

namespace ICD.Connect.Routing.CrestronPro.Transmitters.DmTx200Base
{
    /// <summary>
    /// Base class for DmTx200 device adapters.
    /// </summary>
    /// <typeparam name="TTransmitter"></typeparam>
    /// <typeparam name="TSettings"></typeparam>
#if SIMPLSHARP
	public abstract class AbstractDmTx200BaseAdapter<TTransmitter, TSettings> : AbstractDevice<TSettings>
		where TTransmitter : Crestron.SimplSharpPro.DM.Endpoints.Transmitters.DmTx200Base
#else
    public abstract class AbstractDmTx200BaseAdapter<TSettings> : AbstractDevice<TSettings>
#endif
        where TSettings : AbstractDmTx200BaseAdapterSettings, new()
	{
#if SIMPLSHARP
        public delegate void TransmitterChangeCallback(object sender, TTransmitter transmitter);

		/// <summary>
		/// Raised when the wrapped transmitter changes.
		/// </summary>
		public event TransmitterChangeCallback OnTransmitterChanged;

        private TTransmitter m_Transmitter;
#endif
		private int? m_ParentId;

        #region Properties

#if SIMPLSHARP
        /// <summary>
        /// Gets the transmitter.
        /// </summary>
        public TTransmitter Transmitter
		{
			get { return m_Transmitter; }
			private set
			{
				if (value == m_Transmitter)
					return;

				m_Transmitter = value;

				TransmitterChangeCallback handler = OnTransmitterChanged;
				if (handler != null)
					handler(this, m_Transmitter);
			}
		}
#endif

#endregion

#region Methods

		/// <summary>
		/// Release resources.
		/// </summary>
		protected override void DisposeFinal(bool disposing)
		{
			base.DisposeFinal(disposing);

#if SIMPLSHARP
            // Unsubscribe and unregister.
            SetTransmitter(null, null);
#endif
		}

#if SIMPLSHARP
        /// <summary>
        /// Sets the wrapped transmitter.
        /// </summary>
        /// <param name="transmitter"></param>
        /// <param name="parentId">The id of the parent DM Switch device.</param>
        [PublicAPI]
		public void SetTransmitter(TTransmitter transmitter, int? parentId)
		{
			if (Transmitter != null)
			{
				if (Transmitter.Registered)
					Transmitter.UnRegister();

				try
				{
					Transmitter.Dispose();
				}
				catch
				{
				}
			}

			m_ParentId = parentId;

			Unsubscribe(Transmitter);
			Transmitter = transmitter;
			Subscribe(Transmitter);

			if (Transmitter != null && !Transmitter.Registered)
			{
				if (Name != null)
					Transmitter.Description = Name;
				eDeviceRegistrationUnRegistrationResponse result = Transmitter.Register();
				if (result != eDeviceRegistrationUnRegistrationResponse.Success)
					Logger.AddEntry(eSeverity.Error, "Unable to register {0} - {1}", Transmitter.GetType().Name, result);

				Transmitter.VideoSource = Crestron.SimplSharpPro.DM.Endpoints.Transmitters.DmTx200Base.eSourceSelection.Auto;
			}

			UpdateCachedOnlineStatus();
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
            DMInput input = Transmitter == null ? null : Transmitter.DMInput;

			settings.Ipid = Transmitter == null ? (byte?)null : (byte)Transmitter.ID;
			settings.DmSwitch = m_ParentId;
			settings.DmInputAddress = input == null ? (int?)null : (int)input.Number;
#else
            settings.Ipid = null;
            settings.DmSwitch = m_ParentId;
            settings.DmInputAddress = null;
#endif
        }

		/// <summary>
		/// Override to clear the instance settings.
		/// </summary>
		protected override void ClearSettingsFinal()
		{
			base.ClearSettingsFinal();

#if SIMPLSHARP
            SetTransmitter(null, null);
#endif
		}

		/// <summary>
		/// Override to apply settings to the instance.
		/// </summary>
		/// <param name="settings"></param>
		/// <param name="factory"></param>
		protected override void ApplySettingsFinal(TSettings settings, IDeviceFactory factory)
		{
			base.ApplySettingsFinal(settings, factory);

#if SIMPLSHARP
            TTransmitter transmitter =
				DmEndpointFactoryUtils.InstantiateEndpoint<TTransmitter>(settings.Ipid, settings.DmInputAddress,
				                                                         settings.DmSwitch, factory,
				                                                         InstantiateTransmitter,
				                                                         InstantiateTransmitter,
				                                                         InstantiateTransmitter);

			SetTransmitter(transmitter, settings.DmSwitch);
#else
            throw new NotImplementedException();
#endif
        }

#if SIMPLSHARP
        protected abstract TTransmitter InstantiateTransmitter(byte ipid, CrestronControlSystem controlSystem);

		protected abstract TTransmitter InstantiateTransmitter(byte ipid, DMInput input);

		protected abstract TTransmitter InstantiateTransmitter(DMInput input);
#endif

#endregion

#region Private Methods

		/// <summary>
		/// Gets the current online status of the device.
		/// </summary>
		/// <returns></returns>
		protected override bool GetIsOnlineStatus()
		{
#if SIMPLSHARP
            return Transmitter != null && Transmitter.IsOnline;
#else
            return false;
#endif
        }

        #endregion

#region Transmitter callbacks

#if SIMPLSHARP
        /// <summary>
        /// Subscribes to the transmitter events.
        /// </summary>
        /// <param name="transmitter"></param>
        private void Subscribe(TTransmitter transmitter)
		{
			if (transmitter == null)
				return;

			transmitter.OnlineStatusChange += TransmitterOnlineStatusChange;
		}

		/// <summary>
		/// Unsubscribes from the transmitter events.
		/// </summary>
		/// <param name="transmitter"></param>
		private void Unsubscribe(TTransmitter transmitter)
		{
			if (transmitter == null)
				return;

			transmitter.OnlineStatusChange -= TransmitterOnlineStatusChange;
		}

		/// <summary>
		/// Called when the device goes online/offline.
		/// </summary>
		/// <param name="currentDevice"></param>
		/// <param name="args"></param>
		private void TransmitterOnlineStatusChange(GenericBase currentDevice, OnlineOfflineEventArgs args)
		{
			UpdateCachedOnlineStatus();
		}
#endif

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

			addRow("IPID", m_Transmitter == null ? null : StringUtils.ToIpIdString((byte)m_Transmitter.ID));
			addRow("DM Switch", m_ParentId);

			DMInput input = m_Transmitter == null ? null : m_Transmitter.DMInput;
			addRow("DM Input", input == null ? null : input.Number.ToString());
		}
#endif

#endregion
	}
}
