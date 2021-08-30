#if !NETSTANDARD
using Crestron.SimplSharpPro.DM.Cards;
#endif

namespace ICD.Connect.Routing.CrestronPro.Cards.Outputs.Dmc4kCoHdBase
{
	// ReSharper disable once InconsistentNaming
#if !NETSTANDARD
	public abstract class AbstractDmc4kCoHdBaseAdapter<TDevice, TSettings> : AbstractOutputCardAdapter<TDevice, TSettings>
		where TDevice : Dmc4kCoHdSingleBase
#else
	public abstract class AbstractDmc4kCoHdBaseAdapter<TSettings> : AbstractOutputCardAdapter<TSettings>
#endif
		where TSettings : IOutputCardSettings, new()
	{
	}
}
