#if SIMPLSHARP
using Crestron.SimplSharpPro;
using Crestron.SimplSharpPro.DM;
using ICD.Connect.Misc.CrestronPro.Extensions;
#endif

namespace ICD.Connect.Routing.CrestronPro.Cards.Inputs.DmcCatDsp
{
#if SIMPLSHARP
	public sealed class DmcCatDspAdapter : AbstractInputCardAdapter<Crestron.SimplSharpPro.DM.Cards.DmcCatDsp, DmcCatDspAdapterSettings>
	{
		/// <summary>
		/// Constructor.
		/// </summary>
		public DmcCatDspAdapter()
		{
			Controls.Add(new DmcCatDspAdapterRoutingControl(this, 0));
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
		protected override Crestron.SimplSharpPro.DM.Cards.DmcCatDsp InstantiateCardExternal(byte cresnetId,
		                                                                                CrestronControlSystem controlSystem)
		{
			return new Crestron.SimplSharpPro.DM.Cards.DmcCatDsp(cresnetId, controlSystem);
		}

		/// <summary>
		/// Instantiates an internal card.
		/// </summary>
		/// <param name="cardNumber"></param>
		/// <param name="switcher"></param>
		/// <returns></returns>
		protected override Crestron.SimplSharpPro.DM.Cards.DmcCatDsp InstantiateCardInternal(uint cardNumber, Switch switcher)
		{
			return new Crestron.SimplSharpPro.DM.Cards.DmcCatDsp(cardNumber, switcher);
		}
	}
#else
	public sealed class DmcCatDspAdapter : AbstractInputCardAdapter<DmcCatDspAdapterSettings>
	{
	}
#endif
}