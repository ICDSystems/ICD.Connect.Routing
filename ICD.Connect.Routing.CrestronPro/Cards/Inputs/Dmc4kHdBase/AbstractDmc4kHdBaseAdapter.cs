namespace ICD.Connect.Routing.CrestronPro.Cards.Inputs.Dmc4kHdBase
{
#if !NETSTANDARD
	public abstract class AbstractDmc4kHdBaseAdapter<TCard, TSettings> : AbstractInputCardAdapter<TCard, TSettings>
		where TCard : Crestron.SimplSharpPro.DM.Cards.Dmc4kHdBase
#else
	public abstract class AbstractDmc4kHdBaseAdapter<TSettings> : AbstractInputCardAdapter<TSettings>
#endif
		where TSettings : IInputCardSettings, new()
	{
	}
}
