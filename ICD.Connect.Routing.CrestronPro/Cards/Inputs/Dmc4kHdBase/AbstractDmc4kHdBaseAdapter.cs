namespace ICD.Connect.Routing.CrestronPro.Cards.Inputs.Dmc4kHdBase
{
	public abstract class AbstractDmc4kHdBaseAdapter<TCard, TSettings> : AbstractInputCardAdapter<TCard, TSettings>
		where TCard : Crestron.SimplSharpPro.DM.Cards.Dmc4kHdBase
		where TSettings : IInputCardSettings, new()
	{
	}
}
