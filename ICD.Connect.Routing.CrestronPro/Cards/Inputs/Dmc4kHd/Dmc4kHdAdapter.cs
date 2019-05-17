using ICD.Connect.Routing.CrestronPro.Cards.Inputs.Dmc4kHdBase;
#if SIMPLSHARP
using Crestron.SimplSharpPro;
using Crestron.SimplSharpPro.DM;
using ICD.Connect.Misc.CrestronPro.Extensions;
#endif

namespace ICD.Connect.Routing.CrestronPro.Cards.Inputs.Dmc4kHd
{
#if SIMPLSHARP
	public sealed class Dmc4kHdAdapter :
		AbstractDmc4kHdBaseAdapter<Crestron.SimplSharpPro.DM.Cards.Dmc4kHd, Dmc4kHdAdapterSettings>
	{
		/// <summary>
		/// Constructor.
		/// </summary>
		public Dmc4kHdAdapter()
		{
			Controls.Add(new Dmc4kHdAdapterRoutingControl(this, 0));
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
		protected override Crestron.SimplSharpPro.DM.Cards.Dmc4kHd InstantiateCardExternal(byte cresnetId,
		                                                                                  CrestronControlSystem controlSystem)
		{
			return new Crestron.SimplSharpPro.DM.Cards.Dmc4kHd(cresnetId, controlSystem);
		}

		/// <summary>
		/// Instantiates an internal card.
		/// </summary>
		/// <param name="cardNumber"></param>
		/// <param name="switcher"></param>
		/// <returns></returns>
		protected override Crestron.SimplSharpPro.DM.Cards.Dmc4kHd InstantiateCardInternal(uint cardNumber, Switch switcher)
		{
			return new Crestron.SimplSharpPro.DM.Cards.Dmc4kHd(cardNumber, switcher);
		}
	}
#else
	public sealed class Dmc4kHdAdapter : AbstractDmc4kHdBaseAdapter<Dmc4kHdAdapterSettings>
	{
	}
#endif
}
