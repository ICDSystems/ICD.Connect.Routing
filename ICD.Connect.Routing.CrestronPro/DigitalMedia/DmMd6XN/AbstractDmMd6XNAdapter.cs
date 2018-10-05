namespace ICD.Connect.Routing.CrestronPro.DigitalMedia.DmMd6XN
{
#if SIMPLSHARP
	public abstract class AbstractDmMd6XNAdapter<TSwitcher, TSettings> : AbstractCrestronSwitchAdapter<TSwitcher, TSettings>, IDmMd6XNAdapter
		where TSwitcher : Crestron.SimplSharpPro.DM.DmMd6XN
#else
	public abstract class AbstractDmMd6XNAdapter<TSettings> : AbstractCrestronSwitchAdapter<TSettings>, IDmMd6XNAdapter
#endif
		where TSettings : IDmMd6XNAdapterSettings, new()
	{
		#if SIMPLSHARP
		Crestron.SimplSharpPro.DM.DmMd6XN IDmMd6XNAdapter.Switcher { get { return Switcher; } }
#endif

		/// <summary>
		/// Constructor.
		/// </summary>
		protected AbstractDmMd6XNAdapter()
		{
#if SIMPLSHARP
			Controls.Add(new DmMd6XNSwitcherControl(this));
#endif
		}

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
	}
}
