using ICD.Common.Properties;
using ICD.Connect.Devices;
using ICD.Connect.Routing.CrestronPro.DigitalMedia;
using ICD.Connect.Settings.Originators;

namespace ICD.Connect.Routing.CrestronPro.Cards
{
#if !NETSTANDARD
	public abstract class AbstractCardAdapterBase<TCard, TSettings> : AbstractDevice<TSettings>, ICardAdapter
#else
    public abstract class AbstractCardAdapterBase<TSettings> : AbstractDevice<TSettings>, ICardAdapter
#endif
		where TSettings : ICardSettings, new()
	{
#if !NETSTANDARD
		public event CardChangeCallback OnCardChanged;

		private TCard m_Card;
#endif

		private int? m_CardNumber;
		private int? m_SwitcherId;

		/// <summary>
		/// Returns the Card Number
		/// </summary>
		[PublicAPI]
		public int? CardNumber { get { return m_CardNumber; } protected set { m_CardNumber = value; } }

		/// <summary>
		/// Returns the Switcher ID
		/// </summary>
		[PublicAPI]
		public int? SwitcherId { get { return m_SwitcherId; } protected set { m_SwitcherId = value; } }

#if !NETSTANDARD
		/// <summary>
		/// Gets the wrapped internal card.
		/// </summary>
		[PublicAPI]
		public TCard Card
		{
			get { return m_Card; }
			protected set
			{
// ReSharper disable CompareNonConstrainedGenericWithNull
				if (value == null && m_Card == null)
					return;
				if (value != null && value.Equals(m_Card))
					return;
// ReSharper restore CompareNonConstrainedGenericWithNull

				m_Card = value;

				CardChangeCallback handler = OnCardChanged;
				if (handler != null)
					handler(this, m_Card);
			}
		}
#endif

		/// <summary>
		/// Returns the Switcher
		/// </summary>
		[PublicAPI]
		[CanBeNull]
		public ICrestronSwitchAdapter Switcher
		{
			get
			{
				if (SwitcherId == null)
					return null;

				IOriginator switcher;
				return Core.Originators.TryGetChild((int)SwitcherId, out switcher)
					? switcher as ICrestronSwitchAdapter
					: null;
			}
		}

#if !NETSTANDARD
		/// <summary>
		/// Gets the wrapped internal card.
		/// </summary>
		object ICardAdapter.Card { get { return Card; } }
#endif

#if !!NETSTANDARD
		protected override bool GetIsOnlineStatus()
		{
			return false;
		}
#endif

		/// <summary>
		/// Release resources.
		/// </summary>
		protected override void DisposeFinal(bool disposing)
		{
#if !NETSTANDARD
			OnCardChanged = null;
#endif
			base.DisposeFinal(disposing);
		}
	}
}
