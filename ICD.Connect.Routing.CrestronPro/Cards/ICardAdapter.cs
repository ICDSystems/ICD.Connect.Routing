using System.Collections.Generic;
using ICD.Connect.Devices;
using ICD.Connect.Routing.CrestronPro.DigitalMedia;
#if !NETSTANDARD
using Crestron.SimplSharpPro.DM.Cards;
#endif

namespace ICD.Connect.Routing.CrestronPro.Cards
{
#if !NETSTANDARD
	public delegate void CardChangeCallback(ICardAdapter sender, object card);
#endif

	public interface ICardAdapter : IDevice
	{
#if !NETSTANDARD
		event CardChangeCallback OnCardChanged;

		object Card { get; }

		ICrestronSwitchAdapter Switcher { get; }
#endif
	}

	public interface IInputCardAdapter : ICardAdapter
	{
#if !NETSTANDARD
		/// <summary>
		/// Gets the wrapped internal card.
		/// </summary>
		new CardDevice Card { get; }
#endif
	}

	public interface IOutputCardAdapter : ICardAdapter
	{
#if !NETSTANDARD
		/// <summary>
		/// Gets the wrapped internal card.
		/// </summary>
		new DmcOutputSingle Card { get; }

		IEnumerable<CardDevice> GetInternalCards();
#endif
	}
}
