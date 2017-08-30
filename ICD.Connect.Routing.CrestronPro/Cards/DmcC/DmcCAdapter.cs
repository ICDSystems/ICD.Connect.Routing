using Crestron.SimplSharpPro;
using Crestron.SimplSharpPro.DM;

namespace ICD.Connect.Routing.CrestronPro.Cards.DmcC
{
	public sealed class DmcCAdapter : AbstractCardAdapter<Crestron.SimplSharpPro.DM.Cards.DmcHd, DmcCAdapterSettings>
	{
		/// <summary>
		/// Instantiates an external card.
		/// </summary>
		/// <param name="cresnetId"></param>
		/// <param name="controlSystem"></param>
		/// <returns></returns>
		protected override Crestron.SimplSharpPro.DM.Cards.DmcHd InstantiateCard(uint cresnetId, CrestronControlSystem controlSystem)
		{
			return new Crestron.SimplSharpPro.DM.Cards.DmcHd(cresnetId, controlSystem);
		}

		/// <summary>
		/// Instantiates an internal card.
		/// </summary>
		/// <param name="cardNumber"></param>
		/// <param name="switcher"></param>
		/// <returns></returns>
		protected override Crestron.SimplSharpPro.DM.Cards.DmcHd InstantiateCard(uint cardNumber, Switch switcher)
		{
			return new Crestron.SimplSharpPro.DM.Cards.DmcHd(cardNumber, switcher);
		}
	}
}