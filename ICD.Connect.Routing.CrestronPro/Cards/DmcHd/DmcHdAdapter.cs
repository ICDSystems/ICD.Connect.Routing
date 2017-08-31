using Crestron.SimplSharpPro;
using Crestron.SimplSharpPro.DM;

namespace ICD.Connect.Routing.CrestronPro.Cards.DmcHd
{
	public sealed class DmcHdAdapter : AbstractCardAdapter<Crestron.SimplSharpPro.DM.Cards.DmcHd, DmcHdAdapterSettings>
	{
		/// <summary>
		/// Instantiates an external card.
		/// </summary>
		/// <param name="cresnetId"></param>
		/// <param name="controlSystem"></param>
		/// <returns></returns>
		protected override Crestron.SimplSharpPro.DM.Cards.DmcHd InstantiateCardExternal(byte cresnetId,
		                                                                                 CrestronControlSystem controlSystem)
		{
			return new Crestron.SimplSharpPro.DM.Cards.DmcHd(cresnetId, controlSystem);
		}

		/// <summary>
		/// Instantiates an internal card.
		/// </summary>
		/// <param name="cardNumber"></param>
		/// <param name="switcher"></param>
		/// <returns></returns>
		protected override Crestron.SimplSharpPro.DM.Cards.DmcHd InstantiateCardInternal(uint cardNumber, Switch switcher)
		{
			return new Crestron.SimplSharpPro.DM.Cards.DmcHd(cardNumber, switcher);
		}
	}
}
