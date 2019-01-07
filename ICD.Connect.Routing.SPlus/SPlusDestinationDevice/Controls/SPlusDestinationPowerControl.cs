using ICD.Connect.Devices.Controls;

namespace ICD.Connect.Routing.SPlus.SPlusDestinationDevice.Controls
{
	public sealed class SPlusDestinationPowerControl : AbstractPowerDeviceControl<Device.SPlusDestinationDevice>
	{
		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="parent"></param>
		/// <param name="id"></param>
		public SPlusDestinationPowerControl(Device.SPlusDestinationDevice parent, int id) : base(parent, id)
		{
		}

		/// <summary>
		/// Powers on the device.
		/// </summary>
		public override void PowerOn()
		{
			Parent.PowerOn();
		}

		/// <summary>
		/// Powers off the device.
		/// </summary>
		public override void PowerOff()
		{
			Parent.PowerOff();
		}

		/// <summary>
		/// Sets the power state from the device
		/// </summary>
		/// <param name="isPowered"></param>
		internal void SetPowerStateFeedback(bool isPowered)
		{
			IsPowered = isPowered;
		}
	}
}