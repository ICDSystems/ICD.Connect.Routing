using System.Collections.Generic;
using ICD.Connect.API.Commands;
using ICD.Connect.API.Nodes;
using ICD.Connect.Devices;

namespace ICD.Connect.Routing.Devices.Streaming
{
	public sealed class MockStreamDestinationDevice : AbstractDevice<MockStreamDestinationDeviceSettings>
	{
		public MockStreamDestinationDevice()
		{
			Controls.Add(new MockStreamDestinationDeviceRoutingControl(this, 0));
		}

		protected override bool GetIsOnlineStatus()
		{
			return true;
		}
	}
}
