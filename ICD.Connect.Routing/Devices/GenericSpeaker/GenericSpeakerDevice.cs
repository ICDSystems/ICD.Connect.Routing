using ICD.Connect.Devices;

namespace ICD.Connect.Routing.Devices.GenericSpeaker
{
	public sealed class GenericSpeakerDevice : AbstractDevice<GenericSpeakerDeviceSettings>
	{
		/// <summary>
		/// Constructor.
		/// </summary>
		public GenericSpeakerDevice()
		{
			Controls.Add(new GenericSpeakerDestinationControl(this, 0));
		}

		/// <summary>
		/// Gets the current online status of the device.
		/// </summary>
		/// <returns></returns>
		protected override bool GetIsOnlineStatus()
		{
			return true;
		}
	}
}
