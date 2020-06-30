using System;
using ICD.Connect.Devices;
using ICD.Connect.Devices.Controls;
using ICD.Connect.Settings;

namespace ICD.Connect.Routing.Devices.Streaming
{
	public sealed class MockStreamDestinationDevice : AbstractDevice<MockStreamDestinationDeviceSettings>
	{
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
		protected override void AddControls(MockStreamDestinationDeviceSettings settings, IDeviceFactory factory, Action<IDeviceControl> addControl)
		{
			base.AddControls(settings, factory, addControl);

			addControl(new MockStreamDestinationDeviceRoutingControl(this, 0));
		}
	}
}
