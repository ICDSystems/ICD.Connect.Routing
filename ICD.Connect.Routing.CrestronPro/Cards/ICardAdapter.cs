#if SIMPLSHARP
using Crestron.SimplSharpPro.DM.Cards;
#endif
using ICD.Connect.Devices;

namespace ICD.Connect.Routing.CrestronPro.Cards
{
#if SIMPLSHARP
	public delegate void CardChangeCallback(ICardAdapter sender, CardDevice card);
#endif

	public interface ICardAdapter : IDevice
	{
#if SIMPLSHARP
		event CardChangeCallback OnCardChanged;

		/// <summary>
		/// Gets the wrapped internal card.
		/// </summary>
		CardDevice Card { get; }
#endif
	}
}
