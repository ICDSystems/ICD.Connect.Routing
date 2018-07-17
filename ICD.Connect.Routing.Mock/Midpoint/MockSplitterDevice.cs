using ICD.Connect.Devices;

namespace ICD.Connect.Routing.Mock.Midpoint
{
	public sealed class MockSplitterDevice : AbstractDevice<MockSplitterDeviceSettings>
	{
		/// <summary>
		/// Constructor.
		/// </summary>
		public MockSplitterDevice()
		{
			Controls.Add(new MockRouteSplitterControl(this, 0));
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
