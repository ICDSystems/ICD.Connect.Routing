using System.Collections.Generic;
using Crestron.SimplSharpPro;
using Crestron.SimplSharpPro.DM;
using ICD.Common.Properties;
using ICD.Common.Services.Logging;
using ICD.Connect.Devices;

namespace ICD.Connect.Routing.CrestronPro.DigitalMedia
{
	public abstract class AbstractDmSwitcherAdapter<TSwitcher, TSettings> : AbstractDevice<TSettings>, IDmSwitcherAdapter
		where TSwitcher : Switch
		where TSettings : IDeviceSettings, new()
	{
		public event DmSwitcherChangeCallback OnSwitcherChanged;

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

				DmSwitcherChangeCallback handler = OnSwitcherChanged;
				if (handler != null)
					handler(this, m_Switcher);
			}
		}

		/// <summary>
		/// Gets the wrapped switch instance.
		/// </summary>
		Switch IDmSwitcherAdapter.Switcher { get { return Switcher; } }

		#endregion

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
		/// Gets the current online status of the device.
		/// </summary>
		/// <returns></returns>
		protected override bool GetIsOnlineStatus()
		{
			return m_Switcher != null && m_Switcher.IsOnline;
		}

		#region Methods

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
			}

			Subscribe(Switcher);

			UpdateCachedOnlineStatus();
		}

		/// <summary>
		/// Override to control how the assigned switcher behaves.
		/// </summary>
		/// <param name="switcher"></param>
		protected virtual void ConfigureSwitcher(TSwitcher switcher)
		{
			switcher.EnableUSBBreakaway.BoolValue = true;
			switcher.VideoEnter.BoolValue = true;
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

		#region Switcher Callbacks

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

		#endregion
	}
}
