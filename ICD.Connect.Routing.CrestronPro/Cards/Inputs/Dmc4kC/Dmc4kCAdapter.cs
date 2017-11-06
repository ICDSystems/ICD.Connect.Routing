using ICD.Common.Services.Logging;
#if SIMPLSHARP
using Crestron.SimplSharpPro;
using Crestron.SimplSharpPro.DM;
#endif

namespace ICD.Connect.Routing.CrestronPro.Cards.Inputs.Dmc4kC
{
#if SIMPLSHARP
    public sealed class Dmc4kCAdapter : AbstractCardAdapter<Crestron.SimplSharpPro.DM.Cards.Dmc4kC, Dmc4kCAdapterSettings>
    {
        /// <summary>
        /// Constructor.
        /// </summary>
        public Dmc4kCAdapter()
        {
            Controls.Add(new Dmc4kCAdapterRoutingControl(this, 0));
        }

        /// <summary>
        /// Instantiates an external card.
        /// </summary>
        /// <param name="cresnetId"></param>
        /// <param name="controlSystem"></param>
        /// <returns></returns>
        protected override Crestron.SimplSharpPro.DM.Cards.Dmc4kC InstantiateCardExternal(byte cresnetId,
                                                                                         CrestronControlSystem controlSystem)
        {
            return new Crestron.SimplSharpPro.DM.Cards.Dmc4kC(cresnetId, controlSystem);
        }

        /// <summary>
        /// Instantiates an internal card.
        /// </summary>
        /// <param name="cardNumber"></param>
        /// <param name="switcher"></param>
        /// <returns></returns>
        protected override Crestron.SimplSharpPro.DM.Cards.Dmc4kC InstantiateCardInternal(uint cardNumber, Switch switcher)
        {
            Logger.AddEntry(eSeverity.Warning, "Card Instantiated");
            return new Crestron.SimplSharpPro.DM.Cards.Dmc4kC(cardNumber, switcher);
        }
    }
#else
	public sealed class Dmc4kCAdapter : AbstractCardAdapter<Dmc4kCAdapterSettings>
	{
	}
#endif
}