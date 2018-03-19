#if SIMPLSHARP
using Crestron.SimplSharpPro.DM;
#endif

namespace ICD.Connect.Routing.CrestronPro.DigitalMedia.HdMd8XN
{
#if SIMPLSHARP
	public abstract class AbstractHdMd8XNAdapter<TSwitch, TSettings> : AbstractCrestronSwitchAdapter<TSwitch, TSettings>, IHdMd8XNAdapter
		where TSwitch : HdMd8xN
#else
	public abstract class AbstractHdMd8XNAdapter<TSettings> : AbstractCrestronSwitchAdapter<TSettings>, IHdMd8XNAdapter
#endif
		where TSettings : IHdMd8XNAdapterSettings, new()
	{

#if SIMPLSHARP
		HdMd8xN IHdMd8XNAdapter.Switcher { get { return Switcher; } }
#endif

		/// <summary>
		/// Constructor.
		/// </summary>
		protected AbstractHdMd8XNAdapter()
		{
#if SIMPLSHARP
			Controls.Add(new HdMd8XNSwitcherControl(this));
#endif
		}
	}
}
