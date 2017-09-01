using System;
using Crestron.SimplSharpPro.DM.Cards;
using ICD.Connect.Routing.Connections;
using ICD.Connect.Routing.Controls;
using ICD.Connect.Routing.EventArguments;
using ICD.Connect.Routing.Utils;

namespace ICD.Connect.Routing.CrestronPro.Cards.Inputs
{
	public abstract class AbstractInputCardAdapterRoutingControl<TParent> : AbstractRouteMidpointControl<TParent>
		where TParent : ICardAdapter
	{
		public override event EventHandler<SourceDetectionStateChangeEventArgs> OnSourceDetectionStateChange;
		public override event EventHandler<ActiveInputStateChangeEventArgs> OnActiveInputsChanged;
		public override event EventHandler<TransmissionStateEventArgs> OnActiveTransmissionStateChanged;

		private readonly SwitcherCache m_Cache;
		private CardDevice m_SubscribedCard;

		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="parent"></param>
		/// <param name="id"></param>
		protected AbstractInputCardAdapterRoutingControl(TParent parent, int id)
			: base(parent, id)
		{
			m_Cache = new SwitcherCache();
			Subscribe(parent);
		}

		/// <summary>
		/// Override to release resources.
		/// </summary>
		/// <param name="disposing"></param>
		protected override void DisposeFinal(bool disposing)
		{
			OnSourceDetectionStateChange = null;
			OnActiveInputsChanged = null;

			base.DisposeFinal(disposing);

			Unsubscribe(Parent);
			Unsubscribe(m_SubscribedCard);
		}

		/// <summary>
		/// Returns true if a signal is detected at the given input.
		/// </summary>
		/// <param name="input"></param>
		/// <param name="type"></param>
		/// <returns></returns>
		public sealed override bool GetSignalDetectedState(int input, eConnectionType type)
		{
			return m_Cache.GetSourceDetectedState(input, type);
		}

		private void UpdateCache()
		{
			throw new NotImplementedException();
		}

		#region Parent Callbacks

		/// <summary>
		/// Subscribe to the parent events.
		/// </summary>
		/// <param name="parent"></param>
		private void Subscribe(TParent parent)
		{
			parent.OnCardChanged += ParentOnCardChanged;
		}

		/// <summary>
		/// Unsubscribe from the parent events.
		/// </summary>
		/// <param name="parent"></param>
		private void Unsubscribe(TParent parent)
		{
			parent.OnCardChanged -= ParentOnCardChanged;
		}

		/// <summary>
		/// Called when the wrapped card changes.
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="card"></param>
		private void ParentOnCardChanged(ICardAdapter sender, CardDevice card)
		{
			Unsubscribe(m_SubscribedCard);

			m_Cache.Clear();
			m_SubscribedCard = card;

			Subscribe(m_SubscribedCard);

			UpdateCache();
		}

		#endregion

		#region Card Callbacks

		/// <summary>
		/// Subscribe to the card events.
		/// </summary>
		/// <param name="card"></param>
		protected virtual void Subscribe(CardDevice card)
		{
		}

		/// <summary>
		/// Unsubscribe from the card events.
		/// </summary>
		/// <param name="card"></param>
		protected virtual void Unsubscribe(CardDevice card)
		{
		}

		#endregion
	}
}
