#if SIMPLSHARP
using Crestron.SimplSharpPro.DM;
using Crestron.SimplSharpPro.DM.Cards;
using System.Collections.Generic;
using System.Linq;
using ICD.Connect.Misc.CrestronPro.Extensions;
#endif

namespace ICD.Connect.Routing.CrestronPro.Cards.Outputs.Dmc4kHdo
{
#if SIMPLSHARP
	// ReSharper disable once InconsistentNaming
	public sealed class Dmc4kHdoAdapter : AbstractOutputCardAdapter<Dmc4kHdoSingle, Dmc4kHdoAdapterSettings>
	{
		/// <summary>
		/// Constructor.
		/// </summary>
		public Dmc4kHdoAdapter()
		{
			Controls.Add(new Dmc4kHdoAdapterRoutingControl(this, 0));
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
			       GetInternalCards().Select(internalCard => internalCard as Crestron.SimplSharpPro.DM.Cards.Dmc4kHdo)
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
		protected override Dmc4kHdoSingle InstantiateCardInternal(uint cardNumber, Switch switcher)
		{
			return new Dmc4kHdoSingle(cardNumber, switcher);
		}
	}
#else
	// ReSharper disable once InconsistentNaming
	public sealed class Dmc4kHdoAdapter : AbstractOutputCardAdapter<Dmc4kHdoAdapterSettings>
	{
	}
#endif
}
