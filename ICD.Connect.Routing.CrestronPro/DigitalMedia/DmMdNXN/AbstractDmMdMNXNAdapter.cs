#if SIMPLSHARP
using Crestron.SimplSharpPro;
using Crestron.SimplSharpPro.DM;
using ICD.Connect.Misc.CrestronPro;
#else
using System;
#endif
using ICD.Connect.Settings.Core;

namespace ICD.Connect.Routing.CrestronPro.DigitalMedia.DmMdNXN
{
#if SIMPLSHARP
// ReSharper disable once InconsistentNaming
    public abstract class AbstractDmMdMNXNAdapter<TSwitcher, TSettings> : AbstractDmSwitcherAdapter<TSwitcher, TSettings>, IDmMdMNXNAdapter
		where TSwitcher : DmMDMnxn
#else
    public abstract class AbstractDmMdMNXNAdapter<TSettings> : AbstractDmSwitcherAdapter<TSettings>
#endif
		where TSettings : IDmMdNXNAdapterSettings, new()
	{
        #region Properties

#if SIMPLSHARP
		DmMDMnxn IDmMdMNXNAdapter.Switcher { get { return Switcher; } }
#endif

        #endregion

        /// <summary>
        /// Constructor.
        /// </summary>
        protected AbstractDmMdMNXNAdapter()
		{
#if SIMPLSHARP
            Controls.Add(new DmMdMNXNSwitcherControl(this));
#endif
		}

#region Methods

#if SIMPLSHARP

	    /// <summary>
	    /// Override to control how the assigned switcher behaves.
	    /// </summary>
	    /// <param name="switcher"></param>
	    protected override void ConfigureSwitcher(TSwitcher switcher)
	    {
		    base.ConfigureSwitcher(switcher);

			switcher.EnableAudioBreakaway.BoolValue = true;
			switcher.AudioEnter.BoolValue = true;
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
            TSwitcher switcher = InstantiateSwitcher(settings.Ipid, ProgramInfo.ControlSystem);
			SetSwitcher(switcher);
#else
            throw new NotImplementedException();
#endif
        }

#if SIMPLSHARP
        /// <summary>
        /// Creates a new instance of the wrapped internal switcher.
        /// </summary>
        /// <param name="ipid"></param>
        /// <param name="controlSystem"></param>
        /// <returns></returns>
        protected abstract TSwitcher InstantiateSwitcher(ushort ipid, CrestronControlSystem controlSystem);
#endif

#endregion
	}
}
