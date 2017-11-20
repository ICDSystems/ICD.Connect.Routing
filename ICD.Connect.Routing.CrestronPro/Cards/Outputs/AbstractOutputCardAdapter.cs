using System;
using System.Collections.Generic;
using System.Linq;
using ICD.Common.Properties;
using ICD.Common.Utils.Extensions;
using ICD.Connect.Routing.CrestronPro.DigitalMedia;
#if SIMPLSHARP
using Crestron.SimplSharpPro;
using ICD.Connect.Misc.CrestronPro.Utils.Extensions;
using Crestron.SimplSharpPro.DM;
using Crestron.SimplSharpPro.DM.Cards;
using ICD.Connect.Routing.CrestronPro.Utils;
#endif
using ICD.Common.Services.Logging;
using ICD.Connect.Devices;
using ICD.Connect.Settings.Core;

namespace ICD.Connect.Routing.CrestronPro.Cards
{
#if SIMPLSHARP
    public abstract class AbstractOutputCardAdapter<TCard, TSettings> : AbstractCardAdapterBase<TCard, TSettings>, IOutputCardAdapter
        where TCard : DmcOutputSingle
#else
	public abstract class AbstractOutputCardAdapter<TSettings> : AbstractCardAdapterBase<TSettings>, IOutputCardAdapter
#endif
        where TSettings : IOutputCardSettings, new()
    {

#if SIMPLSHARP
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

            if (Card != null && card != null)
            {
                Unsubscribe(GetInternalCards());

                card.Dispose();
            }
            Card = card;

            Register(GetInternalCards());
            Subscribe(GetInternalCards());

            UpdateCachedOnlineStatus();
        }



        #region Card Callbacks

#if SIMPLSHARP
        /// <summary>
        /// Subscribe to the card events on the internal cards from this output card.
        /// </summary>
        /// <param name="cards"></param>
        protected void Subscribe(IEnumerable<CardDevice> cards)
        {
            foreach (var card in cards)
            {
                card.OnlineStatusChange += CardOnLineStatusChange;
            }
        }

        /// <summary>
        /// Unsubscribe from the card events on the internal cards from this output card.
        /// </summary>
        /// <param name="cards"></param>
        protected void Unsubscribe(IEnumerable<CardDevice> cards)
        {
            foreach (var card in cards)
            {
                card.OnlineStatusChange -= CardOnLineStatusChange;
            }
        }

        /// <summary>
        /// Registers the internal cards from this output card and then re-registers their parents.
        /// </summary>
        /// <param name="cards"></param>
        protected void Register(IEnumerable<CardDevice> cards)
        {
            var card = cards.FirstOrDefault();
            if (card == null)
            {
                return;
            }

            GenericDevice parent = ((Card.DMOCard)card.Parent).Parent as GenericDevice;
            if (parent == null)
            {
                return;
            }
            eDeviceRegistrationUnRegistrationResponse parentResult = parent.ReRegister();
            if (parentResult != eDeviceRegistrationUnRegistrationResponse.Success)
            {
                Logger.AddEntry(eSeverity.Error, "Unable to register parent {0} - {1}", parent.GetType().Name, parentResult);
            }
        }

        /// <summary>
        /// Called when the card goes online or offline.
        /// </summary>
        /// <param name="currentDevice"></param>
        /// <param name="args"></param>
        protected void CardOnLineStatusChange(GenericBase currentDevice, OnlineOfflineEventArgs args)
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

#if SIMPLSHARP
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

#if SIMPLSHARP
            SetCard(settings, factory);
#endif
        }

#if SIMPLSHARP
        private void SetCard(TSettings settings, IDeviceFactory factory)
        {
            TCard card = DmEndpointFactoryUtils.InstantiateCard<TCard>(settings.CardNumber,
                                                                       settings.SwitcherId,
                                                                       factory,
                                                                       InstantiateCardInternal);

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

        /// <summary>
        /// Gets the current online status of the device.
        /// </summary>
        /// <returns></returns>
        protected override bool GetIsOnlineStatus()
        {
#if SIMPLSHARP
            return Card != null && GetInternalCards().AnyAndAll(c => c.IsOnline);
#else
            return false;
#endif
        }
    }
}
