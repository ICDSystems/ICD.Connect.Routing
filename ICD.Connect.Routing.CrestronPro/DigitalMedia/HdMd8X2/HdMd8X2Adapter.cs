#if SIMPLSHARP
using Crestron.SimplSharpPro.DM;
using ICD.Connect.Misc.CrestronPro;
#else
using System;
#endif
using ICD.Connect.Settings.Core;

namespace ICD.Connect.Routing.CrestronPro.DigitalMedia.HdMd8X2
{
	public sealed class HdMd8X2Adapter : AbstractDmSwitcherAdapter<HdMd8x2, HdMd8X2AdapterSettings>
	{
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
	}
}
