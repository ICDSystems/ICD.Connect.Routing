namespace ICD.Connect.Routing.CrestronPro.Cards.Inputs.Dmc4kCBase
{
#if SIMPLSHARP
	public abstract class AbstractDmc4kCBaseAdapter<TCard, TSettings> : AbstractInputCardAdapter<TCard, TSettings>
		where TCard : Crestron.SimplSharpPro.DM.Cards.Dmc4kCBase
#else
	public abstract class AbstractDmc4kCBaseAdapter<TSettings> : AbstractInputCardAdapter<TSettings>
#endif
		where TSettings : IInputCardSettings, new()
	{
	}
}
