#if SIMPLSHARP
using Crestron.SimplSharpPro.DM;
#else
using System;
#endif
using ICD.Connect.Misc.CrestronPro;
using ICD.Connect.Settings.Core;

namespace ICD.Connect.Routing.CrestronPro.DigitalMedia.HdMd4X24kE
{
	/// <summary>
	/// HdMd4X24kEAdapter wraps a HdMd4x24kE to provide a routing device.
	/// </summary>
	public sealed class HdMd4X24kEAdapter : AbstractDmSwitcherAdapter<HdMd4x24kE, HdMd4X24kEAdapterSettings>
	{
		private string m_Address;

#region Constructors

		/// <summary>
		/// Constructor.
		/// </summary>
		public HdMd4X24kEAdapter()
		{
#if SIMPLSHARP
            Controls.Add(new HdMd4X24kESwitcherControl(this));
#endif
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

#if SIMPLSHARP
            settings.Address = m_Address;
			settings.Ipid = Switcher == null ? (byte)0 : (byte)Switcher.ID;
#else
            settings.Address = m_Address;
            settings.Ipid = 0;
#endif
        }

		/// <summary>
		/// Override to clear the instance settings.
		/// </summary>
		protected override void ClearSettingsFinal()
		{
			base.ClearSettingsFinal();

			m_Address = null;
#if SIMPLSHARP
            SetSwitcher(null, null);
#endif
		}

		/// <summary>
		/// Override to apply settings to the instance.
		/// </summary>
		/// <param name="settings"></param>
		/// <param name="factory"></param>
		protected override void ApplySettingsFinal(HdMd4X24kEAdapterSettings settings, IDeviceFactory factory)
		{
			base.ApplySettingsFinal(settings, factory);

#if SIMPLSHARP
            HdMd4x24kE switcher = new HdMd4x24kE(settings.Ipid, settings.Address, ProgramInfo.ControlSystem);
			SetSwitcher(switcher, settings.Address);
#else
            throw new NotImplementedException();
#endif
        }

#endregion
	}
}
