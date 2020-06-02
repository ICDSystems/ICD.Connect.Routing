using System;
using ICD.Connect.Devices.Controls;
using ICD.Connect.Devices.Mock;
using ICD.Connect.Settings;

namespace ICD.Connect.Routing.Mock.Midpoint
{
	public sealed class MockSplitterDevice : AbstractMockDevice<MockSplitterDeviceSettings>
	{
		/// <summary>
		/// Override to add controls to the device.
		/// </summary>
		/// <param name="settings"></param>
		/// <param name="factory"></param>
		/// <param name="addControl"></param>
		protected override void AddControls(MockSplitterDeviceSettings settings, IDeviceFactory factory, Action<IDeviceControl> addControl)
		{
			base.AddControls(settings, factory, addControl);

			addControl(new MockRouteSplitterControl(this, 0));
		}
	}
}
