using ICD.Connect.Devices;

namespace ICD.Connect.Routing.CrestronPro.Cards
{
	public interface ICardSettings : IDeviceSettings
	{
		byte? CresnetId { get; set; }

		int? CardNumber { get; set; }

		int? SwitcherId { get; set; }
	}
}