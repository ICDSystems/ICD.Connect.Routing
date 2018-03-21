#if SIMPLSHARP
using Crestron.SimplSharpPro.DM;
#else
using System;
#endif

namespace ICD.Connect.Routing.CrestronPro.DigitalMedia.DmMdMNXN
{
#if SIMPLSHARP
// ReSharper disable once InconsistentNaming
	public abstract class AbstractDmMdMNXNAdapter<TSwitcher, TSettings> : AbstractCrestronSwitchAdapter<TSwitcher, TSettings>,
	                                                                      IDmMdMNXNAdapter
		where TSwitcher : DmMDMnxn
#else
    public abstract class AbstractDmMdMNXNAdapter<TSettings> : AbstractCrestronSwitchAdapter<TSettings>, IDmMdMNXNAdapter
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

			if (switcher == null)
				return;

			switcher.EnableAudioBreakaway.BoolValue = true;
			switcher.AudioEnter.BoolValue = true;
		}

#endif

		#endregion
	}
}
