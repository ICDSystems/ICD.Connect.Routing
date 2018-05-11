using ICD.Common.Properties;
using ICD.Common.Utils.Services;
using ICD.Connect.Devices;
using ICD.Connect.Routing.CrestronPro.DigitalMedia;
using ICD.Connect.Settings.Cores;

namespace ICD.Connect.Routing.CrestronPro.Cards
{
#if SIMPLSHARP
	public abstract class AbstractCardAdapterBase<TCard, TSettings> : AbstractDevice<TSettings>, ICardAdapter
#else
    public abstract class AbstractCardAdapterBase<TSettings> : AbstractDevice<TSettings>, ICardAdapter
#endif
		where TSettings : ICardSettings, new()
	{
#if SIMPLSHARP
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

#if SIMPLSHARP
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
		public ICrestronSwitchAdapter Switcher
		{
			get
			{
				if (SwitcherId == null)
					return null;
				ICore core = ServiceProvider.GetService<ICore>();
				return core.Originators.GetChild<ICrestronSwitchAdapter>((int)SwitcherId);
			}
		}

#if SIMPLSHARP
		/// <summary>
		/// Gets the wrapped internal card.
		/// </summary>
		object ICardAdapter.Card { get { return Card; } }
#endif

#if !SIMPLSHARP
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
#if SIMPLSHARP
			OnCardChanged = null;
#endif
			base.DisposeFinal(disposing);
		}
	}
}
