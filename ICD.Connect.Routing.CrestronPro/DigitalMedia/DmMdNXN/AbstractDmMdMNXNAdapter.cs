using System.Collections.Generic;
using Crestron.SimplSharpPro;
using Crestron.SimplSharpPro.DM;
using ICD.Common.Properties;
using ICD.Common.Services.Logging;
using ICD.Connect.Devices;
using ICD.Connect.Misc.CrestronPro;
using ICD.Connect.Misc.CrestronPro.Devices;
using ICD.Connect.Settings.Core;

namespace ICD.Connect.Routing.CrestronPro.DigitalMedia.DmMdNXN
{
// ReSharper disable once InconsistentNaming
	public abstract class AbstractDmMdMNXNAdapter<TSwitcher, TSettings> : AbstractDevice<TSettings>, IDmMdMNXNAdapter, IDmParent
		where TSwitcher : DmMDMnxn
		where TSettings : IDmMdNXNAdapterSettings, new()
	{
		public event DmMdMNXNChangeCallback OnSwitcherChanged;

		private TSwitcher m_Switcher;

		#region Properties

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

		#endregion

		/// <summary>
		/// Constructor.
		/// </summary>
		protected AbstractDmMdMNXNAdapter()
		{
			Controls.Add(new DmMdMNXNSwitcherControl(this));
		}

		#region Methods

		/// <summary>
		/// Release resources.
		/// </summary>
		protected override void DisposeFinal(bool disposing)
		{
			OnSwitcherChanged = null;

			base.DisposeFinal(disposing);

			// Unsubscribe and unregister.
			SetSwitcher(null);
		}

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

		#endregion

		#region Settings

		/// <summary>
		/// Override to apply properties to the settings instance.
		/// </summary>
		/// <param name="settings"></param>
		protected override void CopySettingsFinal(TSettings settings)
		{
			base.CopySettingsFinal(settings);

			settings.Ipid = Switcher == null ? (byte)0 : (byte)Switcher.ID;
		}

		/// <summary>
		/// Override to clear the instance settings.
		/// </summary>
		protected override void ClearSettingsFinal()
		{
			base.ClearSettingsFinal();

			SetSwitcher(null);
		}

		/// <summary>
		/// Override to apply settings to the instance.
		/// </summary>
		/// <param name="settings"></param>
		/// <param name="factory"></param>
		protected override void ApplySettingsFinal(TSettings settings, IDeviceFactory factory)
		{
			base.ApplySettingsFinal(settings, factory);

			TSwitcher switcher = InstantiateSwitcher(settings.Ipid, ProgramInfo.ControlSystem);
			SetSwitcher(switcher);
		}

		/// <summary>
		/// Creates a new instance of the wrapped internal switcher.
		/// </summary>
		/// <param name="ipid"></param>
		/// <param name="controlSystem"></param>
		/// <returns></returns>
		protected abstract TSwitcher InstantiateSwitcher(ushort ipid, CrestronControlSystem controlSystem);

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

		#endregion
	}
}
