using ICD.Connect.Devices;

namespace ICD.Connect.Routing.CrestronPro.Cards
{
	public interface ICardSettings : IDeviceSettings
	{
		int? CardNumber { get; set; }

		int? SwitcherId { get; set; }
	}

    public interface IInputCardSettings : ICardSettings
    {
        byte? CresnetId { get; set; }
    }

    public interface IOutputCardSettings : ICardSettings
    {
        
    }
}