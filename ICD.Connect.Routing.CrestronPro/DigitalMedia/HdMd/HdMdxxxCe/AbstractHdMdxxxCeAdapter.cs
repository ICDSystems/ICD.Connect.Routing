using System;
using ICD.Connect.API.Nodes;
using ICD.Connect.Devices.Controls;
using ICD.Connect.Settings;
#if !NETSTANDARD
using Crestron.SimplSharpPro.DM;
#endif

namespace ICD.Connect.Routing.CrestronPro.DigitalMedia.HdMd.HdMdxxxCe
{
#if !NETSTANDARD
	public abstract class AbstractHdMdxxxCeAdapter<TSwitch, TSettings> : AbstractCrestronSwitchAdapter<TSwitch, TSettings>, IHdMdxxxCeAdapter
		where TSwitch : HdMdxxxCE
#else
	public abstract class AbstractHdMdxxxCeAdapter<TSettings> : AbstractCrestronSwitchAdapter<TSettings>, IHdMdxxxCeAdapter
#endif
		where TSettings : IHdMdxxxCeAdapterSettings, new()
	{
		private string m_Address;

#if !NETSTANDARD
		HdMdxxxCE IHdMdxxxCeAdapter.Switcher { get { return Switcher; } }

		/// <summary>
		/// Sets the wrapped switcher.
		/// </summary>
		/// <param name="switcher"></param>
		/// <param name="address"></param>
		public void SetSwitcher(TSwitch switcher, string address)
		{
			m_Address = address;
			SetSwitcher(switcher);
		}
#endif

		#region Settings

		/// <summary>
		/// Override to apply properties to the settings instance.
		/// </summary>
		/// <param name="settings"></param>
		protected override void CopySettingsFinal(TSettings settings)
		{
			base.CopySettingsFinal(settings);

			settings.Address = m_Address;
		}

		/// <summary>
		/// Override to clear the instance settings.
		/// </summary>
		protected override void ClearSettingsFinal()
		{
			base.ClearSettingsFinal();

			m_Address = null;
		}

#if !NETSTANDARD
		/// <summary>
		/// Override to control how the switcher is assigned from settings.
		/// </summary>
		/// <param name="settings"></param>
		protected override void SetSwitcher(TSettings settings)
		{
			TSwitch switcher = InstantiateSwitcher(settings);
			SetSwitcher(switcher, settings.Address);
		}

		/// <summary>
		/// Override to add controls to the device.
		/// </summary>
		/// <param name="settings"></param>
		/// <param name="factory"></param>
		/// <param name="addControl"></param>
		protected override void AddControls(TSettings settings, IDeviceFactory factory, Action<IDeviceControl> addControl)
		{
			base.AddControls(settings, factory, addControl);

			addControl(new HdMdxxxCeSwitcherControl(this));
		}
#endif

		#endregion

		/// <summary>
		/// Calls the delegate for each console status item.
		/// </summary>
		/// <param name="addRow"></param>
		public override void BuildConsoleStatus(AddStatusRowDelegate addRow)
		{
			base.BuildConsoleStatus(addRow);

			addRow("Address", m_Address);
		}
	}
}
