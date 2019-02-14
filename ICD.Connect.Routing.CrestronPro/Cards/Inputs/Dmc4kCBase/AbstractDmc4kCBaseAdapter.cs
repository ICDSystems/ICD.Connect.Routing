namespace ICD.Connect.Routing.CrestronPro.Cards.Inputs.Dmc4kCBase
{
	public abstract class AbstractDmc4kCBaseAdapter<TCard, TSettings> : AbstractInputCardAdapter<TCard, TSettings>
		where TCard : Crestron.SimplSharpPro.DM.Cards.Dmc4kCBase
		where TSettings : IInputCardSettings, new()
	{
	}
}
