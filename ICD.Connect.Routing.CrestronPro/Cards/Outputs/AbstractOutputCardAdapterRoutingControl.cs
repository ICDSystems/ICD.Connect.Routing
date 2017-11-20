using ICD.Common.Utils.Extensions;
#if SIMPLSHARP
using System;
using System.Collections.Generic;
using System.Linq;
using Crestron.SimplSharpPro.DM.Cards;
using ICD.Common.Properties;
using ICD.Connect.Routing.Connections;
using ICD.Connect.Routing.Controls;
using ICD.Connect.Routing.EventArguments;

namespace ICD.Connect.Routing.CrestronPro.Cards.Outputs
{
    public abstract class AbstractOutputCardAdapterRoutingControl<TParent> : AbstractRouteSourceControl<TParent>
        where TParent : IOutputCardAdapter
    {
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
            Subscribe(parent);
        }

        /// <summary>
        /// Override to release resources.
        /// </summary>
        /// <param name="disposing"></param>
        protected override void DisposeFinal(bool disposing)
        {
            base.DisposeFinal(disposing);

            Unsubscribe(Parent);
            Unsubscribe(m_Card);
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
            Unsubscribe(m_Card);

            m_Card = card;

            Subscribe(m_Card);
        }

        /// <summary>
        /// Returns the outputs.
        /// </summary>
        /// <returns></returns>
        public override IEnumerable<ConnectorInfo> GetOutputs()
        {
            CardDevice[] cards = Parent.GetInternalCards().ToArray();

            for (int index = 0; index < cards.Length; index++)
                yield return new ConnectorInfo(index + 1, eConnectionType.Audio | eConnectionType.Video);
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

        [CanBeNull]
        private IRouteSwitcherControl GetParentSwitcherControl()
        {
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
