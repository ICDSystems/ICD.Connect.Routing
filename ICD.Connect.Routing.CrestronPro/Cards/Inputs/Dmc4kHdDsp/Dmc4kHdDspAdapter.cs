using ICD.Connect.Routing.CrestronPro.Cards.Inputs.Dmc4kHdDspBase;
#if SIMPLSHARP
using Crestron.SimplSharpPro;
using Crestron.SimplSharpPro.DM;
#endif

namespace ICD.Connect.Routing.CrestronPro.Cards.Inputs.Dmc4kHdDsp
{
#if SIMPLSHARP
	public sealed class Dmc4kHdDspAdapter :
		AbstractDmc4kHdDspBaseAdapter<Crestron.SimplSharpPro.DM.Cards.Dmc4kHdDsp, Dmc4kHdDspAdapterSettings>
	{
		/// <summary>
		/// Constructor.
		/// </summary>
		public Dmc4kHdDspAdapter()
		{
			Controls.Add(new Dmc4kHdDspAdapterRoutingControl(this, 0));
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
		protected override Crestron.SimplSharpPro.DM.Cards.Dmc4kHdDsp InstantiateCardExternal(byte cresnetId,
		                                                                                      CrestronControlSystem
			                                                                                      controlSystem)
		{
			return new Crestron.SimplSharpPro.DM.Cards.Dmc4kHdDsp(cresnetId, controlSystem);
		}

		/// <summary>
		/// Instantiates an internal card.
		/// </summary>
		/// <param name="cardNumber"></param>
		/// <param name="switcher"></param>
		/// <returns></returns>
		protected override Crestron.SimplSharpPro.DM.Cards.Dmc4kHdDsp InstantiateCardInternal(uint cardNumber, Switch switcher)
		{
			return new Crestron.SimplSharpPro.DM.Cards.Dmc4kHdDsp(cardNumber, switcher);
		}
	}
#else
	public sealed class Dmc4kHdDspAdapter : AbstractDmc4kHdDspBaseAdapter<Dmc4kHdDspAdapterSettings>
	{
	}
#endif
}
