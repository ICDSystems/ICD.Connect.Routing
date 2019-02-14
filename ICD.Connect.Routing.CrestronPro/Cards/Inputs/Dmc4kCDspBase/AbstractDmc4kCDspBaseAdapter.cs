namespace ICD.Connect.Routing.CrestronPro.Cards.Inputs.Dmc4kCDspBase
{
	public abstract class AbstractDmc4kCDspBaseAdapter<TCard, TSettings> : AbstractInputCardAdapter<TCard, TSettings>
		where TCard : Crestron.SimplSharpPro.DM.Cards.Dmc4kCDspBase
		where TSettings : IInputCardSettings, new()
	{
	}
}
