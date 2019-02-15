namespace ICD.Connect.Routing.CrestronPro.Cards.Inputs.Dmc4kHdDspBase
{
#if SIMPLSHARP
	public abstract class AbstractDmc4kHdDspBaseAdapter<TCard, TSettings> : AbstractInputCardAdapter<TCard, TSettings>
		where TCard : Crestron.SimplSharpPro.DM.Cards.Dmc4kHdDspBase
#else
	public abstract class AbstractDmc4kHdDspBaseAdapter<TSettings> : AbstractInputCardAdapter<TSettings>
#endif
		where TSettings : IInputCardSettings, new()
	{
	}
}
