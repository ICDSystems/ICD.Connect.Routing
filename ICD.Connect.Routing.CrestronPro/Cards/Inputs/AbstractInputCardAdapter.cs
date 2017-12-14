using System;
using ICD.Common.Properties;
#if SIMPLSHARP
using Crestron.SimplSharpPro;
using Crestron.SimplSharpPro.DM;
using Crestron.SimplSharpPro.DM.Cards;
using ICD.Connect.Routing.CrestronPro.Utils;
using ICD.Connect.Misc.CrestronPro.Utils.Extensions;
#endif
using ICD.Common.Services.Logging;
using ICD.Connect.Settings.Core;

namespace ICD.Connect.Routing.CrestronPro.Cards.Inputs
{
#if SIMPLSHARP
    public abstract class AbstractInputCardAdapter<TCard, TSettings> : AbstractCardAdapterBase<TCard, TSettings>, IInputCardAdapter
        where TCard : CardDevice
#else
	public abstract class AbstractInputCardAdapter<TSettings> : AbstractCardAdapterBase<TSettings>, ICardAdapter
#endif
        where TSettings : IInputCardSettings, new()
    {
        // Used with settings
        private byte? m_CresnetId;

        /// <summary>
        /// Gets the Cresnet Id of this Input Card
        /// </summary>
        [PublicAPI]
        public byte? CresnetId
        {
            get { return m_CresnetId; }
            protected set { m_CresnetId = value; }
        }

#if SIMPLSHARP
        /// <summary>
        /// Sets the wrapped card instance.
        /// </summary>
        /// <param name="card"></param>
        /// <param name="cresnetId"></param>
        /// <param name="number"></param>
        /// <param name="switcherId"></param>
        private void SetCard(TCard card, byte? cresnetId, int? number, int? switcherId)
        {
            CresnetId = cresnetId;
            CardNumber = number;
            SwitcherId = switcherId;

            if (card == Card)
                return;

            if (Card != null)
            {
                Unsubscribe(Card);
                UnRegister(Card);
                try
                {
                    Card.Dispose();
                }
                catch
                {
                }
            }

            Card = card;
            Register(Card);
            Subscribe(Card);

            UpdateCachedOnlineStatus();
        }
#endif

        #region Card Callbacks

#if SIMPLSHARP
        /// <summary>
        /// Subscribe to the card events.
        /// </summary>
        /// <param name="card"></param>
        private void Subscribe(TCard card)
        {
            if (card == null)
                return;

            card.OnlineStatusChange += CardOnLineStatusChange;
            card.BaseEvent += CardOnBaseEvent;
        }

        /// <summary>
        /// Unsubscribe from the card events.
        /// </summary>
        /// <param name="card"></param>
        private void Unsubscribe(TCard card)
        {
            if (card == null)
                return;

            card.OnlineStatusChange -= CardOnLineStatusChange;
            card.BaseEvent -= CardOnBaseEvent;
        }

        /// <summary>
        /// Registers the card and then re-registers its parent.
        /// </summary>
        /// <param name="card"></param>
        private void Register(TCard card)
        {
            if (card == null)
            {
                return;
            }

            GenericDevice parent = card.Parent as GenericDevice;
            if (parent == null)
            {
                return;
            }

            eDeviceRegistrationUnRegistrationResponse parentResult = parent.ReRegister();
            if (parentResult != eDeviceRegistrationUnRegistrationResponse.Success)
            {
                Logger.AddEntry(eSeverity.Error, "{0} unable to register parent {1} - {2}", this, parent.GetType().Name, parentResult);
            }
        }

        private void UnRegister(TCard card)
        {
            if (card == null || !card.Registered)
            {
                return;
            }
            card.UnRegister();
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

        #region Settings

        /// <summary>
        /// Override to clear the instance settings.
        /// </summary>
        protected override void ClearSettingsFinal()
        {
            base.ClearSettingsFinal();

#if SIMPLSHARP
            SetCard(null, null, null, null);
#endif
        }

        /// <summary>
        /// Override to apply properties to the settings instance.
        /// </summary>
        /// <param name="settings"></param>
        protected override void CopySettingsFinal(TSettings settings)
        {
            base.CopySettingsFinal(settings);

            settings.CresnetId = CresnetId;
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
            TCard card = DmEndpointFactoryUtils.InstantiateCard<TCard>(settings.CresnetId,
                                                                       settings.CardNumber,
                                                                       settings.SwitcherId,
                                                                       factory,
                                                                       InstantiateCardExternal,
                                                                       InstantiateCardInternal);

            SetCard(card, settings.CresnetId, settings.CardNumber, settings.SwitcherId);
        }

        /// <summary>
        /// Instantiates an external card.
        /// </summary>
        /// <param name="cresnetId"></param>
        /// <param name="controlSystem"></param>
        /// <returns></returns>
        protected abstract TCard InstantiateCardExternal(byte cresnetId, CrestronControlSystem controlSystem);

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
        CardDevice IInputCardAdapter.Card { get { return Card; } }
#endif

        #endregion


    }
}
