using ICD.Connect.API.Nodes;
#if SIMPLSHARP
using Crestron.SimplSharpPro.DM;
using ICD.Connect.Misc.CrestronPro;
#else
using System;
#endif

namespace ICD.Connect.Routing.CrestronPro.DigitalMedia.HdMd4X24kE
{
	/// <summary>
	/// HdMd4X24kEAdapter wraps a HdMd4x24kE to provide a routing device.
	/// </summary>
#if SIMPLSHARP
	public sealed class HdMd4X24kEAdapter : AbstractCrestronSwitchAdapter<HdMd4x24kE, HdMd4X24kEAdapterSettings>
#else
	public sealed class HdMd4X24kEAdapter : AbstractCrestronSwitchAdapter<HdMd4X24kEAdapterSettings>
#endif
	{
		private string m_Address;

		/// <summary>
		/// Constructor.
		/// </summary>
		public HdMd4X24kEAdapter()
		{
#if SIMPLSHARP
			Controls.Add(new HdMd4X24kESwitcherControl(this));
#endif
		}

#if SIMPLSHARP
		/// <summary>
		/// Sets the wrapped switcher.
		/// </summary>
		/// <param name="switcher"></param>
		/// <param name="address"></param>
		public void SetSwitcher(HdMd4x24kE switcher, string address)
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
		protected override void CopySettingsFinal(HdMd4X24kEAdapterSettings settings)
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
		protected override void SetSwitcher(HdMd4X24kEAdapterSettings settings)
		{
			HdMd4x24kE switcher = InstantiateSwitcher(settings);
			SetSwitcher(switcher, settings.Address);
		}

		/// <summary>
		/// Creates a new instance of the wrapped internal switcher.
		/// </summary>
		/// <param name="settings"></param>
		/// <returns></returns>
		protected override HdMd4x24kE InstantiateSwitcher(HdMd4X24kEAdapterSettings settings)
		{
			return settings.Ipid == null 
				   ? null
				   : new HdMd4x24kE(settings.Ipid.Value, settings.Address, ProgramInfo.ControlSystem);
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
