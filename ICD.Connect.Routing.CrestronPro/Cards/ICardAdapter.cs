using System.Collections.Generic;
using ICD.Connect.Routing.CrestronPro.DigitalMedia;
#if SIMPLSHARP
using Crestron.SimplSharpPro.DM.Cards;
#endif
using ICD.Connect.Devices;

namespace ICD.Connect.Routing.CrestronPro.Cards
{
#if SIMPLSHARP
	public delegate void CardChangeCallback(ICardAdapter sender, object card);
#endif

    public interface ICardAdapter : IDevice
    {
#if SIMPLSHARP
        event CardChangeCallback OnCardChanged;

        object Card { get; }

        IDmSwitcherAdapter Switcher { get; }
#endif
    }

    public interface IInputCardAdapter : ICardAdapter
	{
#if SIMPLSHARP
		/// <summary>
		/// Gets the wrapped internal card.
		/// </summary>
		new CardDevice Card { get; }
#endif
	}

    public interface IOutputCardAdapter : ICardAdapter
    {
#if SIMPLSHARP
        /// <summary>
        /// Gets the wrapped internal card.
        /// </summary>
        new DmcOutputSingle Card { get; }

        IEnumerable<CardDevice> GetInternalCards();
#endif
    }
}
