using ICD.Connect.Routing.CrestronPro.Cards.Inputs.Dmc4kHdBase;
#if SIMPLSHARP
using Crestron.SimplSharpPro;
using Crestron.SimplSharpPro.DM;
#endif

namespace ICD.Connect.Routing.CrestronPro.Cards.Inputs.Dmc4kzHd
{
#if SIMPLSHARP
	public sealed class Dmc4kzHdAdapter :
		AbstractDmc4kHdBaseAdapter<Crestron.SimplSharpPro.DM.Cards.Dmc4kzHd, Dmc4kzHdAdapterSettings>
	{
		/// <summary>
		/// Constructor.
		/// </summary>
		public Dmc4kzHdAdapter()
		{
			Controls.Add(new Dmc4kzHdAdapterRoutingControl(this, 0));
		}

		protected override bool GetIsOnlineStatus()
		{
			return true;
			//TODO: Crestron api broken, re enable this line when a resolution comes back from them
			return Card != null && Card.PresentFeedback.BoolValue;
		}

		/// <summary>
		/// Instantiates an external card.
		/// </summary>
		/// <param name="cresnetId"></param>
		/// <param name="controlSystem"></param>
		/// <returns></returns>
		protected override Crestron.SimplSharpPro.DM.Cards.Dmc4kzHd InstantiateCardExternal(byte cresnetId,
		                                                                                  CrestronControlSystem controlSystem)
		{
			return new Crestron.SimplSharpPro.DM.Cards.Dmc4kzHd(cresnetId, controlSystem);
		}

		/// <summary>
		/// Instantiates an internal card.
		/// </summary>
		/// <param name="cardNumber"></param>
		/// <param name="switcher"></param>
		/// <returns></returns>
		protected override Crestron.SimplSharpPro.DM.Cards.Dmc4kzHd InstantiateCardInternal(uint cardNumber, Switch switcher)
		{
			return new Crestron.SimplSharpPro.DM.Cards.Dmc4kzHd(cardNumber, switcher);
		}
	}
#else
	public sealed class Dmc4kzHdAdapter : AbstractDmc4kHdBaseAdapter<Dmc4kzHdAdapterSettings>
	{
	}
#endif
}
