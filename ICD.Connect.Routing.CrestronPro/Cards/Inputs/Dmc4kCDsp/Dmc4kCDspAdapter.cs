using ICD.Connect.Misc.CrestronPro.Extensions;
using ICD.Connect.Routing.CrestronPro.Cards.Inputs.Dmc4kCDspBase;
#if SIMPLSHARP
using Crestron.SimplSharpPro;
using Crestron.SimplSharpPro.DM;
#endif

namespace ICD.Connect.Routing.CrestronPro.Cards.Inputs.Dmc4kCDsp
{
#if SIMPLSHARP
	public sealed class Dmc4kCDspAdapter :
		AbstractDmc4kCDspBaseAdapter<Crestron.SimplSharpPro.DM.Cards.Dmc4kCDsp, Dmc4kCDspAdapterSettings>
	{
		/// <summary>
		/// Constructor.
		/// </summary>
		public Dmc4kCDspAdapter()
		{
			Controls.Add(new Dmc4kCDspAdapterRoutingControl(this, 0));
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
		protected override Crestron.SimplSharpPro.DM.Cards.Dmc4kCDsp InstantiateCardExternal(byte cresnetId,
		                                                                                     CrestronControlSystem
			                                                                                     controlSystem)
		{
			return new Crestron.SimplSharpPro.DM.Cards.Dmc4kCDsp(cresnetId, controlSystem);
		}

		/// <summary>
		/// Instantiates an internal card.
		/// </summary>
		/// <param name="cardNumber"></param>
		/// <param name="switcher"></param>
		/// <returns></returns>
		protected override Crestron.SimplSharpPro.DM.Cards.Dmc4kCDsp InstantiateCardInternal(uint cardNumber, Switch switcher)
		{
			return new Crestron.SimplSharpPro.DM.Cards.Dmc4kCDsp(cardNumber, switcher);
		}
	}
#else
	public sealed class Dmc4kCDspAdapter : AbstractInputCardAdapter<Dmc4kCDspAdapterSettings>
	{
	}
#endif
}
