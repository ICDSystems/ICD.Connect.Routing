using System.Collections.Generic;
#if SIMPLSHARP
using Crestron.SimplSharpPro;
using Crestron.SimplSharpPro.DM;
using ICD.Connect.Misc.CrestronPro;
using ICD.Connect.Misc.CrestronPro.Devices;
#else
using System;
#endif
using ICD.Common.Properties;
using ICD.Common.Services.Logging;
using ICD.Connect.Devices;
using ICD.Connect.Settings.Core;

namespace ICD.Connect.Routing.CrestronPro.DigitalMedia.DmMdNXN
{
#if SIMPLSHARP
// ReSharper disable once InconsistentNaming
    public abstract class AbstractDmMdMNXNAdapter<TSwitcher, TSettings> : AbstractDevice<TSettings>, IDmMdMNXNAdapter, IDmParent
		where TSwitcher : DmMDMnxn
#else
    public abstract class AbstractDmMdMNXNAdapter<TSettings> : AbstractDevice<TSettings>
#endif
        where TSettings : IDmMdNXNAdapterSettings, new()
	{
#if SIMPLSHARP
		public event DmMdMNXNChangeCallback OnSwitcherChanged;

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

				DmMdMNXNChangeCallback handler = OnSwitcherChanged;
				if (handler != null)
					handler(this, m_Switcher);
			}
		}

		DmMDMnxn IDmMdMNXNAdapter.Switcher { get { return Switcher; } }
#endif

        #endregion

        /// <summary>
        /// Constructor.
        /// </summary>
        protected AbstractDmMdMNXNAdapter()
		{
#if SIMPLSHARP
            Controls.Add(new DmMdMNXNSwitcherControl(this));
#endif
		}

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
					Logger.AddEntry(eSeverity.Error, "Unable to register {0} - {1}", Switcher.GetType().Name, result);

				Switcher.EnableAudioBreakaway.BoolValue = true;
				Switcher.EnableUSBBreakaway.BoolValue = true;
				Switcher.VideoEnter.BoolValue = true;
				Switcher.AudioEnter.BoolValue = true;
				Switcher.USBEnter.BoolValue = true;
			}

			Subscribe(Switcher);

			UpdateCachedOnlineStatus();
		}

		/// <summary>
		/// Gets the DMInput at the given address.
		/// </summary>
		/// <param name="address"></param>
		/// <returns></returns>
		public DMInput GetDmInput(int address)
		{
			if (address < 0 || !m_Switcher.Inputs.Contains((uint)address))
				throw new KeyNotFoundException(string.Format("{0} has no input at address {1}", this, address));

			return m_Switcher.Inputs[(uint)address];
		}

		/// <summary>
		/// Gets the DMOutput at the given address.
		/// </summary>
		/// <param name="address"></param>
		/// <returns></returns>
		public DMOutput GetDmOutput(int address)
		{
			if (address < 0 || !m_Switcher.Outputs.Contains((uint)address))
				throw new KeyNotFoundException(string.Format("{0} has no output at address {1}", this, address));

			return m_Switcher.Outputs[(uint)address];
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
            settings.Ipid = Switcher == null ? (byte)0 : (byte)Switcher.ID;
#else
            settings.Ipid = 0;
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
            TSwitcher switcher = InstantiateSwitcher(settings.Ipid, ProgramInfo.ControlSystem);
			SetSwitcher(switcher);
#else
            throw new NotImplementedException();
#endif
        }

#if SIMPLSHARP
        /// <summary>
        /// Creates a new instance of the wrapped internal switcher.
        /// </summary>
        /// <param name="ipid"></param>
        /// <param name="controlSystem"></param>
        /// <returns></returns>
        protected abstract TSwitcher InstantiateSwitcher(ushort ipid, CrestronControlSystem controlSystem);
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
