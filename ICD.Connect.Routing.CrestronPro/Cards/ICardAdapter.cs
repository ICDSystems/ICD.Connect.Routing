using Crestron.SimplSharpPro.DM.Cards;
using ICD.Connect.Devices;

namespace ICD.Connect.Routing.CrestronPro.Cards
{
	public delegate void CardChangeCallback(ICardAdapter sender, CardDevice card);

	public interface ICardAdapter : IDevice
	{
		event CardChangeCallback OnCardChanged;

		/// <summary>
		/// Gets the wrapped internal card.
		/// </summary>
		CardDevice Card { get; }
	}
}
