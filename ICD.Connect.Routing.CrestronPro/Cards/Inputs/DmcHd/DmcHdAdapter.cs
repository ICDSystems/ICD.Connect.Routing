#if SIMPLSHARP
using Crestron.SimplSharpPro;
using Crestron.SimplSharpPro.DM;
using ICD.Connect.Misc.CrestronPro.Extensions;
#endif

namespace ICD.Connect.Routing.CrestronPro.Cards.Inputs.DmcHd
{
#if SIMPLSHARP
	public sealed class DmcHdAdapter :
		AbstractInputCardAdapter<Crestron.SimplSharpPro.DM.Cards.DmcHd, DmcHdAdapterSettings>
	{
		/// <summary>
		/// Constructor.
		/// </summary>
		public DmcHdAdapter()
		{
			Controls.Add(new DmcHdAdapterRoutingControl(this, 0));
		}

		protected override bool GetIsOnlineStatus()
		{
			return true;
			//TODO: Crestron api broken, re enable this line when a resolution comes back from them
			return Card != null && Card.PresentFeedback.GetBoolValueOrDefault();
		}

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
#else
	public sealed class DmcHdAdapter : AbstractInputCardAdapter<DmcHdAdapterSettings>
	{
	}
#endif
}
