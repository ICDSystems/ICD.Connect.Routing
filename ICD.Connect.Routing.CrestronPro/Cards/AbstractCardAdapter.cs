using Crestron.SimplSharpPro;
using Crestron.SimplSharpPro.DM;
using Crestron.SimplSharpPro.DM.Cards;
using ICD.Common.Services.Logging;
using ICD.Connect.Devices;
using ICD.Connect.Settings.Core;

namespace ICD.Connect.Routing.CrestronPro.Cards
{
	public abstract class AbstractCardAdapter<TCard, TSettings> : AbstractDevice<TSettings>
		where TCard : CardDevice
		where TSettings : ICardSettings, new()
	{
		public delegate void CardChangeCallback(object sender, TCard transmitter);

		/// <summary>
		/// Called when the wrapped internal card changes.
		/// </summary>
		public event CardChangeCallback OnCardChanged;

		private TCard m_Card;

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

				CardChangeCallback handler = OnCardChanged;
				if (handler != null)
					handler(this, m_Card);
			}
		}

		/// <summary>
		/// Release resources.
		/// </summary>
		protected override void DisposeFinal(bool disposing)
		{
			OnCardChanged = null;

			base.DisposeFinal(disposing);
		}

		/// <summary>
		/// Sets the wrapped internal card instance.
		/// </summary>
		/// <param name="card"></param>
		public void SetCard(TCard card)
		{
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

		/// <summary>
		/// Instantiates an external card.
		/// </summary>
		/// <param name="cresnetId"></param>
		/// <param name="controlSystem"></param>
		/// <returns></returns>
		protected abstract TCard InstantiateCard(uint cresnetId, CrestronControlSystem controlSystem);

		/// <summary>
		/// Instantiates an internal card.
		/// </summary>
		/// <param name="cardNumber"></param>
		/// <param name="switcher"></param>
		/// <returns></returns>
		protected abstract TCard InstantiateCard(uint cardNumber, Switch switcher);

		/// <summary>
		/// Gets the current online status of the device.
		/// </summary>
		/// <returns></returns>
		protected override bool GetIsOnlineStatus()
		{
			return m_Card != null && m_Card.IsOnline;
		}

		#region Card Callbacks

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

		#endregion

		#region Settings

		/// <summary>
		/// Override to clear the instance settings.
		/// </summary>
		protected override void ClearSettingsFinal()
		{
			base.ClearSettingsFinal();

			SetCard(null);
		}

		/// <summary>
		/// Override to apply properties to the settings instance.
		/// </summary>
		/// <param name="settings"></param>
		protected override void CopySettingsFinal(TSettings settings)
		{
			base.CopySettingsFinal(settings);
		}

		/// <summary>
		/// Override to apply settings to the instance.
		/// </summary>
		/// <param name="settings"></param>
		/// <param name="factory"></param>
		protected override void ApplySettingsFinal(TSettings settings, IDeviceFactory factory)
		{
			base.ApplySettingsFinal(settings, factory);
		}

		#endregion
	}
}
