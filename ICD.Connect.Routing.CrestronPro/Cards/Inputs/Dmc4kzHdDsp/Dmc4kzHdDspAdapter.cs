using ICD.Connect.Routing.CrestronPro.Cards.Inputs.Dmc4kHdDspBase;
#if SIMPLSHARP
using Crestron.SimplSharpPro;
using Crestron.SimplSharpPro.DM;
using ICD.Connect.Misc.CrestronPro.Extensions;
#endif

namespace ICD.Connect.Routing.CrestronPro.Cards.Inputs.Dmc4kzHdDsp
{
#if SIMPLSHARP
	public sealed class Dmc4kzHdDspAdapter :
		AbstractDmc4kHdDspBaseAdapter<Crestron.SimplSharpPro.DM.Cards.Dmc4kzHdDsp, Dmc4kzHdDspAdapterSettings>
	{
		/// <summary>
		/// Constructor.
		/// </summary>
		public Dmc4kzHdDspAdapter()
		{
			Controls.Add(new Dmc4kzHdDspAdapterRoutingControl(this, 0));
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
		protected override Crestron.SimplSharpPro.DM.Cards.Dmc4kzHdDsp InstantiateCardExternal(byte cresnetId,
		                                                                                      CrestronControlSystem
			                                                                                      controlSystem)
		{
			return new Crestron.SimplSharpPro.DM.Cards.Dmc4kzHdDsp(cresnetId, controlSystem);
		}

		/// <summary>
		/// Instantiates an internal card.
		/// </summary>
		/// <param name="cardNumber"></param>
		/// <param name="switcher"></param>
		/// <returns></returns>
		protected override Crestron.SimplSharpPro.DM.Cards.Dmc4kzHdDsp InstantiateCardInternal(uint cardNumber, Switch switcher)
		{
			return new Crestron.SimplSharpPro.DM.Cards.Dmc4kzHdDsp(cardNumber, switcher);
		}
	}
#else
	public sealed class Dmc4kzHdDspAdapter : AbstractDmc4kHdDspBaseAdapter<Dmc4kzHdDspAdapterSettings>
	{
	}
#endif
}
