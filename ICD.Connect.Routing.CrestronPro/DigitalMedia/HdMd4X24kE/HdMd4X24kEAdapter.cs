﻿using Crestron.SimplSharpPro;
using Crestron.SimplSharpPro.DM;
using ICD.Common.Properties;
using ICD.Common.Services.Logging;
using ICD.Connect.Devices;
using ICD.Connect.Misc.CrestronPro;
using ICD.Connect.Settings.Core;

namespace ICD.Connect.Routing.CrestronPro.DigitalMedia.HdMd4X24kE
{
	/// <summary>
	/// HdMd4X24kEAdapter wraps a HdMd4x24kE to provide a routing device.
	/// </summary>
	public sealed class HdMd4X24kEAdapter : AbstractDevice<HdMd4X24kEAdapterSettings>
	{
		public delegate void SwitcherChangeCallback(HdMd4X24kEAdapter sender, HdMd4x24kE switcher);

		/// <summary>
		/// Raised when the wrapped switcher changes.
		/// </summary>
		public event SwitcherChangeCallback OnSwitcherChanged;

		private HdMd4x24kE m_Switcher;
		private string m_Address;

		#region Properties

		/// <summary>
		/// Gets the wrapped switcher.
		/// </summary>
		public HdMd4x24kE Switcher
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

		#endregion

		#region Constructors

		/// <summary>
		/// Constructor.
		/// </summary>
		public HdMd4X24kEAdapter()
		{
			Controls.Add(new HdMd4X24kESwitcherControl(this));
		}

		#endregion

		#region Methods

		/// <summary>
		/// Release resources.
		/// </summary>
		protected override void DisposeFinal(bool disposing)
		{
			base.DisposeFinal(disposing);

			// Unsubscribe and unregister.
			SetSwitcher(null, null);
		}

		/// <summary>
		/// Sets the wrapped switcher.
		/// </summary>
		/// <param name="switcher"></param>
		/// <param name="address"></param>
		[PublicAPI]
		public void SetSwitcher(HdMd4x24kE switcher, string address)
		{
			m_Address = address;

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

		#endregion

		#region Settings

		/// <summary>
		/// Override to apply properties to the settings instance.
		/// </summary>
		/// <param name="settings"></param>
		protected override void CopySettingsFinal(HdMd4X24kEAdapterSettings settings)
		{
			base.CopySettingsFinal(settings);

			settings.Address = m_Address;
			settings.Ipid = Switcher == null ? (byte)0 : (byte)Switcher.ID;
		}

		/// <summary>
		/// Override to clear the instance settings.
		/// </summary>
		protected override void ClearSettingsFinal()
		{
			base.ClearSettingsFinal();

			m_Address = null;
			SetSwitcher(null, null);
		}

		/// <summary>
		/// Override to apply settings to the instance.
		/// </summary>
		/// <param name="settings"></param>
		/// <param name="factory"></param>
		protected override void ApplySettingsFinal(HdMd4X24kEAdapterSettings settings, IDeviceFactory factory)
		{
			base.ApplySettingsFinal(settings, factory);

			HdMd4x24kE switcher = new HdMd4x24kE(settings.Ipid, settings.Address, ProgramInfo.ControlSystem);
			SetSwitcher(switcher, settings.Address);
		}

		#endregion

		#region Private Methods

		/// <summary>
		/// Gets the current online status of the device.
		/// </summary>
		/// <returns></returns>
		protected override bool GetIsOnlineStatus()
		{
			return Switcher != null && Switcher.IsOnline;
		}

		/// <summary>
		/// Subscribe to the switcher events.
		/// </summary>
		/// <param name="switcher"></param>
		private void Subscribe(HdMd4x24kE switcher)
		{
			if (switcher == null)
				return;

			switcher.OnlineStatusChange += SwitcherOnlineStatusChange;
		}

		/// <summary>
		/// Unsubscribe from the switcher events.
		/// </summary>
		/// <param name="switcher"></param>
		private void Unsubscribe(HdMd4x24kE switcher)
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

		#endregion
	}
}
