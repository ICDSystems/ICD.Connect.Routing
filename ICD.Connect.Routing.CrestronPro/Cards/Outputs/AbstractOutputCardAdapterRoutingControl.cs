#if SIMPLSHARP
using System;
using System.Collections.Generic;
using System.Linq;
using Crestron.SimplSharpPro.DM.Cards;
using ICD.Common.Properties;
using ICD.Common.Utils.Extensions;
using ICD.Connect.Routing.Connections;
using ICD.Connect.Routing.Controls;
using ICD.Connect.Routing.EventArguments;

namespace ICD.Connect.Routing.CrestronPro.Cards.Outputs
{
	public abstract class AbstractOutputCardAdapterRoutingControl<TParent> : AbstractRouteSourceControl<TParent>
		where TParent : IOutputCardAdapter
	{
		/// <summary>
		/// Raised when the device starts/stops actively transmitting on an output.
		/// </summary>
		public override event EventHandler<TransmissionStateEventArgs> OnActiveTransmissionStateChanged;

		/// <summary>
		/// Gets the current subscribed card.
		/// </summary>
		[CanBeNull] private object m_Card;

		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="parent"></param>
		/// <param name="id"></param>
		protected AbstractOutputCardAdapterRoutingControl(TParent parent, int id)
			: base(parent, id)
		{
		}

		/// <summary>
		/// Override to release resources.
		/// </summary>
		/// <param name="disposing"></param>
		protected override void DisposeFinal(bool disposing)
		{
			base.DisposeFinal(disposing);

			Unsubscribe(m_Card);
		}

		#region Methods

		/// <summary>
		/// Gets the output at the given address.
		/// </summary>
		/// <param name="output"></param>
		/// <returns></returns>
		public override ConnectorInfo GetOutput(int output)
		{
			int index = output - 1;

			CardDevice card;
			if (!Parent.GetInternalCards().TryElementAt(index, out card))
				throw new ArgumentOutOfRangeException("output");

			return new ConnectorInfo(output, eConnectionType.Audio | eConnectionType.Video);
		}

		/// <summary>
		/// Returns true if the source contains an output at the given address.
		/// </summary>
		/// <param name="output"></param>
		/// <returns></returns>
		public override bool ContainsOutput(int output)
		{
			return output >= 1 && output <= Parent.GetInternalCards().Count();
		}

		/// <summary>
		/// Returns the outputs.
		/// </summary>
		/// <returns></returns>
		public override IEnumerable<ConnectorInfo> GetOutputs()
		{
			int address = 0;
			return Parent.GetInternalCards().Select(card => new ConnectorInfo(address++, eConnectionType.Audio | eConnectionType.Video));
		}

		/// <summary>
		/// Returns true if the device is actively transmitting on the given output.
		/// This is NOT the same as sending video, since some devices may send an
		/// idle signal by default.
		/// </summary>
		/// <param name="output"></param>
		/// <param name="type"></param>
		/// <returns></returns>
		public override bool GetActiveTransmissionState(int output, eConnectionType type)
		{
			IRouteSwitcherControl control = GetParentSwitcherControl();
			return control != null && control.GetActiveTransmissionState(output, type);
		}

		#endregion

		#region Parent Callbacks

		/// <summary>
		/// Subscribe to the parent events.
		/// </summary>
		/// <param name="parent"></param>
		protected override void Subscribe(TParent parent)
		{
			base.Subscribe(parent);

			parent.OnCardChanged += ParentOnCardChanged;
		}

		/// <summary>
		/// Unsubscribe from the parent events.
		/// </summary>
		/// <param name="parent"></param>
		protected override void Unsubscribe(TParent parent)
		{
			base.Unsubscribe(parent);

			parent.OnCardChanged -= ParentOnCardChanged;
		}

		/// <summary>
		/// Called when the wrapped card changes.
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="card"></param>
		private void ParentOnCardChanged(ICardAdapter sender, object card)
		{
			Unsubscribe(m_Card);

			m_Card = card;

			Subscribe(m_Card);
		}

		[CanBeNull]
		private IRouteSwitcherControl GetParentSwitcherControl()
		{
// ReSharper disable once CompareNonConstrainedGenericWithNull
			if (Parent == null || Parent.Switcher == null)
				return null;
			return Parent.Switcher.Controls.GetControl<IRouteSwitcherControl>();
		}

		#endregion

		#region Card Callbacks

		/// <summary>
		/// Subscribe to the card events.
		/// </summary>
		/// <param name="card"></param>
		private void Subscribe(object card)
		{
			IRouteSwitcherControl control = GetParentSwitcherControl();
			if (control == null)
				return;

			control.OnActiveTransmissionStateChanged += ControlOnActiveTransmissionStateChanged;
		}

		/// <summary>
		/// Unsubscribe from the card events.
		/// </summary>
		/// <param name="card"></param>
		private void Unsubscribe(object card)
		{
			IRouteSwitcherControl control = GetParentSwitcherControl();
			if (control == null)
				return;

			control.OnActiveTransmissionStateChanged -= ControlOnActiveTransmissionStateChanged;
		}

		private void ControlOnActiveTransmissionStateChanged(object sender, TransmissionStateEventArgs args)
		{
			if (GetOutputs().Select(o => o.Address).Contains(args.Output))
				OnActiveTransmissionStateChanged.Raise(this, new TransmissionStateEventArgs(args));
		}

		#endregion
	}
}

#endif
