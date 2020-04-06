using ICD.Connect.Devices.Mock;

namespace ICD.Connect.Routing.Mock.Midpoint
{
	public sealed class MockSplitterDevice : AbstractMockDevice<MockSplitterDeviceSettings>
	{
		/// <summary>
		/// Constructor.
		/// </summary>
		public MockSplitterDevice()
		{
			Controls.Add(new MockRouteSplitterControl(this, 0));
		}
	}
}
