using System.Collections.Generic;
using System.Linq;
using ICD.Common.Utils.Extensions;
#if SIMPLSHARP
using Crestron.SimplSharpPro.DM;
using Crestron.SimplSharpPro.DM.Cards;
#endif

namespace ICD.Connect.Routing.CrestronPro.Cards.Outputs.Dmc4kCoHd
{
#if SIMPLSHARP
    // ReSharper disable once InconsistentNaming
    public sealed class Dmc4kCoHdAdapter : AbstractOutputCardAdapter<Dmc4kCoHdSingle, Dmc4kCoHdAdapterSettings>
    {
        /// <summary>
        /// Constructor.
        /// </summary>
        public Dmc4kCoHdAdapter()
        {
            Controls.Add(new Dmc4kCoHdAdapterRoutingControl(this, 0));
        }

        /// <summary>
        /// Gets the current online status of the device.
        /// </summary>
        /// <returns></returns>
        protected override bool GetIsOnlineStatus()
        {
            return Card != null && 
                   GetInternalCards().Select(internalCard => internalCard as DmcCoBaseB)
                                     .All(internalBase => internalBase == null || internalBase.OnlineFeedback.BoolValue);
        }

        public override IEnumerable<CardDevice> GetInternalCards()
        {
            if (Card == null)
                yield break;

            yield return Card.Card1;
            yield return Card.Card2;
        }

        /// <summary>
        /// Instantiates an internal card.
        /// </summary>
        /// <param name="cardNumber"></param>
        /// <param name="switcher"></param>
        /// <returns></returns>
        protected override Dmc4kCoHdSingle InstantiateCardInternal(uint cardNumber, Switch switcher)
        {
            return new Dmc4kCoHdSingle(cardNumber, switcher);
        }
    }
#else
	public sealed class Dmc4kCoHdAdapter : AbstractOutputCardAdapter<Dmc4kCoHdAdapterSettings>
	{
	    protected override bool GetIsOnlineStatus()
	    {
	        return false;
	    }
	}
#endif
}
