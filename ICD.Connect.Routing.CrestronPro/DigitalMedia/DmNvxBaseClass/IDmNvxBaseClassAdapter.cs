using ICD.Connect.Routing.CrestronPro.DigitalMedia.Dm100xStrBase;

namespace ICD.Connect.Routing.CrestronPro.DigitalMedia.DmNvxBaseClass
{
	public interface IDmNvxBaseClassAdapter : IDm100XStrBaseAdapter
	{
		/// <summary>
		/// Gets the configured device mode (i.e. Transmit or Receive)
		/// </summary>
		eDeviceMode DeviceMode { get; }

		/// <summary>
		/// Configures the current device mode.
		/// </summary>
		/// <param name="deviceMode"></param>
		void SetDeviceMode(eDeviceMode deviceMode);
	}
}