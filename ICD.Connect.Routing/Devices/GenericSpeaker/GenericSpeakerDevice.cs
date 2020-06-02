using System;
using ICD.Connect.Devices;
using ICD.Connect.Devices.Controls;
using ICD.Connect.Settings;

namespace ICD.Connect.Routing.Devices.GenericSpeaker
{
	public sealed class GenericSpeakerDevice : AbstractDevice<GenericSpeakerDeviceSettings>
	{
		/// <summary>
		/// Gets the current online status of the device.
		/// </summary>
		/// <returns></returns>
		protected override bool GetIsOnlineStatus()
		{
			return true;
		}

		/// <summary>
		/// Override to add controls to the device.
		/// </summary>
		/// <param name="settings"></param>
		/// <param name="factory"></param>
		/// <param name="addControl"></param>
		protected override void AddControls(GenericSpeakerDeviceSettings settings, IDeviceFactory factory, Action<IDeviceControl> addControl)
		{
			base.AddControls(settings, factory, addControl);

			addControl(new GenericSpeakerDestinationControl(this, 0));
		}
	}
}
