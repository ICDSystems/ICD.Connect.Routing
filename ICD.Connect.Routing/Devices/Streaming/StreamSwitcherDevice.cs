using ICD.Connect.Devices;

namespace ICD.Connect.Routing.Devices.Streaming
{
	public sealed class StreamSwitcherDevice : AbstractDevice<StreamSwitcherDeviceSettings>
	{
		protected override bool GetIsOnlineStatus()
		{
			return true;
		}
	}
}
