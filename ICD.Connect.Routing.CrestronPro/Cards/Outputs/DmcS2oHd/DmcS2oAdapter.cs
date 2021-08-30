using System;
using System.Collections.Generic;
using System.Linq;
using ICD.Connect.Devices.Controls;
using ICD.Connect.Settings;
#if !NETSTANDARD
using Crestron.SimplSharpPro.DM;
using Crestron.SimplSharpPro.DM.Cards;
#endif

namespace ICD.Connect.Routing.CrestronPro.Cards.Outputs.DmcS2oHd
{
#if !NETSTANDARD
	// ReSharper disable once InconsistentNaming
	public sealed class DmcS2oHdAdapter : AbstractOutputCardAdapter<DmcS2oHdSingle, DmcS2oHdAdapterSettings>
	{
		/// <summary>
		/// Override to add controls to the device.
		/// </summary>
		/// <param name="settings"></param>
		/// <param name="factory"></param>
		/// <param name="addControl"></param>
		protected override void AddControls(DmcS2oHdAdapterSettings settings, IDeviceFactory factory, Action<IDeviceControl> addControl)
		{
			base.AddControls(settings, factory, addControl);

			addControl(new DmcS2oHdAdapterRoutingControl(this, 0));
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
		protected override DmcS2oHdSingle InstantiateCardInternal(uint cardNumber, Switch switcher)
		{
			return new DmcS2oHdSingle(cardNumber, switcher);
		}
	}
#else
	public sealed class DmcS2oHdAdapter : AbstractOutputCardAdapter<DmcS2oHdAdapterSettings>
	{
	    protected override bool GetIsOnlineStatus()
	    {
	        return false;
	    }
	}
#endif
}
