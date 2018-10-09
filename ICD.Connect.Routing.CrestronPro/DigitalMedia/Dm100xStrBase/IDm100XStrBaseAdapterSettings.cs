using ICD.Connect.Devices;
using ICD.Connect.Routing.CrestronPro.DigitalMedia.DmXioDirectorBase;
using ICD.Connect.Settings.Attributes.SettingsProperties;

namespace ICD.Connect.Routing.CrestronPro.DigitalMedia.Dm100xStrBase
{
	public interface IDm100XStrBaseAdapterSettings : IDeviceSettings
	{
		uint? EthernetId { get; set; }

		uint? EndpointId { get; set; }

		[OriginatorIdSettingsProperty(typeof(IDmXioDirectorBaseAdapter))]
		int? DirectorId { get; set; }

		uint? DomainId { get; set; }

		/// <summary>
		/// Returns true if the settings have been configured for receive.
		/// </summary>
		bool IsReceiver { get; }
	}
}
