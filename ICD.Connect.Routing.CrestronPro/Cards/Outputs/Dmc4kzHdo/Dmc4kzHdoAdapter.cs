#if SIMPLSHARP
using Crestron.SimplSharpPro.DM;
using Crestron.SimplSharpPro.DM.Cards;
using System.Collections.Generic;
using System.Linq;
using ICD.Connect.Misc.CrestronPro.Extensions;
#endif

namespace ICD.Connect.Routing.CrestronPro.Cards.Outputs.Dmc4kzHdo
{
#if SIMPLSHARP
	// ReSharper disable once InconsistentNaming
	public sealed class Dmc4kzHdoAdapter : AbstractOutputCardAdapter<Dmc4kzHdoSingle, Dmc4kzHdoAdapterSettings>
	{
		/// <summary>
		/// Constructor.
		/// </summary>
		public Dmc4kzHdoAdapter()
		{
			Controls.Add(new Dmc4kzHdoAdapterRoutingControl(this, 0));
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
			                         .All(internalBase => internalBase == null || internalBase.PresentFeedback.GetBoolValueOrDefault());
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
		protected override Dmc4kzHdoSingle InstantiateCardInternal(uint cardNumber, Switch switcher)
		{
			return new Dmc4kzHdoSingle(cardNumber, switcher);
		}
	}
#else
	public sealed class Dmc4kzHdoAdapter : AbstractDmc4kCoHdBaseAdapter<Dmc4kzHdoAdapterSettings>
	{
	    protected override bool GetIsOnlineStatus()
	    {
	        return false;
	    }
	}
#endif
}
