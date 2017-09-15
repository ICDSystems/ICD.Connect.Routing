#if SIMPLSHARP
using System;
using Crestron.SimplSharpPro;
using Crestron.SimplSharpPro.DM.Cards;
using ICD.Common.Utils.Extensions;
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

		/// <summary>
		/// Gets the current subscribed card.
		/// </summary>
		protected CardDevice Card { get; private set; }

		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="parent"></param>
		/// <param name="id"></param>
		protected AbstractInputCardAdapterRoutingControl(TParent parent, int id)
			: base(parent, id)
		{
			m_Cache = new SwitcherCache();
			Subscribe(m_Cache);

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

			Unsubscribe(m_Cache);
			Unsubscribe(Parent);
			Unsubscribe(Card);
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

		/// <summary>
		/// Update the cache with the current state of the card.
		/// </summary>
		/// <param name="cache"></param>
		protected abstract void UpdateCache(SwitcherCache cache);

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
			Unsubscribe(Card);

			m_Cache.Clear();
			Card = card;

			Subscribe(Card);

			UpdateCache(m_Cache);
		}

#endregion

#region Cache Callbacks

		/// <summary>
		/// Subscribe to the switcher cache events.
		/// </summary>
		/// <param name="cache"></param>
		private void Subscribe(SwitcherCache cache)
		{
			cache.OnSourceDetectionStateChange += CacheOnSourceDetectionStateChange;
			cache.OnActiveInputsChanged += CacheOnActiveInputsChanged;
			cache.OnActiveTransmissionStateChanged += CacheOnActiveTransmissionStateChanged;
		}

		/// <summary>
		/// Unsubscribe from the switcher cache events.
		/// </summary>
		/// <param name="cache"></param>
		private void Unsubscribe(SwitcherCache cache)
		{
			cache.OnSourceDetectionStateChange += CacheOnSourceDetectionStateChange;
			cache.OnActiveInputsChanged += CacheOnActiveInputsChanged;
			cache.OnActiveTransmissionStateChanged += CacheOnActiveTransmissionStateChanged;
		}

		/// <summary>
		/// Called when the cached active transmission state changes.
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="args"></param>
		private void CacheOnActiveTransmissionStateChanged(object sender, TransmissionStateEventArgs args)
		{
			OnActiveTransmissionStateChanged.Raise(this, new TransmissionStateEventArgs(args));
		}

		/// <summary>
		/// Called when the cached active inputs change.
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="args"></param>
		private void CacheOnActiveInputsChanged(object sender, ActiveInputStateChangeEventArgs args)
		{
			OnActiveInputsChanged.Raise(this, new ActiveInputStateChangeEventArgs(args));
		}

		/// <summary>
		/// Called when the caches source detection state changes.
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="args"></param>
		private void CacheOnSourceDetectionStateChange(object sender, SourceDetectionStateChangeEventArgs args)
		{
			OnSourceDetectionStateChange.Raise(this, new SourceDetectionStateChangeEventArgs(args));
		}

#endregion

#region Card Callbacks

		/// <summary>
		/// Subscribe to the card events.
		/// </summary>
		/// <param name="card"></param>
		private void Subscribe(CardDevice card)
		{
			if (card == null)
				return;

			card.BaseEvent += CardOnBaseEvent;
		}

		/// <summary>
		/// Unsubscribe from the card events.
		/// </summary>
		/// <param name="card"></param>
		private void Unsubscribe(CardDevice card)
		{
			if (card == null)
				return;

			card.BaseEvent -= CardOnBaseEvent;
		}

		/// <summary>
		/// Called when the card reports a change.
		/// </summary>
		/// <param name="device"></param>
		/// <param name="args"></param>
		private void CardOnBaseEvent(GenericBase device, BaseEventArgs args)
		{
			UpdateCache(m_Cache);
		}

#endregion
	}
}
#endif
