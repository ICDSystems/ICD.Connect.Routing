namespace ICD.Connect.Routing.CrestronPro.Cards.Inputs.Dmc4kCDspBase
{
#if !NETSTANDARD
	public abstract class AbstractDmc4kCDspBaseAdapter<TCard, TSettings> : AbstractInputCardAdapter<TCard, TSettings>
		where TCard : Crestron.SimplSharpPro.DM.Cards.Dmc4kCDspBase
#else
	public abstract class AbstractDmc4kCDspBaseAdapter<TSettings> : AbstractInputCardAdapter<TSettings>
#endif
		where TSettings : IInputCardSettings, new()
	{
	}
}
