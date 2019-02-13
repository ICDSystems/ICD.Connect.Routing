using System.Collections.Generic;
using System.Linq;
#if SIMPLSHARP
using Crestron.SimplSharpPro.DM;
using Crestron.SimplSharpPro.DM.Cards;
#endif

namespace ICD.Connect.Routing.CrestronPro.Cards.Outputs.DmcCoHd
{
#if SIMPLSHARP
	// ReSharper disable once InconsistentNaming
	public sealed class DmcCoHdAdapter : AbstractOutputCardAdapter<DmcCoHdSingle, DmcCoHdAdapterSettings>
	{
		/// <summary>
		/// Constructor.
		/// </summary>
		public DmcCoHdAdapter()
		{
			Controls.Add(new DmcCoHdAdapterRoutingControl(this, 0));
		}

		/// <summary>
		/// Gets the current online status of the device.
		/// </summary>
		/// <returns></returns>
		protected override bool GetIsOnlineStatus()
		{
			return true;
			//TODO: Crestron api broken, re enable this line when a resolution comes back from them
			return Card != null &&
			       GetInternalCards().Select(internalCard => internalCard as DmcCoBaseB)
			                         .All(internalBase => internalBase == null || internalBase.PresentFeedback.BoolValue);
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
		protected override DmcCoHdSingle InstantiateCardInternal(uint cardNumber, Switch switcher)
		{
			return new DmcCoHdSingle(cardNumber, switcher);
		}
	}
#else
	public sealed class DmcCoHdAdapter : AbstractOutputCardAdapter<DmcCoHdAdapterSettings>
	{
	    protected override bool GetIsOnlineStatus()
	    {
	        return false;
	    }
	}
#endif
}