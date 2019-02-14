using System.Collections.Generic;
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
		where TParent : IInputCardAdapter
	{
		/// <summary>
		/// Raised when an input source status changes.
		/// </summary>
		public override event EventHandler<SourceDetectionStateChangeEventArgs> OnSourceDetectionStateChange;

		/// <summary>
		/// Raised when the device starts/stops actively using an input, e.g. unroutes an input.
		/// </summary>
		public override event EventHandler<ActiveInputStateChangeEventArgs> OnActiveInputsChanged;

		/// <summary>
		/// Raised when the device starts/stops actively transmitting on an output.
		/// </summary>
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

		/// <summary>
		/// Gets the input at the given address.
		/// </summary>
		/// <param name="input"></param>
		/// <returns></returns>
		public override ConnectorInfo GetInput(int input)
		{
			if (ContainsInput(input))
				return new ConnectorInfo(input, eConnectionType.Audio | eConnectionType.Video);

			string message = string.Format("No input at address {0}", input);
			throw new ArgumentOutOfRangeException("input", message);
		}

		/// <summary>
		/// Returns true if the destination contains an input at the given address.
		/// </summary>
		/// <param name="input"></param>
		/// <returns></returns>
		public override bool ContainsInput(int input)
		{
			return input == 1;
		}

		/// <summary>
		/// Returns the inputs.
		/// </summary>
		/// <returns></returns>
		public override IEnumerable<ConnectorInfo> GetInputs()
		{
			yield return GetInput(1);
		}

		/// <summary>
		/// Gets the output at the given address.
		/// </summary>
		/// <param name="output"></param>
		/// <returns></returns>
		public override ConnectorInfo GetOutput(int output)
		{
			if (ContainsOutput(output))
				return new ConnectorInfo(output, eConnectionType.Audio | eConnectionType.Video);

			string message = string.Format("No output at address {0}", output);
			throw new ArgumentOutOfRangeException("output", message);
		}

		/// <summary>
		/// Returns true if the source contains an output at the given address.
		/// </summary>
		/// <param name="output"></param>
		/// <returns></returns>
		public override bool ContainsOutput(int output)
		{
			return output == 1;
		}

		/// <summary>
		/// Returns the outputs.
		/// </summary>
		/// <returns></returns>
		public override IEnumerable<ConnectorInfo> GetOutputs()
		{
			yield return GetOutput(1);
		}

		/// <summary>
		/// Gets the outputs for the given input.
		/// </summary>
		/// <param name="input"></param>
		/// <param name="type"></param>
		/// <returns></returns>
		public override IEnumerable<ConnectorInfo> GetOutputs(int input, eConnectionType type)
		{
			if (input != 1)
				throw new ArgumentException(string.Format("{0} only has 1 input", GetType().Name), "input");

			switch (type)
			{
				case eConnectionType.Audio:
				case eConnectionType.Video:
				case eConnectionType.Audio | eConnectionType.Video:
					yield return GetOutput(1);
					break;

				default:
					throw new ArgumentException("type");
			}
		}

		/// <summary>
		/// Gets the input routed to the given output matching the given type.
		/// </summary>
		/// <param name="output"></param>
		/// <param name="type"></param>
		/// <returns></returns>
		/// <exception cref="InvalidOperationException">Type has multiple flags.</exception>
		public override ConnectorInfo? GetInput(int output, eConnectionType type)
		{
			if (output != 1)
				throw new ArgumentException(string.Format("{0} only has 1 output", GetType().Name), "output");

			switch (type)
			{
				case eConnectionType.Audio:
				case eConnectionType.Video:
				case eConnectionType.Audio | eConnectionType.Video:
					return GetInput(1);

				default:
					throw new ArgumentException("type");
			}
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
		private void ParentOnCardChanged(ICardAdapter sender, object card)
		{
			Unsubscribe(Card);

			m_Cache.Clear();
			Card = card as CardDevice;

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
			cache.OnSourceDetectionStateChange -= CacheOnSourceDetectionStateChange;
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
