using System;
using System.Collections.Generic;
using System.Linq;
using ICD.Common.Utils.Services.Logging;
using ICD.Connect.Settings;
#if !NETSTANDARD
using Crestron.SimplSharpPro;
using Crestron.SimplSharpPro.DM;
using Crestron.SimplSharpPro.DM.Cards;
using ICD.Connect.Routing.CrestronPro.Utils;
using ICD.Connect.Misc.CrestronPro.Extensions;
#endif

namespace ICD.Connect.Routing.CrestronPro.Cards.Outputs
{
#if !NETSTANDARD
	public abstract class AbstractOutputCardAdapter<TCard, TSettings> : AbstractCardAdapterBase<TCard, TSettings>,
	                                                                    IOutputCardAdapter
		where TCard : DmcOutputSingle
#else
	public abstract class AbstractOutputCardAdapter<TSettings> : AbstractCardAdapterBase<TSettings>, IOutputCardAdapter
#endif
		where TSettings : IOutputCardSettings, new()
	{
#if !NETSTANDARD
		public abstract IEnumerable<CardDevice> GetInternalCards();

		/// <summary>
		/// Sets the wrapped card instance.
		/// </summary>
		/// <param name="card"></param>
		/// <param name="number"></param>
		/// <param name="switcherId"></param>
		public void SetCard(TCard card, int? number, int? switcherId)
		{
			if (Card == card)
				return;

			SwitcherId = switcherId;
			CardNumber = number;

			if (Card != null)
			{
				Unsubscribe(GetInternalCards());
				Card.Dispose();
			}
			Card = card;

			Register(GetInternalCards());
			Subscribe(GetInternalCards());

			UpdateCachedOnlineStatus();
		}

		#region Card Callbacks

#if !NETSTANDARD
		/// <summary>
		/// Subscribe to the card events on the internal cards from this output card.
		/// </summary>
		/// <param name="cards"></param>
		protected void Subscribe(IEnumerable<CardDevice> cards)
		{
			foreach (CardDevice card in cards)
			{
				card.OnlineStatusChange += CardOnLineStatusChange;
				card.BaseEvent += CardOnBaseEvent;
			}
		}

		/// <summary>
		/// Unsubscribe from the card events on the internal cards from this output card.
		/// </summary>
		/// <param name="cards"></param>
		protected void Unsubscribe(IEnumerable<CardDevice> cards)
		{
			foreach (CardDevice card in cards)
			{
				card.OnlineStatusChange -= CardOnLineStatusChange;
				card.BaseEvent -= CardOnBaseEvent;
			}
		}

		/// <summary>
		/// Reregisters the parent switcher from the first output card on this list.
		/// </summary>
		/// <param name="cards"></param>
		protected void Register(IEnumerable<CardDevice> cards)
		{
			CardDevice card = cards.FirstOrDefault();
			if (card == null)
				return;

			GenericDevice parent = ((Card.DMOCard)card.Parent).Parent as GenericDevice;
			if (parent == null)
				return;
			eDeviceRegistrationUnRegistrationResponse parentResult = parent.ReRegister();
			if (parentResult != eDeviceRegistrationUnRegistrationResponse.Success)
			{
				Logger.Log(eSeverity.Error, "Unable to register parent {0} - {1}", parent.GetType().Name,
				           parentResult);
			}
		}

		/// <summary>
		/// Called when the card goes online or offline.
		/// </summary>
		/// <param name="currentDevice"></param>
		/// <param name="args"></param>
		private void CardOnLineStatusChange(GenericBase currentDevice, OnlineOfflineEventArgs args)
		{
			UpdateCachedOnlineStatus();
		}

		/// <summary>
		/// Called whenever the card fires any event. 
		/// Needed because Crestron does not properly update the online status of the cards.
		/// </summary>
		/// <param name="device"></param>
		/// <param name="args"></param>
		private void CardOnBaseEvent(GenericBase device, BaseEventArgs args)
		{
			UpdateCachedOnlineStatus();
		}
#endif

		#endregion

#endif

		#region Settings

		/// <summary>
		/// Override to clear the instance settings.
		/// </summary>
		protected override void ClearSettingsFinal()
		{
			base.ClearSettingsFinal();

#if !NETSTANDARD
			SetCard(null, null, null);
#endif
		}

		/// <summary>
		/// Override to apply properties to the settings instance.
		/// </summary>
		/// <param name="settings"></param>
		protected override void CopySettingsFinal(TSettings settings)
		{
			base.CopySettingsFinal(settings);
			settings.CardNumber = CardNumber;
			settings.SwitcherId = SwitcherId;
		}

		/// <summary>
		/// Override to apply settings to the instance.
		/// </summary>
		/// <param name="settings"></param>
		/// <param name="factory"></param>
		protected override void ApplySettingsFinal(TSettings settings, IDeviceFactory factory)
		{
			base.ApplySettingsFinal(settings, factory);

#if !NETSTANDARD
			SetCard(settings, factory);
#endif
		}

#if !NETSTANDARD
		private void SetCard(TSettings settings, IDeviceFactory factory)
		{
			TCard card = null;

			try
			{
				card = DmEndpointFactoryUtils.InstantiateCard<TCard>(settings.CardNumber,
				                                                     settings.SwitcherId,
				                                                     factory,
				                                                     InstantiateCardInternal);
			}
			catch (Exception e)
			{
				Logger.Log(eSeverity.Error, "Failed to instantiate {0} - {1}", typeof(TCard).Name, e.Message);
			}

			SetCard(card, settings.CardNumber, settings.SwitcherId);
		}

		/// <summary>
		/// Instantiates an internal card.
		/// </summary>
		/// <param name="cardNumber"></param>
		/// <param name="switcher"></param>
		/// <returns></returns>
		protected abstract TCard InstantiateCardInternal(uint cardNumber, Switch switcher);

		/// <summary>
		/// Gets the wrapped internal card.
		/// </summary>
		DmcOutputSingle IOutputCardAdapter.Card { get { return Card; } }
#endif

		#endregion
	}
}
