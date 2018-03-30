using System;
using ICD.Common.Properties;
using ICD.Common.Utils;
using ICD.Common.Utils.Services.Logging;
using ICD.Connect.API.Nodes;
using ICD.Connect.Devices;
using ICD.Connect.Settings.Core;
#if SIMPLSHARP
using Crestron.SimplSharpPro;
using Crestron.SimplSharpPro.DM;
#endif

namespace ICD.Connect.Routing.CrestronPro.DigitalMedia
{
#if SIMPLSHARP
	public abstract class AbstractCrestronSwitchAdapter<TSwitcher, TSettings> : AbstractDevice<TSettings>, ICrestronSwitchAdapter
		where TSwitcher : Switch
#else
	public abstract class AbstractCrestronSwitchAdapter<TSettings> : AbstractDevice<TSettings>, ICrestronSwitchAdapter
#endif
		where TSettings : ICrestronSwitchAdapterSettings, new()
	{
#if SIMPLSHARP
		public event DmSwitcherChangeCallback OnSwitcherChanged;

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

				DmSwitcherChangeCallback handler = OnSwitcherChanged;
				if (handler != null)
					handler(this, m_Switcher);
			}
		}

		/// <summary>
		/// Gets the wrapped switch instance.
		/// </summary>
		Switch ICrestronSwitchAdapter.Switcher { get { return Switcher; } }
#endif

		#endregion

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

		/// <summary>
		/// Gets the current online status of the device.
		/// </summary>
		/// <returns></returns>
		protected override bool GetIsOnlineStatus()
		{
#if SIMPLSHARP
			return m_Switcher != null && m_Switcher.IsOnline;
#else
			return false;
#endif
		}

		#region Methods

#if SIMPLSHARP
		/// <summary>
		/// Sets the wrapped switcher.
		/// </summary>
		/// <param name="switcher"></param>
		[PublicAPI]
		protected void SetSwitcher(TSwitcher switcher)
		{
			Unsubscribe(Switcher);
			Unregister(Switcher);

			Switcher = switcher;

			Register(Switcher);
			Subscribe(Switcher);

			ConfigureSwitcher(Switcher);

			UpdateCachedOnlineStatus();
		}

		/// <summary>
		/// Unregisters the given switcher.
		/// </summary>
		/// <param name="switcher"></param>
		private void Unregister(TSwitcher switcher)
		{
			if (switcher == null || !switcher.Registered)
				return;

			switcher.UnRegister();

			try
			{
				switcher.Dispose();
			}
			catch
			{
			}
		}

		/// <summary>
		/// Registers the given switcher.
		/// </summary>
		/// <param name="switcher"></param>
		private void Register(TSwitcher switcher)
		{
			if (switcher == null || switcher.Registered)
				return;

			if (Name != null)
				switcher.Description = Name;

			eDeviceRegistrationUnRegistrationResponse result = switcher.Register();
			if (result != eDeviceRegistrationUnRegistrationResponse.Success)
				Logger.AddEntry(eSeverity.Error, "{0} unable to register {1} - {2}", this, switcher.GetType().Name, result);
		}

		/// <summary>
		/// Override to control how the assigned switcher behaves.
		/// </summary>
		/// <param name="switcher"></param>
		protected virtual void ConfigureSwitcher(TSwitcher switcher)
		{
			if (switcher == null)
				return;
			if (switcher.EnableUSBBreakaway.Supported)
				switcher.EnableUSBBreakaway.BoolValue = true;
			if (switcher.VideoEnter.Supported)
				switcher.VideoEnter.BoolValue = true;
			if (switcher.USBEnter.Supported)
				switcher.USBEnter.BoolValue = true;
		}

		/// <summary>
		/// Gets the DMInput at the given address.
		/// </summary>
		/// <param name="address"></param>
		/// <returns></returns>
		public DMInput GetDmInput(int address)
		{
			if (address < 0 || !m_Switcher.Inputs.Contains((uint)address))
				throw new IndexOutOfRangeException(string.Format("{0} has no input at address {1}", this, address));

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
				throw new IndexOutOfRangeException(string.Format("{0} has no output at address {1}", this, address));

			return m_Switcher.Outputs[(uint)address];
		}
#endif

		#endregion

		#region Switcher Callbacks

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
		/// <param name="genericBase"></param>
		/// <param name="args"></param>
		private void SwitcherOnlineStatusChange(GenericBase genericBase, OnlineOfflineEventArgs args)
		{
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

			SetSwitcher(settings);
#endif
		}

#if SIMPLSHARP
		/// <summary>
		/// Override to control how the switcher is assigned from settings.
		/// </summary>
		/// <param name="settings"></param>
		protected virtual void SetSwitcher(TSettings settings)
		{
			TSwitcher switcher = InstantiateSwitcher(settings);
			SetSwitcher(switcher);
		}

		/// <summary>
		/// Creates a new instance of the wrapped internal switcher.
		/// </summary>
		/// <param name="settings"></param>
		/// <returns></returns>
		protected abstract TSwitcher InstantiateSwitcher(TSettings settings);
#endif

		#endregion

		#region Console

		/// <summary>
		/// Calls the delegate for each console status item.
		/// </summary>
		/// <param name="addRow"></param>
		public override void BuildConsoleStatus(AddStatusRowDelegate addRow)
		{
			base.BuildConsoleStatus(addRow);

#if SIMPLSHARP
			addRow("IPID", Switcher == null ? null : StringUtils.ToIpIdString((byte)Switcher.ID));
#endif
		}

		#endregion
	}
}
