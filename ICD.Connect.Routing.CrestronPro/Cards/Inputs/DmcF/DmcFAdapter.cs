#if SIMPLSHARP
using Crestron.SimplSharpPro;
using Crestron.SimplSharpPro.DM;
using ICD.Connect.Misc.CrestronPro.Extensions;
#endif

namespace ICD.Connect.Routing.CrestronPro.Cards.Inputs.DmcF
{
#if SIMPLSHARP
	public sealed class DmcFAdapter : AbstractInputCardAdapter<Crestron.SimplSharpPro.DM.Cards.DmcF, DmcFAdapterSettings>
	{
		/// <summary>
		/// Constructor.
		/// </summary>
		public DmcFAdapter()
		{
			Controls.Add(new DmcFAdapterRoutingControl(this, 0));
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
		protected override Crestron.SimplSharpPro.DM.Cards.DmcF InstantiateCardExternal(byte cresnetId,
		                                                                                CrestronControlSystem controlSystem)
		{
			return new Crestron.SimplSharpPro.DM.Cards.DmcF(cresnetId, controlSystem);
		}

		/// <summary>
		/// Instantiates an internal card.
		/// </summary>
		/// <param name="cardNumber"></param>
		/// <param name="switcher"></param>
		/// <returns></returns>
		protected override Crestron.SimplSharpPro.DM.Cards.DmcF InstantiateCardInternal(uint cardNumber, Switch switcher)
		{
			return new Crestron.SimplSharpPro.DM.Cards.DmcF(cardNumber, switcher);
		}
	}
#else
	public sealed class DmcFAdapter : AbstractInputCardAdapter<DmcFAdapterSettings>
	{
	}
#endif
}
