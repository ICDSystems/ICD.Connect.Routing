using ICD.Connect.API.Nodes;
#if SIMPLSHARP
using Crestron.SimplSharpPro.DM;
using ICD.Connect.Misc.CrestronPro;
#else
using System;
#endif

namespace ICD.Connect.Routing.CrestronPro.DigitalMedia.HdMdNXM
{
#if SIMPLSHARP
	public abstract class AbstractHdMdNXMAdapter<TSwitcher, TSettings> : AbstractCrestronSwitchAdapter<TSwitcher, TSettings>, IHdMdNXMAdapter
		where TSwitcher : HdMdNxM
#else
	public abstract class AbstractHdMdNXMAdapter<TSettings> : AbstractCrestronSwitchAdapter<TSettings>
#endif
		where TSettings : IHdMdNXMAdapterSettings, new()
	{
		private string m_Address;

#if SIMPLSHARP
		HdMdNxM IHdMdNXMAdapter.Switcher { get { return Switcher; } }
#endif

		/// <summary>
		/// Constructor.
		/// </summary>
		protected AbstractHdMdNXMAdapter()
		{
#if SIMPLSHARP
			Controls.Add(new HdMdNXMSwitcherControl(this));
#endif
		}

#if SIMPLSHARP
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

#if SIMPLSHARP
		/// <summary>
		/// Override to control how the switcher is assigned from settings.
		/// </summary>
		/// <param name="settings"></param>
		protected override void SetSwitcher(TSettings settings)
		{
			TSwitcher switcher = InstantiateSwitcher(settings);
			SetSwitcher(switcher, settings.Address);
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
