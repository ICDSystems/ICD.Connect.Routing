#if SIMPLSHARP
using System;
using Crestron.SimplSharpPro.DM;
using Crestron.SimplSharpPro.DM.Cards;
using ICD.Common.Properties;
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
		[CanBeNull]
		private CardDevice Card { get; set; }

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
		public override sealed bool GetSignalDetectedState(int input, eConnectionType type)
		{
			return m_Cache.GetSourceDetectedState(input, type);
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
			Unsubscribe(Card);

			m_Cache.Clear();
			Card = card;

			Subscribe(Card);

			RebuildCache();
		}

		/// <summary>
		/// Clears and updates the cache.
		/// </summary>
		private void RebuildCache()
		{
			m_Cache.Clear();

			// Source detection
			foreach (ConnectorInfo input in GetInputs())
			{
				bool detected = GetVideoDetectedFeedback(input.Address);
				m_Cache.SetSourceDetectedState(input.Address, eConnectionType.Audio | eConnectionType.Video, detected);
			}
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

			card.Switcher.DMInputChange += SwitcherOnDmInputChange;
		}

		/// <summary>
		/// Unsubscribe from the card events.
		/// </summary>
		/// <param name="card"></param>
		private void Unsubscribe(CardDevice card)
		{
			if (card == null)
				return;

			card.Switcher.DMInputChange -= SwitcherOnDmInputChange;
		}

		private void SwitcherOnDmInputChange(Switch device, DMInputEventArgs args)
		{
			if (Card == null)
				return;

			int input = (int)Card.SwitcherInputOutput.Number;
			if (args.Number != input)
				return;

			bool state = GetVideoDetectedFeedback(input);
			m_Cache.SetSourceDetectedState(input, eConnectionType.Audio | eConnectionType.Video, state);
		}

		private bool GetVideoDetectedFeedback(int input)
		{
			return Card != null && Card.Switcher.Inputs[(uint)input].VideoDetectedFeedback.BoolValue;
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
		}

		/// <summary>
		/// Unsubscribe from the switcher cache events.
		/// </summary>
		/// <param name="cache"></param>
		private void Unsubscribe(SwitcherCache cache)
		{
			cache.OnSourceDetectionStateChange += CacheOnSourceDetectionStateChange;
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
	}
}
#endif
