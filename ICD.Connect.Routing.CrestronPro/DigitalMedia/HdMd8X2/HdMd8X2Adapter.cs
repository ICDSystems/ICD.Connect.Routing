#if SIMPLSHARP
using Crestron.SimplSharpPro;
using Crestron.SimplSharpPro.DM;
using ICD.Connect.Misc.CrestronPro;
#endif
using ICD.Common.Properties;
using ICD.Common.Services.Logging;
using ICD.Connect.Devices;
using ICD.Connect.Settings.Core;
using System;

namespace ICD.Connect.Routing.CrestronPro.DigitalMedia.HdMd8X2
{
	public sealed class HdMd8X2Adapter : AbstractDevice<HdMd8X2AdapterSettings>
	{
#if SIMPLSHARP
        public delegate void SwitcherChangeCallback(HdMd8X2Adapter sender, HdMd8x2 switcher);

		/// <summary>
		/// Raised when the wrapped switcher changes.
		/// </summary>
		public event SwitcherChangeCallback OnSwitcherChanged;

		private HdMd8x2 m_Switcher;
#endif

#region Properties

#if SIMPLSHARP
        /// <summary>
        /// Gets the wrapped switcher.
        /// </summary>
        public HdMd8x2 Switcher
		{
			get { return m_Switcher; }
			private set
			{
				if (value == m_Switcher)
					return;

				m_Switcher = value;

				SwitcherChangeCallback handler = OnSwitcherChanged;
				if (handler != null)
					handler(this, m_Switcher);
			}
		}
#endif

#endregion

#region Constructors

		/// <summary>
		/// Constructor.
		/// </summary>
		public HdMd8X2Adapter()
		{
#if SIMPLSHARP
            Controls.Add(new HdMd8X2SwitcherControl(this));
#endif
		}

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
            SetSwitcher(null);
#endif
		}

#if SIMPLSHARP
        /// <summary>
        /// Sets the wrapped switcher.
        /// </summary>
        /// <param name="switcher"></param>
        [PublicAPI]
		public void SetSwitcher(HdMd8x2 switcher)
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
			}

			Subscribe(Switcher);

			UpdateCachedOnlineStatus();
		}
#endif

#endregion

#region Settings

		/// <summary>
		/// Override to apply properties to the settings instance.
		/// </summary>
		/// <param name="settings"></param>
		protected override void CopySettingsFinal(HdMd8X2AdapterSettings settings)
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
		protected override void ApplySettingsFinal(HdMd8X2AdapterSettings settings, IDeviceFactory factory)
		{
			base.ApplySettingsFinal(settings, factory);

#if SIMPLSHARP
            HdMd8x2 switcher = new HdMd8x2(settings.Ipid, ProgramInfo.ControlSystem);
			SetSwitcher(switcher);
#else
            throw new NotImplementedException();
#endif
        }

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
        private void Subscribe(HdMd8x2 switcher)
		{
			if (switcher == null)
				return;

			switcher.OnlineStatusChange += SwitcherOnlineStatusChange;
		}

		/// <summary>
		/// Unsubscribe from the switcher events.
		/// </summary>
		/// <param name="switcher"></param>
		private void Unsubscribe(HdMd8x2 switcher)
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
