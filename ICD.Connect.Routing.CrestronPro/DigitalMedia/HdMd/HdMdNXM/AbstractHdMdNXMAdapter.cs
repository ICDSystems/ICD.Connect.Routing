using System;
using ICD.Connect.API.Nodes;
using ICD.Connect.Devices.Controls;
using ICD.Connect.Settings;
#if !NETSTANDARD
using Crestron.SimplSharpPro.DM;
#endif

namespace ICD.Connect.Routing.CrestronPro.DigitalMedia.HdMd.HdMdNXM
{
#if !NETSTANDARD
	public abstract class AbstractHdMdNXMAdapter<TSwitcher, TSettings> : AbstractCrestronSwitchAdapter<TSwitcher, TSettings>, IHdMdNXMAdapter
		where TSwitcher : HdMdNxM
#else
	public abstract class AbstractHdMdNXMAdapter<TSettings> : AbstractCrestronSwitchAdapter<TSettings>
#endif
		where TSettings : IHdMdNXMAdapterSettings, new()
	{
		private string m_Address;

#if !NETSTANDARD
		HdMdNxM IHdMdNXMAdapter.Switcher { get { return Switcher; } }

		/// <summary>
		/// Sets the wrapped switcher.
		/// </summary>
		/// <param name="switcher"></param>
		/// <param name="address"></param>
		public void SetSwitcher(TSwitcher switcher, string address)
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
			TSwitcher switcher = InstantiateSwitcher(settings);
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

			addControl(new HdMdNXMSwitcherControl(this));
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
