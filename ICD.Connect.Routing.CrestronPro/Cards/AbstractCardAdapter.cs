﻿#if SIMPLSHARP
using Crestron.SimplSharpPro;
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
	public abstract class AbstractCardAdapter<TCard, TSettings> : AbstractDevice<TSettings>, ICardAdapter
		where TCard : CardDevice
#else
	public abstract class AbstractCardAdapter<TSettings> : AbstractDevice<TSettings>, ICardAdapter
#endif
		where TSettings : ICardSettings, new()
	{
#if SIMPLSHARP
		public delegate void CardChangeCallback(object sender, TCard transmitter);

		/// <summary>
		/// Called when the wrapped internal card changes.
		/// </summary>
		public event Cards.CardChangeCallback OnCardChanged;

		private TCard m_Card;
#endif

		// Used with settings
		private byte? m_CresnetId;
		private int? m_CardNumber;
		private int? m_SwitcherId;

#if SIMPLSHARP
		/// <summary>
		/// Gets the wrapped internal card.
		/// </summary>
		public TCard Card
		{
			get { return m_Card; }
			private set
			{
				if (value == m_Card)
					return;

				m_Card = value;

				Cards.CardChangeCallback handler = OnCardChanged;
				if (handler != null)
					handler(this, m_Card);
			}
		}

		/// <summary>
		/// Gets the wrapped internal card.
		/// </summary>
		CardDevice ICardAdapter.Card { get { return Card; } }
#endif

		/// <summary>
		/// Release resources.
		/// </summary>
		protected override void DisposeFinal(bool disposing)
		{
#if SIMPLSHARP
			OnCardChanged = null;
#endif

			base.DisposeFinal(disposing);
		}

#if SIMPLSHARP
		/// <summary>
		/// Sets the wrapped card instance.
		/// </summary>
		/// <param name="card"></param>
		/// <param name="number"></param>
		/// <param name="switcherId"></param>
		public void SetCard(TCard card, int number, int switcherId)
		{
			SetCard(card, null, number, switcherId);
		}

		/// <summary>
		/// Sets the wrapped card instance.
		/// </summary>
		/// <param name="card"></param>
		/// <param name="cresnetId"></param>
		public void SetCard(TCard card, byte cresnetId)
		{
			SetCard(card, cresnetId, null, null);
		}

		/// <summary>
		/// Sets the wrapped card instance.
		/// </summary>
		/// <param name="card"></param>
		/// <param name="cresnetId"></param>
		/// <param name="number"></param>
		/// <param name="switcherId"></param>
		private void SetCard(TCard card, byte? cresnetId, int? number, int? switcherId)
		{
			m_CresnetId = cresnetId;
			m_CardNumber = number;
			m_SwitcherId = switcherId;

			if (card == Card)
				return;

			if (Card != null)
			{
				if (Card.Registered)
					Card.UnRegister();

				try
				{
					Card.Dispose();
				}
				catch
				{
				}
			}

			Unsubscribe(Card);
			Card = card;
			Subscribe(Card);

			if (Card != null && !Card.Registered)
			{
				if (Name != null)
					Card.Description = Name;
				eDeviceRegistrationUnRegistrationResponse result = Card.Register();
				if (result != eDeviceRegistrationUnRegistrationResponse.Success)
					Logger.AddEntry(eSeverity.Error, "Unable to register {0} - {1}", Card.GetType().Name, result);
			}

			UpdateCachedOnlineStatus();
		}
#endif

		/// <summary>
		/// Gets the current online status of the device.
		/// </summary>
		/// <returns></returns>
		protected override bool GetIsOnlineStatus()
		{
#if SIMPLSHARP
			return m_Card != null && m_Card.IsOnline;
#else
			return false;
#endif
		}

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

			settings.CresnetId = m_CresnetId;
			settings.CardNumber = m_CardNumber;
			settings.SwitcherId = m_SwitcherId;
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
#endif

#endregion
	}
}