namespace ICD.Connect.Routing.CrestronPro.Cards.Inputs.Dmc4kHdDspBase
{
	public abstract class AbstractDmc4kHdDspBaseAdapter<TCard, TSettings> : AbstractInputCardAdapter<TCard, TSettings>
		where TCard : Crestron.SimplSharpPro.DM.Cards.Dmc4kHdDspBase
		where TSettings : IInputCardSettings, new()
	{
	}
}
