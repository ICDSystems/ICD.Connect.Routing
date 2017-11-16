using ICD.Common.Services.Logging;
#if SIMPLSHARP
using Crestron.SimplSharpPro;
using Crestron.SimplSharpPro.DM;
#endif

namespace ICD.Connect.Routing.CrestronPro.Cards.Inputs.Dmc4kHdDsp
{
#if SIMPLSHARP
	public sealed class Dmc4kHdDspAdapter : AbstractInputCardAdapter<Crestron.SimplSharpPro.DM.Cards.Dmc4kHdDsp, Dmc4kHdDspAdapterSettings>
	{
		/// <summary>
		/// Constructor.
		/// </summary>
        public Dmc4kHdDspAdapter()
		{
            Controls.Add(new Dmc4kHdDspAdapterRoutingControl(this, 0));
		}

		/// <summary>
		/// Instantiates an external card.
		/// </summary>
		/// <param name="cresnetId"></param>
		/// <param name="controlSystem"></param>
		/// <returns></returns>
        protected override Crestron.SimplSharpPro.DM.Cards.Dmc4kHdDsp InstantiateCardExternal(byte cresnetId,
		                                                                                 CrestronControlSystem controlSystem)
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
	public sealed class Dmc4kHdDspAdapter : AbstractInputCardAdapter<Dmc4kHdDspAdapterSettings>
	{
	}
#endif
}
