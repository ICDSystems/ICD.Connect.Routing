using System;
using ICD.Common.Utils.Services.Logging;
#if SIMPLSHARP
using Crestron.SimplSharpPro;
#endif
using ICD.Common.Properties;
using ICD.Connect.Devices;
using ICD.Connect.Misc.CrestronPro;
using ICD.Connect.Settings.Core;

namespace ICD.Connect.Routing.CrestronPro.DigitalMedia.Dm100xStrBase
{
#if SIMPLSHARP
	public abstract class AbstractDm100XStrBaseAdapter<TSwitcher, TSettings> : AbstractDevice<TSettings>, IDm100XStrBaseAdapter
		where TSwitcher : Crestron.SimplSharpPro.DM.Streaming.Dm100xStrBase
#else
	public abstract class AbstractDm100XStrBaseAdapter<TSettings> : AbstractDevice<TSettings>, IDm100XStrBaseAdapter
#endif
		where TSettings : IDm100XStrBaseAdapterSettings, new()
	{
#if SIMPLSHARP
		public event Dm100XStrBaseChangeCallback OnSwitcherChanged;

        private TSwitcher m_Switcher;
#endif

        #region Properties

#if SIMPLSHARP
        /// <summary>
        /// Gets the wrapped switcher.
        /// </summary>
        public TSwitcher Switcher
		{
			get { return m_Switcher; }
			private set
			{
				if (value == m_Switcher)
					return;

				m_Switcher = value;

				Dm100XStrBaseChangeCallback handler = OnSwitcherChanged;
				if (handler != null)
					handler(this, m_Switcher);
			}
		}

		Crestron.SimplSharpPro.DM.Streaming.Dm100xStrBase IDm100XStrBaseAdapter.Switcher { get { return Switcher; } }
#endif

        #endregion

#region Methods

		/// <summary>
		/// Release resources.
		/// </summary>
		protected override void DisposeFinal(bool disposing)
		{
#if SIMPLSHARP
            OnSwitcherChanged = null;
#endif

			base.DisposeFinal(disposing);

#if SIMPLSHARP
            // Unsubscribe and unregister.
            SetSwitcher(null);
#endif
		}

#if SIMPLSHARP
        /// <summary>
        /// Sets the wrapped switcher.
        /// </summary>
        /// <param name="switcher"></param>
        [PublicAPI]
		public void SetSwitcher(TSwitcher switcher)
		{
			Unsubscribe(Switcher);

			if (Switcher != null)
			{
				if (Switcher.Registered)
					Switcher.UnRegister();

				try
				{
					Switcher.Dispose();
				}
				catch
				{
				}
			}

			Switcher = switcher;

			if (Switcher != null && !Switcher.Registered)
			{
				if (Name != null)
					Switcher.Description = Name;

				eDeviceRegistrationUnRegistrationResponse result = Switcher.Register();
				if (result != eDeviceRegistrationUnRegistrationResponse.Success)
					Log(eSeverity.Error, "Unable to register {0} - {1}", Switcher.GetType().Name, result);
			}

			Subscribe(Switcher);

			UpdateCachedOnlineStatus();
		}

#endif

        #endregion

		#region Ports

#if SIMPLSHARP
		/// <summary>
		/// Gets the port at the given addres.
		/// </summary>
		/// <param name="address"></param>
		/// <returns></returns>
		public virtual ComPort GetComPort(int address)
		{
			if (Switcher == null)
				throw new InvalidOperationException("No switcher instantiated");

			return Switcher.ComPorts[(uint)address];
		}

		/// <summary>
		/// Gets the port at the given addres.
		/// </summary>
		/// <param name="address"></param>
		/// <returns></returns>
		public virtual IROutputPort GetIrOutputPort(int address)
		{
			if (Switcher == null)
				throw new InvalidOperationException("No switcher instantiated");

			return Switcher.IROutputPorts[(uint)address];
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
            settings.EthernetId = Switcher == null ? (byte)0 : (byte)Switcher.ID;
#else
            settings.EthernetId = 0;
#endif
        }

		/// <summary>
		/// Override to clear the instance settings.
		/// </summary>
		protected override void ClearSettingsFinal()
		{
			base.ClearSettingsFinal();

#if SIMPLSHARP
            SetSwitcher(null);
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
            TSwitcher switcher = InstantiateSwitcher(settings.EthernetId, ProgramInfo.ControlSystem);
			SetSwitcher(switcher);
#else
            throw new NotImplementedException();
#endif
        }

#if SIMPLSHARP
        /// <summary>
        /// Creates a new instance of the wrapped internal switcher.
        /// </summary>
        /// <param name="ethernetId"></param>
        /// <param name="controlSystem"></param>
        /// <returns></returns>
        protected abstract TSwitcher InstantiateSwitcher(uint ethernetId, CrestronControlSystem controlSystem);
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
            return Switcher != null && Switcher.IsOnline;
#else
            return false;
#endif
        }

#if SIMPLSHARP
        /// <summary>
        /// Subscribe to the switcher events.
        /// </summary>
        /// <param name="switcher"></param>
        private void Subscribe(TSwitcher switcher)
		{
			if (switcher == null)
				return;

			switcher.OnlineStatusChange += SwitcherOnlineStatusChange;
		}

		/// <summary>
		/// Unsubscribe from the switcher events.
		/// </summary>
		/// <param name="switcher"></param>
		private void Unsubscribe(TSwitcher switcher)
		{
			if (switcher == null)
				return;

			switcher.OnlineStatusChange -= SwitcherOnlineStatusChange;
		}

		/// <summary>
		/// Called when the device online status changes.
		/// </summary>
		/// <param name="currentDevice"></param>
		/// <param name="args"></param>
		private void SwitcherOnlineStatusChange(GenericBase currentDevice, OnlineOfflineEventArgs args)
		{
			UpdateCachedOnlineStatus();
		}
#endif

#endregion
	}
}